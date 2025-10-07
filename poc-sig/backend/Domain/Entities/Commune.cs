using NetTopologySuite.Geometries;

namespace PocSig.Domain.Entities;

public class Commune
{
    public int Id { get; set; }
    public string CodeInsee { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public int? Population { get; set; }
    public Point Geometry { get; set; } = null!; // Centroïde de la commune
    public string DepartementCode { get; set; } = string.Empty;
    public string DepartementNom { get; set; } = string.Empty;
    public string? EPCICode { get; set; }
    public string? EPCINom { get; set; }
    public DateTime CreatedUtc { get; set; }
}
