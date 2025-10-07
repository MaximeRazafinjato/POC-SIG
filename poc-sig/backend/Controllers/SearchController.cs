using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PocSig.DTOs;
using PocSig.Infrastructure;
using System.Globalization;

namespace PocSig.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SearchController> _logger;

    public SearchController(AppDbContext context, IMemoryCache cache, ILogger<SearchController> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet("autocomplete")]
    public async Task<ActionResult<Result<List<SearchResultDto>>>> AutoComplete(
        [FromQuery] string q,
        [FromQuery] int maxResults = 10)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Validation
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
        {
            return BadRequest(Result<List<SearchResultDto>>.Invalid(
                new List<ValidationError> { new ValidationError { ErrorMessage = "Query must be at least 2 characters" } }));
        }

        if (maxResults < 1 || maxResults > 50)
        {
            return BadRequest(Result<List<SearchResultDto>>.Invalid(
                new List<ValidationError> { new ValidationError { ErrorMessage = "MaxResults must be between 1 and 50" } }));
        }

        // Cache key
        var cacheKey = $"search:{q.ToLowerInvariant()}:{maxResults}";

        // Try get from cache
        if (_cache.TryGetValue(cacheKey, out List<SearchResultDto>? cachedResults) && cachedResults != null)
        {
            _logger.LogInformation("Search cache hit for query: {Query}", q);
            return Ok(Result<List<SearchResultDto>>.Success(cachedResults));
        }

        var results = new List<SearchResultDto>();
        var searchTerm = q.ToLowerInvariant().Trim();

        try
        {
            // 1. Search in Communes
            var communes = await _context.Communes
                .Where(c => EF.Functions.Like(c.Nom.ToLower(), $"%{searchTerm}%")
                         || EF.Functions.Like(c.CodeInsee, $"{searchTerm}%"))
                .OrderBy(c => c.Nom)
                .Take(maxResults)
                .ToListAsync();

            foreach (var commune in communes)
            {
                results.Add(new SearchResultDto
                {
                    Id = $"commune:{commune.CodeInsee}",
                    Type = "commune",
                    Label = $"{commune.Nom} ({commune.DepartementCode})",
                    SecondaryLabel = $"{commune.DepartementNom}, Grand Est",
                    Latitude = commune.Geometry.Y,
                    Longitude = commune.Geometry.X,
                    Score = CalculateScore(commune.Nom, searchTerm)
                });
            }

            // 2. Search in EPCIs
            var epcis = await _context.EPCIs
                .Where(e => EF.Functions.Like(e.Nom.ToLower(), $"%{searchTerm}%")
                         || EF.Functions.Like(e.CodeSiren, $"{searchTerm}%"))
                .OrderBy(e => e.Nom)
                .Take(maxResults)
                .ToListAsync();

            foreach (var epci in epcis)
            {
                results.Add(new SearchResultDto
                {
                    Id = $"epci:{epci.CodeSiren}",
                    Type = "epci",
                    Label = epci.Nom,
                    SecondaryLabel = $"EPCI - {epci.TypeEPCI} ({epci.CommunesCount} communes)",
                    Score = CalculateScore(epci.Nom, searchTerm)
                });
            }

            // 3. Search in Departements
            var departements = await _context.Departements
                .Where(d => EF.Functions.Like(d.Nom.ToLower(), $"%{searchTerm}%")
                         || EF.Functions.Like(d.CodeDept, $"{searchTerm}%"))
                .OrderBy(d => d.Nom)
                .Take(maxResults)
                .ToListAsync();

            foreach (var dept in departements)
            {
                results.Add(new SearchResultDto
                {
                    Id = $"departement:{dept.CodeDept}",
                    Type = "departement",
                    Label = $"{dept.Nom} ({dept.CodeDept})",
                    SecondaryLabel = "Grand Est",
                    Score = CalculateScore(dept.Nom, searchTerm)
                });
            }

            // 4. Try parse coordinates (format: "48.5,7.5" or "48.5, 7.5")
            if (TryParseCoordinates(searchTerm, out var lat, out var lon))
            {
                results.Add(new SearchResultDto
                {
                    Id = $"coord:{lat},{lon}",
                    Type = "coordinate",
                    Label = $"Coordonnées: {lat:F5}°N, {lon:F5}°E",
                    Latitude = lat,
                    Longitude = lon,
                    Score = 1000 // High priority for coordinates
                });
            }

            // Sort by score and limit
            var sortedResults = results
                .OrderByDescending(r => r.Score)
                .ThenBy(r => r.Label)
                .Take(maxResults)
                .ToList();

            // Cache results for 5 minutes
            _cache.Set(cacheKey, sortedResults, TimeSpan.FromMinutes(5));

            sw.Stop();
            _logger.LogInformation("Search completed in {ElapsedMs}ms for query: {Query}, found {Count} results",
                sw.ElapsedMilliseconds, q, sortedResults.Count);

            return Ok(Result<List<SearchResultDto>>.Success(sortedResults));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during search for query: {Query}", q);
            return StatusCode(500, Result<List<SearchResultDto>>.Error("An error occurred during search"));
        }
    }

    private int CalculateScore(string name, string searchTerm)
    {
        var nameLower = name.ToLowerInvariant();

        // Exact match
        if (nameLower == searchTerm) return 100;

        // Starts with
        if (nameLower.StartsWith(searchTerm)) return 80;

        // Contains at word boundary
        if (nameLower.Contains($" {searchTerm}")) return 60;

        // Contains anywhere
        if (nameLower.Contains(searchTerm)) return 40;

        return 0;
    }

    private bool TryParseCoordinates(string input, out double lat, out double lon)
    {
        lat = 0;
        lon = 0;

        // Format accepté: "48.5, 7.5" ou "48.5,7.5"
        // Lat entre 47-50 (Grand Est), Lon entre 4-8
        var parts = input.Split(',');
        if (parts.Length != 2)
            return false;

        if (!double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out lat))
            return false;

        if (!double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out lon))
            return false;

        // Validate range for Grand Est region
        if (lat < 47.0 || lat > 50.0)
            return false;

        if (lon < 4.0 || lon > 8.0)
            return false;

        return true;
    }
}
