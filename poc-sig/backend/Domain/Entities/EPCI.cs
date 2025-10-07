namespace PocSig.Domain.Entities;

public class EPCI
{
    public int Id { get; set; }
    public string CodeSiren { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string TypeEPCI { get; set; } = string.Empty; // CC, CA, CU, Métropole
    public string? DepartementCode { get; set; } // Département principal
    public int CommunesCount { get; set; }
    public DateTime CreatedUtc { get; set; }
}
