namespace PocSig.DTOs;

public class SearchResultDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "commune", "epci", "departement", "coordinate"
    public string Label { get; set; } = string.Empty;
    public string? SecondaryLabel { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? BoundingBox { get; set; } // Format: "minLon,minLat,maxLon,maxLat"
    public int Score { get; set; }
}
