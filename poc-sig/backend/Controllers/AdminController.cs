using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PocSig.Infrastructure;
using System.Text.Json;

namespace PocSig.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminController> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public AdminController(AppDbContext context, ILogger<AdminController> logger, ILoggerFactory loggerFactory)
    {
        _context = context;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    [HttpPost("clean-database")]
    public async Task<IActionResult> CleanDatabase()
    {
        try
        {
            _logger.LogWarning("Starting database cleanup...");

            // Delete all features first (due to foreign key constraint)
            var featureCount = await _context.Features.CountAsync();
            _context.Features.RemoveRange(_context.Features);
            await _context.SaveChangesAsync();

            // Delete all layers
            var layerCount = await _context.Layers.CountAsync();
            _context.Layers.RemoveRange(_context.Layers);
            await _context.SaveChangesAsync();

            // Reset identity columns
            try
            {
                await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Features', RESEED, 0)");
                await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Layers', RESEED, 0)");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not reset identity columns");
            }

            _logger.LogInformation("Database cleaned successfully. Deleted {FeatureCount} features and {LayerCount} layers",
                featureCount, layerCount);

            return Ok(new
            {
                success = true,
                message = $"Database cleaned successfully. Deleted {featureCount} features and {layerCount} layers",
                deletedFeatures = featureCount,
                deletedLayers = layerCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning database");
            return StatusCode(500, new { error = "Failed to clean database", details = ex.Message });
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetDatabaseStats()
    {
        var featureCount = await _context.Features.CountAsync();
        var layerCount = await _context.Layers.CountAsync();

        var layerDetails = await _context.Layers
            .Select(l => new
            {
                l.Id,
                l.Name,
                FeatureCount = _context.Features.Count(f => f.LayerId == l.Id)
            })
            .ToListAsync();

        return Ok(new
        {
            totalFeatures = featureCount,
            totalLayers = layerCount,
            layers = layerDetails
        });
    }

    [HttpPost("load-demo-data")]
    public async Task<IActionResult> LoadDemoData()
    {
        try
        {
            _logger.LogInformation("Loading Grand Est water resources demo data...");

            // Use the new comprehensive Grand Est water resources data file with 500+ real data points
            var fileName = "grand_est_eau_complet.geojson";
            var results = new List<object>();

            try
            {
                // Import the comprehensive Grand Est water data file
                var importLogger = _loggerFactory.CreateLogger<PocSig.ETL.ImportGeoJsonCommand>();
                var importCommand = new PocSig.ETL.ImportGeoJsonCommand(_context, importLogger);

                // Import all features into a single layer
                var result = await importCommand.ExecuteAsync(null, "Ressources en eau - Grand Est", fileName);

                results.Add(new
                {
                    fileName,
                    layerName = "Ressources en eau - Grand Est",
                    success = true,
                    message = result
                });

                _logger.LogInformation("Successfully imported {FileName} with Grand Est water resources data", fileName);

                // Create additional specialized layers from the imported data
                await CreateSpecializedLayers();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import {FileName}", fileName);
                results.Add(new
                {
                    fileName,
                    layerName = "Ressources en eau - Grand Est",
                    success = false,
                    error = ex.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = "Grand Est water resources demo data loading completed",
                results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading demo data");
            return StatusCode(500, new { error = "Failed to load demo data", details = ex.Message });
        }
    }

    private async Task CreateSpecializedLayers()
    {
        try
        {
            // Group features by their layer property
            var features = await _context.Features
                .Where(f => f.Layer.Name == "Ressources en eau - Grand Est")
                .ToListAsync();

            var layerGroups = features.GroupBy(f =>
            {
                if (!string.IsNullOrEmpty(f.PropertiesJson))
                {
                    using var doc = JsonDocument.Parse(f.PropertiesJson);
                    if (doc.RootElement.TryGetProperty("layer", out var layerProp))
                    {
                        return layerProp.GetString();
                    }
                }
                return "Autres";
            });

            foreach (var group in layerGroups)
            {
                if (!string.IsNullOrEmpty(group.Key) && group.Key != "Autres")
                {
                    // Create a new layer for this category
                    var newLayer = new Domain.Entities.Layer
                    {
                        Name = group.Key,
                        Srid = 4326,
                        GeometryType = "Geometry",
                        CreatedUtc = DateTime.UtcNow,
                        UpdatedUtc = DateTime.UtcNow,
                        MetadataJson = JsonSerializer.Serialize(new
                        {
                            source = "Grand Est Water Resources",
                            category = group.Key,
                            importDate = DateTime.UtcNow
                        })
                    };

                    _context.Layers.Add(newLayer);
                    await _context.SaveChangesAsync();

                    // Copy features to the new layer
                    foreach (var feature in group)
                    {
                        var newFeature = new Domain.Entities.FeatureEntity
                        {
                            LayerId = newLayer.Id,
                            Geometry = feature.Geometry,
                            PropertiesJson = feature.PropertiesJson,
                            ValidFromUtc = feature.ValidFromUtc,
                            ValidToUtc = feature.ValidToUtc
                        };
                        _context.Features.Add(newFeature);
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Created specialized layer '{LayerName}' with {Count} features",
                        group.Key, group.Count());
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating specialized layers");
        }
    }

    [HttpPost("rename-default-layer")]
    public async Task<IActionResult> RenameDefaultLayer()
    {
        try
        {
            var defaultLayer = await _context.Layers.FirstOrDefaultAsync(l => l.Id == 1);
            if (defaultLayer != null)
            {
                defaultLayer.Name = "Toutes les couches";
                defaultLayer.MetadataJson = "{\"description\": \"Vue globale de toutes les couches du POC\"}";
                await _context.SaveChangesAsync();
                _logger.LogInformation("Renamed default layer to 'Toutes les couches'");
                return Ok(new { message = "Layer renamed successfully" });
            }
            return NotFound(new { message = "Default layer not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renaming default layer");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}