using NetTopologySuite.Geometries;
using PocSig.Domain.Entities;
using PocSig.Infrastructure;

namespace PocSig.Services;

public class TerritorialDataSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<TerritorialDataSeeder> _logger;
    private readonly GeometryFactory _geometryFactory;

    public TerritorialDataSeeder(AppDbContext context, ILogger<TerritorialDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
        _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
    }

    public async Task<(int departementsCount, int communesCount, int epcisCount)> SeedGrandEstTerritorialDataAsync()
    {
        _logger.LogInformation("Starting territorial data seeding for Grand Est region...");

        var departementsCount = await SeedDepartements();
        var communesCount = await SeedCommunes();
        var epcisCount = await SeedEPCIs();

        _logger.LogInformation("Territorial data seeding completed: {DeptCount} départements, {CommuneCount} communes, {EpciCount} EPCI",
            departementsCount, communesCount, epcisCount);

        return (departementsCount, communesCount, epcisCount);
    }

    private async Task<int> SeedDepartements()
    {
        var departements = new List<Departement>
        {
            new Departement { CodeDept = "08", Nom = "Ardennes", Population = 270582, RegionCode = "44", RegionNom = "Grand Est", CreatedUtc = DateTime.UtcNow },
            new Departement { CodeDept = "10", Nom = "Aube", Population = 310020, RegionCode = "44", RegionNom = "Grand Est", CreatedUtc = DateTime.UtcNow },
            new Departement { CodeDept = "51", Nom = "Marne", Population = 566145, RegionCode = "44", RegionNom = "Grand Est", CreatedUtc = DateTime.UtcNow },
            new Departement { CodeDept = "52", Nom = "Haute-Marne", Population = 172512, RegionCode = "44", RegionNom = "Grand Est", CreatedUtc = DateTime.UtcNow },
            new Departement { CodeDept = "54", Nom = "Meurthe-et-Moselle", Population = 733481, RegionCode = "44", RegionNom = "Grand Est", CreatedUtc = DateTime.UtcNow },
            new Departement { CodeDept = "55", Nom = "Meuse", Population = 184083, RegionCode = "44", RegionNom = "Grand Est", CreatedUtc = DateTime.UtcNow },
            new Departement { CodeDept = "57", Nom = "Moselle", Population = 1043522, RegionCode = "44", RegionNom = "Grand Est", CreatedUtc = DateTime.UtcNow },
            new Departement { CodeDept = "67", Nom = "Bas-Rhin", Population = 1125559, RegionCode = "44", RegionNom = "Grand Est", CreatedUtc = DateTime.UtcNow },
            new Departement { CodeDept = "68", Nom = "Haut-Rhin", Population = 764030, RegionCode = "44", RegionNom = "Grand Est", CreatedUtc = DateTime.UtcNow },
            new Departement { CodeDept = "88", Nom = "Vosges", Population = 364499, RegionCode = "44", RegionNom = "Grand Est", CreatedUtc = DateTime.UtcNow }
        };

        _context.Departements.AddRange(departements);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} départements", departements.Count);
        return departements.Count;
    }

    private async Task<int> SeedCommunes()
    {
        var communes = new List<Commune>
        {
            // Ardennes (08)
            new Commune { CodeInsee = "08105", Nom = "Charleville-Mézières", Population = 45948, DepartementCode = "08", DepartementNom = "Ardennes", Geometry = _geometryFactory.CreatePoint(new Coordinate(4.7167, 49.7667)), CreatedUtc = DateTime.UtcNow },
            new Commune { CodeInsee = "08409", Nom = "Sedan", Population = 16409, DepartementCode = "08", DepartementNom = "Ardennes", Geometry = _geometryFactory.CreatePoint(new Coordinate(4.9428, 49.7014)), CreatedUtc = DateTime.UtcNow },

            // Aube (10)
            new Commune { CodeInsee = "10387", Nom = "Troyes", Population = 61652, DepartementCode = "10", DepartementNom = "Aube", Geometry = _geometryFactory.CreatePoint(new Coordinate(4.0833, 48.3000)), CreatedUtc = DateTime.UtcNow },
            new Commune { CodeInsee = "10362", Nom = "Saint-André-les-Vergers", Population = 12355, DepartementCode = "10", DepartementNom = "Aube", Geometry = _geometryFactory.CreatePoint(new Coordinate(4.0500, 48.2833)), CreatedUtc = DateTime.UtcNow },

            // Marne (51)
            new Commune { CodeInsee = "51454", Nom = "Reims", Population = 182460, DepartementCode = "51", DepartementNom = "Marne", Geometry = _geometryFactory.CreatePoint(new Coordinate(4.0333, 49.2500)), CreatedUtc = DateTime.UtcNow },
            new Commune { CodeInsee = "51108", Nom = "Châlons-en-Champagne", Population = 44476, DepartementCode = "51", DepartementNom = "Marne", Geometry = _geometryFactory.CreatePoint(new Coordinate(4.3667, 48.9589)), CreatedUtc = DateTime.UtcNow },
            new Commune { CodeInsee = "51230", Nom = "Épernay", Population = 23507, DepartementCode = "51", DepartementNom = "Marne", Geometry = _geometryFactory.CreatePoint(new Coordinate(3.9600, 49.0428)), CreatedUtc = DateTime.UtcNow },

            // Haute-Marne (52)
            new Commune { CodeInsee = "52121", Nom = "Chaumont", Population = 22000, DepartementCode = "52", DepartementNom = "Haute-Marne", Geometry = _geometryFactory.CreatePoint(new Coordinate(5.1389, 48.1122)), CreatedUtc = DateTime.UtcNow },
            new Commune { CodeInsee = "52448", Nom = "Saint-Dizier", Population = 24134, DepartementCode = "52", DepartementNom = "Haute-Marne", Geometry = _geometryFactory.CreatePoint(new Coordinate(4.9494, 48.6378)), CreatedUtc = DateTime.UtcNow },

            // Meurthe-et-Moselle (54)
            new Commune { CodeInsee = "54395", Nom = "Nancy", Population = 104286, DepartementCode = "54", DepartementNom = "Meurthe-et-Moselle", Geometry = _geometryFactory.CreatePoint(new Coordinate(6.1833, 48.6833)), CreatedUtc = DateTime.UtcNow },
            new Commune { CodeInsee = "54580", Nom = "Vandœuvre-lès-Nancy", Population = 29439, DepartementCode = "54", DepartementNom = "Meurthe-et-Moselle", Geometry = _geometryFactory.CreatePoint(new Coordinate(6.1667, 48.6500)), CreatedUtc = DateTime.UtcNow },
            new Commune { CodeInsee = "54329", Nom = "Lunéville", Population = 19620, DepartementCode = "54", DepartementNom = "Meurthe-et-Moselle", Geometry = _geometryFactory.CreatePoint(new Coordinate(6.4981, 48.5931)), CreatedUtc = DateTime.UtcNow },

            // Meuse (55)
            new Commune { CodeInsee = "55029", Nom = "Bar-le-Duc", Population = 14827, DepartementCode = "55", DepartementNom = "Meuse", Geometry = _geometryFactory.CreatePoint(new Coordinate(5.1608, 48.7714)), CreatedUtc = DateTime.UtcNow },
            new Commune { CodeInsee = "55545", Nom = "Verdun", Population = 17639, DepartementCode = "55", DepartementNom = "Meuse", Geometry = _geometryFactory.CreatePoint(new Coordinate(5.3833, 49.1600)), CreatedUtc = DateTime.UtcNow },

            // Moselle (57)
            new Commune { CodeInsee = "57463", Nom = "Metz", Population = 116429, DepartementCode = "57", DepartementNom = "Moselle", Geometry = _geometryFactory.CreatePoint(new Coordinate(6.1778, 49.1197)), CreatedUtc = DateTime.UtcNow },
            new Commune { CodeInsee = "57227", Nom = "Forbach", Population = 21613, DepartementCode = "57", DepartementNom = "Moselle", Geometry = _geometryFactory.CreatePoint(new Coordinate(6.8978, 49.1900)), CreatedUtc = DateTime.UtcNow },
            new Commune { CodeInsee = "57672", Nom = "Thionville", Population = 40701, DepartementCode = "57", DepartementNom = "Moselle", Geometry = _geometryFactory.CreatePoint(new Coordinate(6.1669, 49.3578)), CreatedUtc = DateTime.UtcNow },
            new Commune { CodeInsee = "57474", Nom = "Montigny-lès-Metz", Population = 21928, DepartementCode = "57", DepartementNom = "Moselle", Geometry = _geometryFactory.CreatePoint(new Coordinate(6.1539, 49.1000)), CreatedUtc = DateTime.UtcNow },

            // Bas-Rhin (67)
            new Commune { CodeInsee = "67482", Nom = "Strasbourg", Population = 280966, DepartementCode = "67", DepartementNom = "Bas-Rhin", Geometry = _geometryFactory.CreatePoint(new Coordinate(7.7500, 48.5833)), CreatedUtc = DateTime.UtcNow },
            new Commune { CodeInsee = "67218", Nom = "Haguenau", Population = 35098, DepartementCode = "67", DepartementNom = "Bas-Rhin", Geometry = _geometryFactory.CreatePoint(new Coordinate(7.7906, 48.8156)), CreatedUtc = DateTime.UtcNow },
            new Commune { CodeInsee = "67447", Nom = "Schiltigheim", Population = 32400, DepartementCode = "67", DepartementNom = "Bas-Rhin", Geometry = _geometryFactory.CreatePoint(new Coordinate(7.7500, 48.6067)), CreatedUtc = DateTime.UtcNow },
            new Commune { CodeInsee = "67124", Nom = "Sélestat", Population = 19459, DepartementCode = "67", DepartementNom = "Bas-Rhin", Geometry = _geometryFactory.CreatePoint(new Coordinate(7.4525, 48.2600)), CreatedUtc = DateTime.UtcNow },

            // Haut-Rhin (68)
            new Commune { CodeInsee = "68066", Nom = "Colmar", Population = 68703, DepartementCode = "68", DepartementNom = "Haut-Rhin", Geometry = _geometryFactory.CreatePoint(new Coordinate(7.3553, 48.0778)), CreatedUtc = DateTime.UtcNow },
            new Commune { CodeInsee = "68224", Nom = "Mulhouse", Population = 108312, DepartementCode = "68", DepartementNom = "Haut-Rhin", Geometry = _geometryFactory.CreatePoint(new Coordinate(7.3389, 47.7508)), CreatedUtc = DateTime.UtcNow },
            new Commune { CodeInsee = "68297", Nom = "Saint-Louis", Population = 22304, DepartementCode = "68", DepartementNom = "Haut-Rhin", Geometry = _geometryFactory.CreatePoint(new Coordinate(7.5603, 47.5903)), CreatedUtc = DateTime.UtcNow },

            // Vosges (88)
            new Commune { CodeInsee = "88160", Nom = "Épinal", Population = 31903, DepartementCode = "88", DepartementNom = "Vosges", Geometry = _geometryFactory.CreatePoint(new Coordinate(6.4500, 48.1744)), CreatedUtc = DateTime.UtcNow },
            new Commune { CodeInsee = "88304", Nom = "Saint-Dié-des-Vosges", Population = 19311, DepartementCode = "88", DepartementNom = "Vosges", Geometry = _geometryFactory.CreatePoint(new Coordinate(6.9500, 48.2833)), CreatedUtc = DateTime.UtcNow }
        };

        _context.Communes.AddRange(communes);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} communes", communes.Count);
        return communes.Count;
    }

    private async Task<int> SeedEPCIs()
    {
        var epcis = new List<EPCI>
        {
            new EPCI { CodeSiren = "200067213", Nom = "Eurométropole de Strasbourg", TypeEPCI = "Métropole", DepartementCode = "67", CommunesCount = 33, CreatedUtc = DateTime.UtcNow },
            new EPCI { CodeSiren = "200068146", Nom = "Mulhouse Alsace Agglomération", TypeEPCI = "CA", DepartementCode = "68", CommunesCount = 39, CreatedUtc = DateTime.UtcNow },
            new EPCI { CodeSiren = "200070530", Nom = "Métropole du Grand Nancy", TypeEPCI = "Métropole", DepartementCode = "54", CommunesCount = 20, CreatedUtc = DateTime.UtcNow },
            new EPCI { CodeSiren = "200039865", Nom = "Metz Métropole", TypeEPCI = "Métropole", DepartementCode = "57", CommunesCount = 44, CreatedUtc = DateTime.UtcNow },
            new EPCI { CodeSiren = "200066876", Nom = "Communauté d'agglomération de Reims", TypeEPCI = "CA", DepartementCode = "51", CommunesCount = 143, CreatedUtc = DateTime.UtcNow },
            new EPCI { CodeSiren = "246800726", Nom = "Colmar Agglomération", TypeEPCI = "CA", DepartementCode = "68", CommunesCount = 9, CreatedUtc = DateTime.UtcNow },
            new EPCI { CodeSiren = "200067825", Nom = "Communauté d'agglomération d'Épinal", TypeEPCI = "CA", DepartementCode = "88", CommunesCount = 78, CreatedUtc = DateTime.UtcNow },
            new EPCI { CodeSiren = "200067726", Nom = "Grand Troyes Agglomération", TypeEPCI = "CA", DepartementCode = "10", CommunesCount = 81, CreatedUtc = DateTime.UtcNow },
            new EPCI { CodeSiren = "200041010", Nom = "Communauté d'agglomération Ardenne Métropole", TypeEPCI = "CA", DepartementCode = "08", CommunesCount = 58, CreatedUtc = DateTime.UtcNow },
            new EPCI { CodeSiren = "200068799", Nom = "Communauté d'agglomération de Châlons-en-Champagne", TypeEPCI = "CA", DepartementCode = "51", CommunesCount = 38, CreatedUtc = DateTime.UtcNow }
        };

        _context.EPCIs.AddRange(epcis);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} EPCI", epcis.Count);
        return epcis.Count;
    }
}
