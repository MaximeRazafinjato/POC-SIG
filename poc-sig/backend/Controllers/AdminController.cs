using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PocSig.Infrastructure;

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
            _logger.LogInformation("Loading all demo data...");

            var demoFiles = new[]
            {
                ("paris_monuments.geojson", "Monuments de Paris"),
                ("paris_parks.geojson", "Parcs et Jardins"),
                ("paris_metro.geojson", "Lignes de Métro"),
                ("paris_museums.geojson", "Musées")
            };

            var results = new List<object>();

            foreach (var (fileName, layerName) in demoFiles)
            {
                try
                {
                    // Import the file directly with automatic layer creation
                    var importLogger = _loggerFactory.CreateLogger<PocSig.ETL.ImportGeoJsonCommand>();
                    var importCommand = new PocSig.ETL.ImportGeoJsonCommand(_context, importLogger);
                    var result = await importCommand.ExecuteAsync(null, layerName, fileName);

                    results.Add(new
                    {
                        fileName,
                        layerName,
                        success = true,
                        message = result
                    });

                    _logger.LogInformation("Successfully imported {FileName} into layer '{LayerName}'",
                        fileName, layerName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to import {FileName}", fileName);
                    results.Add(new
                    {
                        fileName,
                        layerName,
                        success = false,
                        error = ex.Message
                    });
                }
            }

            return Ok(new
            {
                success = true,
                message = "Demo data loading completed",
                results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading demo data");
            return StatusCode(500, new { error = "Failed to load demo data", details = ex.Message });
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