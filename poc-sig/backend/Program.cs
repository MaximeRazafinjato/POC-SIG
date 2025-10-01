using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PocSig.Infrastructure;
using PocSig.Controllers;
using PocSig.ETL;
using PocSig.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        x => x.UseNetTopologySuite()
    );
});

builder.Services.AddScoped<ImportGeoJsonCommand>();

// Add HttpClient for HubEau API calls
builder.Services.AddHttpClient<HubEauService>(client =>
{
    client.BaseAddress = new Uri("https://hubeau.eaufrance.fr/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5555"];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add in-memory caching
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseRouting();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var stopwatch = Stopwatch.StartNew();
        await dbContext.Database.MigrateAsync();
        stopwatch.Stop();
        logger.LogInformation("Database migrated successfully in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

        if (!await dbContext.Layers.AnyAsync())
        {
            var defaultLayer = new PocSig.Domain.Entities.Layer
            {
                Name = "Default",
                Srid = 4326,
                GeometryType = "MultiPolygon",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
                MetadataJson = "{\"description\": \"Default layer for POC\"}"
            };

            dbContext.Layers.Add(defaultLayer);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Created default layer");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");
        throw;
    }
}

app.Run();

public partial class Program { }