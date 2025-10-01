using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PocSig.Infrastructure;
using PocSig.Domain.Entities;
using NetTopologySuite.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using System.Text.Json;
using System.Diagnostics;

namespace PocSig.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeaturesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<FeaturesController> _logger;
    private readonly IMemoryCache _cache;

    public FeaturesController(AppDbContext context, ILogger<FeaturesController> logger, IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    [HttpGet("{layerId}")]
    public async Task<ActionResult<object>> GetFeatures(
        [FromRoute] int layerId,
        [FromQuery] string? bbox = null,
        [FromQuery] string? operation = null,
        [FromQuery] double? bufferMeters = null,
        [FromQuery] DateTime? validFrom = null,
        [FromQuery] DateTime? validTo = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
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

            Geometry? filterGeometry = null;
            if (!string.IsNullOrEmpty(bbox))
            {
                _logger.LogInformation("Applying bbox filter: {Bbox}", bbox);
                var parts = bbox.Split(',');
                if (parts.Length == 4)
                {
                    var minX = double.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                    var minY = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                    var maxX = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                    var maxY = double.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);

                    // Handle case where bbox is a single point (expand slightly to create a valid box)
                    if (minX == maxX && minY == maxY)
                    {
                        var epsilon = 0.0001; // About 11 meters
                        minX -= epsilon;
                        minY -= epsilon;
                        maxX += epsilon;
                        maxY += epsilon;
                    }

                    var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
                    filterGeometry = geometryFactory.CreatePolygon([
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
                        _logger.LogInformation("Using 'within' spatial operation");
                        query = query.Where(f => f.Geometry != null && f.Geometry.Within(filterGeometry));
                    }
                    else
                    {
                        _logger.LogInformation("Using 'intersects' spatial operation");
                        query = query.Where(f => f.Geometry != null && f.Geometry.Intersects(filterGeometry));
                    }
                }
            }

            var totalCount = await query.CountAsync();
            var skip = (page - 1) * pageSize;

            var features = await query
                .OrderBy(f => f.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var geoJsonWriter = new GeoJsonWriter();
            var featureCollection = new FeatureCollection();

            foreach (var feature in features)
            {
                var properties = new AttributesTable();
                properties.Add("id", feature.Id);
                properties.Add("layerId", feature.LayerId);
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
            _logger.LogInformation("Retrieved {Count} features from layer {LayerId} in {ElapsedMs}ms (Total: {Total}, Page: {Page})",
                features.Count, layerId, stopwatch.ElapsedMilliseconds, totalCount, page);

            return Ok(new
            {
                type = "FeatureCollection",
                features = JsonSerializer.Deserialize<JsonElement>(geoJson).GetProperty("features"),
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                queryTimeMs = stopwatch.ElapsedMilliseconds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving features");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{layerId}/stats")]
    public async Task<ActionResult<object>> GetStats(
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
            var query = _context.Features.Where(f => f.LayerId == layerId);

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
                    var minX = double.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                    var minY = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                    var maxX = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                    var maxY = double.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);

                    // Handle case where bbox is a single point (expand slightly to create a valid box)
                    if (minX == maxX && minY == maxY)
                    {
                        var epsilon = 0.0001; // About 11 meters
                        minX -= epsilon;
                        minY -= epsilon;
                        maxX += epsilon;
                        maxY += epsilon;
                    }

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

            var count = await query.CountAsync();

            var totalAreaSqMeters = await query
                .Where(f => f.Geometry.GeometryType == "Polygon" || f.Geometry.GeometryType == "MultiPolygon")
                .SumAsync(f => f.Geometry.Area * 111320.0 * 111320.0);

            stopwatch.Stop();
            _logger.LogInformation("Calculated stats for layer {LayerId} in {ElapsedMs}ms", layerId, stopwatch.ElapsedMilliseconds);

            return Ok(new
            {
                count,
                totalAreaSqMeters,
                totalAreaHectares = totalAreaSqMeters / 10000,
                queryTimeMs = stopwatch.ElapsedMilliseconds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating stats");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateFeature([FromBody] CreateFeatureRequest request)
    {
        try
        {
            var wktReader = new WKTReader();
            var geometry = wktReader.Read(request.WKT);
            geometry.SRID = 4326;

            var feature = new FeatureEntity
            {
                LayerId = request.LayerId,
                PropertiesJson = request.PropertiesJson,
                Geometry = geometry,
                ValidFromUtc = request.ValidFromUtc ?? DateTime.UtcNow,
                ValidToUtc = request.ValidToUtc
            };

            _context.Features.Add(feature);
            await _context.SaveChangesAsync();

            return Ok(new { id = feature.Id, message = "Feature created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating feature");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class CreateFeatureRequest
{
    public int LayerId { get; set; }
    public string WKT { get; set; } = string.Empty;
    public string? PropertiesJson { get; set; }
    public DateTime? ValidFromUtc { get; set; }
    public DateTime? ValidToUtc { get; set; }
}