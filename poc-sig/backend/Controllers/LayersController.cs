using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PocSig.Infrastructure;
using PocSig.Domain.Entities;
using PocSig.ETL;
using System.Diagnostics;

namespace PocSig.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LayersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ImportGeoJsonCommand _importCommand;
    private readonly ILogger<LayersController> _logger;

    public LayersController(AppDbContext context, ImportGeoJsonCommand importCommand, ILogger<LayersController> logger)
    {
        _context = context;
        _importCommand = importCommand;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetLayers()
    {
        var stopwatch = Stopwatch.StartNew();

        var layers = await _context.Layers
            .Select(l => new
            {
                l.Id,
                l.Name,
                l.Srid,
                l.GeometryType,
                l.CreatedUtc,
                l.UpdatedUtc,
                l.MetadataJson,
                FeatureCount = l.Features.Count()
            })
            .ToListAsync();

        stopwatch.Stop();
        _logger.LogInformation("Retrieved {Count} layers in {ElapsedMs}ms", layers.Count, stopwatch.ElapsedMilliseconds);

        return Ok(layers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Layer>> GetLayer(int id)
    {
        var layer = await _context.Layers.FindAsync(id);
        if (layer == null)
        {
            return NotFound();
        }

        return Ok(layer);
    }

    [HttpPost]
    public async Task<ActionResult<Layer>> CreateLayer([FromBody] Layer layer)
    {
        layer.CreatedUtc = DateTime.UtcNow;
        layer.UpdatedUtc = DateTime.UtcNow;

        _context.Layers.Add(layer);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new layer: {LayerName} with ID: {LayerId}", layer.Name, layer.Id);

        return CreatedAtAction(nameof(GetLayer), new { id = layer.Id }, layer);
    }

    [HttpPost("import")]
    public async Task<ActionResult> ImportGeoJson([FromQuery] int? layerId, [FromQuery] string? layerName = "Imported", [FromQuery] string? fileName = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await _importCommand.ExecuteAsync(layerId, layerName, fileName);

            stopwatch.Stop();
            _logger.LogInformation("GeoJSON import completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

            return Ok(new
            {
                success = true,
                message = result,
                elapsedMs = stopwatch.ElapsedMilliseconds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during GeoJSON import");
            return StatusCode(500, new
            {
                success = false,
                message = ex.Message,
                elapsedMs = stopwatch.ElapsedMilliseconds
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLayer(int id)
    {
        var layer = await _context.Layers.FindAsync(id);
        if (layer == null)
        {
            return NotFound();
        }

        _context.Layers.Remove(layer);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted layer: {LayerName} with ID: {LayerId}", layer.Name, id);

        return NoContent();
    }
}