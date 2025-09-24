using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocSig.Domain.Entities;

namespace PocSig.Infrastructure.Configurations;

public class LayerConfiguration : IEntityTypeConfiguration<Layer>
{
    public void Configure(EntityTypeBuilder<Layer> builder)
    {
        builder.ToTable("Layers");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Srid)
            .IsRequired()
            .HasDefaultValue(4326);

        builder.Property(e => e.GeometryType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.CreatedUtc)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.UpdatedUtc)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.MetadataJson)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(e => e.Name)
            .IsUnique();

        builder.HasMany(e => e.Features)
            .WithOne(e => e.Layer)
            .HasForeignKey(e => e.LayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}