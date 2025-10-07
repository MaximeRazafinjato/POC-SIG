using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocSig.Domain.Entities;

namespace PocSig.Infrastructure.Configurations;

public class DepartementConfiguration : IEntityTypeConfiguration<Departement>
{
    public void Configure(EntityTypeBuilder<Departement> builder)
    {
        builder.ToTable("Departements", "dbo");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.CodeDept)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(d => d.Nom)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.RegionCode)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(d => d.RegionNom)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.CreatedUtc)
            .IsRequired();

        // Index pour performance de recherche
        builder.HasIndex(d => d.CodeDept).IsUnique();
        builder.HasIndex(d => d.Nom);
    }
}
