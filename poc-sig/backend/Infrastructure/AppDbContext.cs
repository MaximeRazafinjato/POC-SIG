using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using PocSig.Domain.Entities;

namespace PocSig.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Layer> Layers => Set<Layer>();
    public DbSet<FeatureEntity> Features => Set<FeatureEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("dbo");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

}