using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PocSig.Infrastructure;
using NetTopologySuite.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using System.Text;
using System.Text.Json;
using System.Diagnostics;

namespace PocSig.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ExportController> _logger;

    public ExportController(AppDbContext context, ILogger<ExportController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("{layerId}/geojson")]
    public async Task<IActionResult> ExportGeoJson(
        [FromRoute] int layerId,
        [FromQuery] string? bbox = null,
        [FromQuery] string? operation = null,
        [FromQuery] double? bufferMeters = null,
        [FromQuery] DateTime? validFrom = null,
        [FromQuery] DateTime? validTo = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var query = _context.Features
                .Include(f => f.Layer)
                .Where(f => f.LayerId == layerId);

            if (validFrom.HasValue)
            {
                query = query.Where(f => f.ValidFromUtc >= validFrom.Value);
            }

            if (validTo.HasValue)
            {
                query = query.Where(f => !f.ValidToUtc.HasValue || f.ValidToUtc.Value <= validTo.Value);
            }

            if (!string.IsNullOrEmpty(bbox))
            {
                var parts = bbox.Split(',');
                if (parts.Length == 4)
                {
                    var minX = double.Parse(parts[0]);
                    var minY = double.Parse(parts[1]);
                    var maxX = double.Parse(parts[2]);
                    var maxY = double.Parse(parts[3]);

                    var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
                    var filterGeometry = geometryFactory.CreatePolygon([
                        new Coordinate(minX, minY),
                        new Coordinate(maxX, minY),
                        new Coordinate(maxX, maxY),
                        new Coordinate(minX, maxY),
                        new Coordinate(minX, minY)
                    ]);

                    if (bufferMeters.HasValue && bufferMeters.Value > 0)
                    {
                        var bufferDegrees = bufferMeters.Value / 111320.0;
                        filterGeometry = (Polygon)filterGeometry.Buffer(bufferDegrees);
                    }

                    if (operation == "within")
                    {
                        query = query.Where(f => f.Geometry != null && f.Geometry.Within(filterGeometry));
                    }
                    else
                    {
                        query = query.Where(f => f.Geometry != null && f.Geometry.Intersects(filterGeometry));
                    }
                }
            }

            var features = await query.ToListAsync();

            var geoJsonWriter = new GeoJsonWriter();
            var featureCollection = new FeatureCollection();

            foreach (var feature in features)
            {
                var properties = new AttributesTable();
                properties.Add("id", feature.Id);
                properties.Add("layerId", feature.LayerId);
                properties.Add("layerName", feature.Layer.Name);
                properties.Add("validFromUtc", feature.ValidFromUtc.ToString("O"));
                properties.Add("validToUtc", feature.ValidToUtc?.ToString("O"));

                if (!string.IsNullOrEmpty(feature.PropertiesJson))
                {
                    try
                    {
                        var customProps = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(feature.PropertiesJson);
                        if (customProps != null)
                        {
                            foreach (var prop in customProps)
                            {
                                properties.Add(prop.Key, prop.Value.ToString());
                            }
                        }
                    }
                    catch { }
                }

                featureCollection.Add(new Feature(feature.Geometry, properties));
            }

            var geoJson = geoJsonWriter.Write(featureCollection);

            stopwatch.Stop();
            _logger.LogInformation("Exported {Count} features to GeoJSON in {ElapsedMs}ms", features.Count, stopwatch.ElapsedMilliseconds);

            var fileName = $"export_layer_{layerId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.geojson";
            return File(Encoding.UTF8.GetBytes(geoJson), "application/geo+json", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting GeoJSON");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{layerId}/csv")]
    public async Task<IActionResult> ExportCsv(
        [FromRoute] int layerId,
        [FromQuery] string? bbox = null,
        [FromQuery] string? operation = null,
        [FromQuery] double? bufferMeters = null,
        [FromQuery] DateTime? validFrom = null,
        [FromQuery] DateTime? validTo = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var query = _context.Features
                .Include(f => f.Layer)
                .Where(f => f.LayerId == layerId);

            if (validFrom.HasValue)
            {
                query = query.Where(f => f.ValidFromUtc >= validFrom.Value);
            }

            if (validTo.HasValue)
            {
                query = query.Where(f => !f.ValidToUtc.HasValue || f.ValidToUtc.Value <= validTo.Value);
            }

            if (!string.IsNullOrEmpty(bbox))
            {
                var parts = bbox.Split(',');
                if (parts.Length == 4)
                {
                    var minX = double.Parse(parts[0]);
                    var minY = double.Parse(parts[1]);
                    var maxX = double.Parse(parts[2]);
                    var maxY = double.Parse(parts[3]);

                    var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
                    var filterGeometry = geometryFactory.CreatePolygon([
                        new Coordinate(minX, minY),
                        new Coordinate(maxX, minY),
                        new Coordinate(maxX, maxY),
                        new Coordinate(minX, maxY),
                        new Coordinate(minX, minY)
                    ]);

                    if (bufferMeters.HasValue && bufferMeters.Value > 0)
                    {
                        var bufferDegrees = bufferMeters.Value / 111320.0;
                        filterGeometry = (Polygon)filterGeometry.Buffer(bufferDegrees);
                    }

                    if (operation == "within")
                    {
                        query = query.Where(f => f.Geometry != null && f.Geometry.Within(filterGeometry));
                    }
                    else
                    {
                        query = query.Where(f => f.Geometry != null && f.Geometry.Intersects(filterGeometry));
                    }
                }
            }

            var features = await query.ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("id,layerId,layerName,centroidLat,centroidLon,geometryType,area,validFromUtc,validToUtc,properties");

            foreach (var feature in features)
            {
                var centroid = feature.Geometry.Centroid;
                var area = feature.Geometry.Area * 111320.0 * 111320.0;

                var row = new List<string>
                {
                    feature.Id.ToString(),
                    feature.LayerId.ToString(),
                    $"\"{feature.Layer.Name}\"",
                    centroid.Y.ToString("F6"),
                    centroid.X.ToString("F6"),
                    feature.Geometry.GeometryType,
                    area.ToString("F2"),
                    feature.ValidFromUtc.ToString("O"),
                    feature.ValidToUtc?.ToString("O") ?? "",
                    $"\"{feature.PropertiesJson?.Replace("\"", "\"\"")}\"" ?? ""
                };

                csv.AppendLine(string.Join(",", row));
            }

            stopwatch.Stop();
            _logger.LogInformation("Exported {Count} features to CSV in {ElapsedMs}ms", features.Count, stopwatch.ElapsedMilliseconds);

            var fileName = $"export_layer_{layerId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting CSV");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}