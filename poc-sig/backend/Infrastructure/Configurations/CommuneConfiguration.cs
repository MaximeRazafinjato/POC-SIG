using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocSig.Domain.Entities;

namespace PocSig.Infrastructure.Configurations;

public class CommuneConfiguration : IEntityTypeConfiguration<Commune>
{
    public void Configure(EntityTypeBuilder<Commune> builder)
    {
        builder.ToTable("Communes", "dbo");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CodeInsee)
            .IsRequired()
            .HasMaxLength(5);

        builder.Property(c => c.Nom)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.DepartementCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(c => c.DepartementNom)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.EPCICode)
            .HasMaxLength(9);

        builder.Property(c => c.EPCINom)
            .HasMaxLength(200);

        builder.Property(c => c.Geometry)
            .IsRequired()
            .HasColumnType("geography");

        builder.Property(c => c.CreatedUtc)
            .IsRequired();

        // Index pour performance de recherche
        builder.HasIndex(c => c.CodeInsee).IsUnique();
        builder.HasIndex(c => c.Nom);
        builder.HasIndex(c => c.DepartementCode);
        // Note: Spatial indexes must be created separately via SQL for geography columns
    }
}
