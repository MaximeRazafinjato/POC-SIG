using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using PocSig.Domain.Entities;
using PocSig.Infrastructure;
using System.Diagnostics;
using System.Text.Json;

namespace PocSig.ETL;

public class ImportGeoJsonCommand
{
    private readonly AppDbContext _context;
    private readonly ILogger<ImportGeoJsonCommand> _logger;

    public ImportGeoJsonCommand(AppDbContext context, ILogger<ImportGeoJsonCommand> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> ExecuteAsync(int? layerId, string? layerName, string? fileName = null)
    {
        var stopwatch = Stopwatch.StartNew();

        // Use the provided fileName or default to "sample.geojson"
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = "sample.geojson";
        }

        _logger.LogInformation("Importing GeoJSON file: {FileName}", fileName);
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"GeoJSON file not found: {filePath}");
        }

        _logger.LogInformation("Starting GeoJSON import from {FilePath}", filePath);

        var geoJsonText = await File.ReadAllTextAsync(filePath);
        var reader = new GeoJsonReader();

        FeatureCollection featureCollection;
        try
        {
            featureCollection = reader.Read<FeatureCollection>(geoJsonText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse GeoJSON");
            throw new InvalidOperationException($"Failed to parse GeoJSON: {ex.Message}");
        }

        Layer? layer = null;
        if (layerId.HasValue)
        {
            layer = await _context.Layers.FindAsync(layerId.Value);
            if (layer == null)
            {
                throw new ArgumentException($"Layer with ID {layerId.Value} not found");
            }
        }
        else
        {
            layer = new Layer
            {
                Name = layerName ?? $"Import_{DateTime.UtcNow:yyyyMMdd_HHmmss}",
                Srid = 4326,
                GeometryType = "Geometry",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    source = "GeoJSON Import",
                    importDate = DateTime.UtcNow,
                    fileName = Path.GetFileName(filePath)
                })
            };

            _context.Layers.Add(layer);
            await _context.SaveChangesAsync();
        }

        var importedFeatures = new List<FeatureEntity>();
        var batchSize = 100;

        foreach (var feature in featureCollection)
        {
            try
            {
                var geometry = feature.Geometry;

                if (geometry.SRID != 4326)
                {
                    if (geometry.SRID == 0)
                    {
                        geometry.SRID = 4326;
                    }
                    else
                    {
                        _logger.LogWarning("Feature has SRID {SRID}, converting to 4326", geometry.SRID);
                        var transform = new CoordinateTransform(geometry.SRID, 4326);
                        geometry = transform.Transform(geometry);
                    }
                }

                var propertiesDict = new Dictionary<string, object>();
                if (feature.Attributes != null)
                {
                    foreach (var attr in feature.Attributes.GetNames())
                    {
                        var value = feature.Attributes[attr];
                        if (value != null)
                        {
                            propertiesDict[attr] = value;
                        }
                    }
                }

                var featureEntity = new FeatureEntity
                {
                    LayerId = layer.Id,
                    Geometry = geometry,
                    PropertiesJson = JsonSerializer.Serialize(propertiesDict),
                    ValidFromUtc = GetValidFromDate(propertiesDict),
                    ValidToUtc = GetValidToDate(propertiesDict)
                };

                importedFeatures.Add(featureEntity);

                if (importedFeatures.Count >= batchSize)
                {
                    _context.Features.AddRange(importedFeatures);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Imported batch of {Count} features", importedFeatures.Count);
                    importedFeatures.Clear();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import feature");
            }
        }

        if (importedFeatures.Any())
        {
            _context.Features.AddRange(importedFeatures);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Imported final batch of {Count} features", importedFeatures.Count);
        }

        var totalImported = await _context.Features.CountAsync(f => f.LayerId == layer.Id);

        try
        {
            var rebuildIndexSql = @"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Features_Geometry' AND object_id = OBJECT_ID('Features'))
                BEGIN
                    ALTER INDEX IX_Features_Geometry ON Features REBUILD
                END";

            await _context.Database.ExecuteSqlRawAsync(rebuildIndexSql);
            _logger.LogInformation("Spatial index rebuilt successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to rebuild spatial index");
        }

        stopwatch.Stop();
        var message = $"Successfully imported {totalImported} features to layer '{layer.Name}' (ID: {layer.Id}) in {stopwatch.ElapsedMilliseconds}ms";
        _logger.LogInformation(message);

        return message;
    }

    private class CoordinateTransform
    {
        private readonly int _sourceSRID;
        private readonly int _targetSRID;

        public CoordinateTransform(int sourceSRID, int targetSRID)
        {
            _sourceSRID = sourceSRID;
            _targetSRID = targetSRID;
        }

        public Geometry Transform(Geometry geometry)
        {
            var cloned = geometry.Copy();
            cloned.SRID = _targetSRID;
            return cloned;
        }
    }

    private DateTime GetValidFromDate(Dictionary<string, object> properties)
    {
        if (properties.TryGetValue("validFrom", out var validFrom) && validFrom != null)
        {
            if (DateTime.TryParse(validFrom.ToString(), out var date))
                return date.ToUniversalTime();
        }
        if (properties.TryGetValue("year", out var year) && year != null)
        {
            if (int.TryParse(year.ToString(), out var yearInt))
                return new DateTime(yearInt, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }
        return DateTime.UtcNow;
    }

    private DateTime? GetValidToDate(Dictionary<string, object> properties)
    {
        if (properties.TryGetValue("validTo", out var validTo) && validTo != null)
        {
            if (DateTime.TryParse(validTo.ToString(), out var date))
                return date.ToUniversalTime();
        }
        if (properties.TryGetValue("endYear", out var endYear) && endYear != null)
        {
            if (int.TryParse(endYear.ToString(), out var yearInt))
                return new DateTime(yearInt, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        }
        return null;
    }
}