namespace PocSig.Domain.Entities;

public class Departement
{
    public int Id { get; set; }
    public string CodeDept { get; set; } = string.Empty; // 08, 10, 51, 52, 54, 55, 57, 67, 68, 88
    public string Nom { get; set; } = string.Empty;
    public int? Population { get; set; }
    public string RegionCode { get; set; } = "44"; // Grand Est
    public string RegionNom { get; set; } = "Grand Est";
    public DateTime CreatedUtc { get; set; }
}
