using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocSig.Domain.Entities;

namespace PocSig.Infrastructure.Configurations;

public class FeatureEntityConfiguration : IEntityTypeConfiguration<FeatureEntity>
{
    public void Configure(EntityTypeBuilder<FeatureEntity> builder)
    {
        builder.ToTable("Features");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.LayerId)
            .IsRequired();

        builder.Property(e => e.PropertiesJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Geometry)
            .IsRequired()
            .HasColumnType("geometry");

        builder.Property(e => e.ValidFromUtc)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.ValidToUtc)
            .IsRequired(false);

        builder.HasIndex(e => e.LayerId)
            .HasDatabaseName("IX_Features_LayerId");

        builder.HasIndex(e => e.ValidFromUtc)
            .HasDatabaseName("IX_Features_ValidFromUtc");

        builder.HasIndex(e => e.ValidToUtc)
            .HasDatabaseName("IX_Features_ValidToUtc");


        builder.HasOne(e => e.Layer)
            .WithMany(e => e.Features)
            .HasForeignKey(e => e.LayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}