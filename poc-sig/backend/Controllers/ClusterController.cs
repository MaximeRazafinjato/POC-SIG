using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PocSig.Infrastructure;
using NetTopologySuite.Geometries;
using System.Text.Json;

namespace PocSig.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClusterController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ClusterController> _logger;

    public ClusterController(AppDbContext context, ILogger<ClusterController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("{layerId}")]
    public async Task<ActionResult<object>> GetClusters(
        [FromRoute] int layerId,
        [FromQuery] double? zoom = null,
        [FromQuery] string? bbox = null,
        [FromQuery] int clusterRadius = 50)
    {
        try
        {
            // Get the viewport bounds
            Geometry? viewportGeometry = null;
            if (!string.IsNullOrEmpty(bbox))
            {
                var parts = bbox.Split(',');
                if (parts.Length == 4)
                {
                    var minX = double.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                    var minY = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                    var maxX = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                    var maxY = double.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);

                    var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
                    viewportGeometry = geometryFactory.CreatePolygon([
                        new Coordinate(minX, minY),
                        new Coordinate(maxX, minY),
                        new Coordinate(maxX, maxY),
                        new Coordinate(minX, maxY),
                        new Coordinate(minX, minY)
                    ]);
                }
            }

            // Query features in the viewport
            var query = _context.Features
                .Where(f => f.LayerId == layerId && f.Geometry != null);

            if (viewportGeometry != null)
            {
                query = query.Where(f => f.Geometry.Intersects(viewportGeometry));
            }

            var features = await query.ToListAsync();

            // If zoom level is high enough or few features, return individual features
            if (zoom >= 12 || features.Count <= 100)
            {
                return Ok(new
                {
                    type = "FeatureCollection",
                    features = features.Select(f => new
                    {
                        type = "Feature",
                        properties = new
                        {
                            id = f.Id,
                            cluster = false,
                            properties = f.PropertiesJson != null ?
                                JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(f.PropertiesJson) :
                                new Dictionary<string, JsonElement>()
                        },
                        geometry = new
                        {
                            type = f.Geometry.GeometryType,
                            coordinates = GetCoordinates(f.Geometry)
                        }
                    })
                });
            }

            // Perform clustering for points
            var clusters = new List<object>();
            var processedIndices = new HashSet<int>();
            var clusterDistanceDegrees = clusterRadius / 111320.0; // Convert meters to degrees (rough approximation)

            for (int i = 0; i < features.Count; i++)
            {
                if (processedIndices.Contains(i)) continue;

                var centerFeature = features[i];
                if (centerFeature.Geometry is not Point centerPoint)
                {
                    // Non-point features are not clustered
                    clusters.Add(new
                    {
                        type = "Feature",
                        properties = new
                        {
                            id = centerFeature.Id,
                            cluster = false,
                            properties = centerFeature.PropertiesJson != null ?
                                JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(centerFeature.PropertiesJson) :
                                new Dictionary<string, JsonElement>()
                        },
                        geometry = new
                        {
                            type = centerFeature.Geometry.GeometryType,
                            coordinates = GetCoordinates(centerFeature.Geometry)
                        }
                    });
                    processedIndices.Add(i);
                    continue;
                }

                var clusterMembers = new List<int> { i };
                processedIndices.Add(i);

                // Find nearby points
                for (int j = i + 1; j < features.Count; j++)
                {
                    if (processedIndices.Contains(j)) continue;

                    var testFeature = features[j];
                    if (testFeature.Geometry is Point testPoint)
                    {
                        var distance = centerPoint.Distance(testPoint);
                        if (distance <= clusterDistanceDegrees)
                        {
                            clusterMembers.Add(j);
                            processedIndices.Add(j);
                        }
                    }
                }

                if (clusterMembers.Count > 1)
                {
                    // Create cluster
                    var sumX = 0.0;
                    var sumY = 0.0;
                    foreach (var idx in clusterMembers)
                    {
                        var pt = features[idx].Geometry as Point;
                        sumX += pt.X;
                        sumY += pt.Y;
                    }

                    clusters.Add(new
                    {
                        type = "Feature",
                        properties = new
                        {
                            cluster = true,
                            point_count = clusterMembers.Count,
                            point_count_abbreviated = GetAbbreviatedCount(clusterMembers.Count)
                        },
                        geometry = new
                        {
                            type = "Point",
                            coordinates = new[] { sumX / clusterMembers.Count, sumY / clusterMembers.Count }
                        }
                    });
                }
                else
                {
                    // Single point
                    clusters.Add(new
                    {
                        type = "Feature",
                        properties = new
                        {
                            id = centerFeature.Id,
                            cluster = false,
                            properties = centerFeature.PropertiesJson != null ?
                                JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(centerFeature.PropertiesJson) :
                                new Dictionary<string, JsonElement>()
                        },
                        geometry = new
                        {
                            type = "Point",
                            coordinates = new[] { centerPoint.X, centerPoint.Y }
                        }
                    });
                }
            }

            _logger.LogInformation("Clustered {FeatureCount} features into {ClusterCount} clusters",
                features.Count, clusters.Count);

            return Ok(new
            {
                type = "FeatureCollection",
                features = clusters,
                metadata = new
                {
                    totalFeatures = features.Count,
                    clustersCreated = clusters.Count(c => ((dynamic)c).properties.cluster == true),
                    zoom = zoom
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clustering features for layer {LayerId}", layerId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private string GetAbbreviatedCount(int count)
    {
        if (count < 1000) return count.ToString();
        if (count < 10000) return $"{count / 1000.0:F1}k";
        return $"{count / 1000}k";
    }

    private object GetCoordinates(Geometry geometry)
    {
        return geometry switch
        {
            Point point => new[] { point.X, point.Y },
            LineString line => line.Coordinates.Select(c => new[] { c.X, c.Y }),
            Polygon polygon => new[] { polygon.ExteriorRing.Coordinates.Select(c => new[] { c.X, c.Y }) },
            _ => null
        };
    }
}