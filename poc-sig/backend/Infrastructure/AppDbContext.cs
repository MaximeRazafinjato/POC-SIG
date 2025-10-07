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
    public DbSet<Commune> Communes => Set<Commune>();
    public DbSet<EPCI> EPCIs => Set<EPCI>();
    public DbSet<Departement> Departements => Set<Departement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("dbo");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

}