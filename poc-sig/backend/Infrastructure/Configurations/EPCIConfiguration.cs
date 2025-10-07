using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocSig.Domain.Entities;

namespace PocSig.Infrastructure.Configurations;

public class EPCIConfiguration : IEntityTypeConfiguration<EPCI>
{
    public void Configure(EntityTypeBuilder<EPCI> builder)
    {
        builder.ToTable("EPCIs", "dbo");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CodeSiren)
            .IsRequired()
            .HasMaxLength(9);

        builder.Property(e => e.Nom)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.TypeEPCI)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.DepartementCode)
            .HasMaxLength(3);

        builder.Property(e => e.CreatedUtc)
            .IsRequired();

        // Index pour performance de recherche
        builder.HasIndex(e => e.CodeSiren).IsUnique();
        builder.HasIndex(e => e.Nom);
    }
}
