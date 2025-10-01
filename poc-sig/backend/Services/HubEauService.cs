using System.Text.Json;
using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using PocSig.Domain.Entities;

namespace PocSig.Services
{
    public class HubEauService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HubEauService> _logger;
        private readonly GeometryFactory _geometryFactory;

        public HubEauService(HttpClient httpClient, ILogger<HubEauService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _geometryFactory = new GeometryFactory();
        }

        public async Task<List<FeatureEntity>> GetGrandEstWaterDataAsync()
        {
            var features = new List<FeatureEntity>();

            // Départements du Grand Est
            var departements = new[] { "08", "10", "51", "52", "54", "55", "57", "67", "68", "88" };

            // 1. Récupérer les piézomètres (niveaux des nappes)
            foreach (var dept in departements)
            {
                try
                {
                    var piezometres = await GetPiezometresAsync(dept);
                    features.AddRange(piezometres);
                    _logger.LogInformation($"Récupéré {piezometres.Count} piézomètres pour le département {dept}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Erreur lors de la récupération des piézomètres pour {dept}");
                }
            }

            // Note: Les APIs de qualité et hydrométrie semblent avoir des restrictions d'accès
            // On se concentre sur les piézomètres qui fonctionnent correctement

            _logger.LogInformation($"Total de {features.Count} éléments récupérés depuis Hub'Eau");
            return features;
        }

        private async Task<List<FeatureEntity>> GetPiezometresAsync(string departement)
        {
            var features = new List<FeatureEntity>();
            var url = $"https://hubeau.eaufrance.fr/api/v1/niveaux_nappes/stations?code_departement={departement}&size=200";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("data", out var data))
                    {
                        foreach (var item in data.EnumerateArray())
                        {
                            if (TryGetCoordinates(item, "x", "y", out var lon, out var lat))
                            {
                                var properties = new Dictionary<string, object>
                                {
                                    ["name"] = GetStringProperty(item, "nom_commune") + " - Piézomètre " + GetStringProperty(item, "code_bss"),
                                    ["type"] = "Piézomètre",
                                    ["category"] = "nappe_phreatique",
                                    ["layer"] = "Piézomètres",
                                    ["code_bss"] = GetStringProperty(item, "code_bss"),
                                    ["commune"] = GetStringProperty(item, "nom_commune"),
                                    ["departement"] = GetStringProperty(item, "nom_departement"),
                                    ["altitude_sol"] = GetNumberProperty(item, "altitude_station"),
                                    ["profondeur"] = GetNumberProperty(item, "profondeur_investigation", 10),
                                    ["date_debut"] = GetStringProperty(item, "date_debut_mesure"),
                                    ["color"] = "#4169E1"
                                };

                                var feature = new FeatureEntity
                                {
                                    Geometry = _geometryFactory.CreatePoint(new Coordinate(lon, lat)),
                                    PropertiesJson = JsonSerializer.Serialize(properties),
                                    ValidFromUtc = DateTime.UtcNow
                                };

                                features.Add(feature);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération des piézomètres pour {departement}");
            }

            return features;
        }

        private async Task<List<FeatureEntity>> GetStationsQualiteAsync()
        {
            var features = new List<FeatureEntity>();
            // BBox approximative du Grand Est
            var url = "https://hubeau.eaufrance.fr/api/v1/qualite_rivieres/station_pc?bbox=3.5,47.5,8.5,50.0&size=500";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("data", out var data))
                    {
                        foreach (var item in data.EnumerateArray())
                        {
                            if (TryGetCoordinates(item, "longitude", "latitude", out var lon, out var lat))
                            {
                                var properties = new Dictionary<string, object>
                                {
                                    ["name"] = "Station qualité - " + GetStringProperty(item, "libelle_station"),
                                    ["type"] = "Station qualité",
                                    ["category"] = "qualite_eau",
                                    ["layer"] = "Qualité de l'eau",
                                    ["code_station"] = GetStringProperty(item, "code_station"),
                                    ["commune"] = GetStringProperty(item, "libelle_commune"),
                                    ["cours_eau"] = GetStringProperty(item, "libelle_cours_eau"),
                                    ["bassin"] = GetStringProperty(item, "libelle_bassin"),
                                    ["color"] = "#32CD32"
                                };

                                var feature = new FeatureEntity
                                {
                                    Geometry = _geometryFactory.CreatePoint(new Coordinate(lon, lat)),
                                    PropertiesJson = JsonSerializer.Serialize(properties),
                                    ValidFromUtc = DateTime.UtcNow
                                };

                                features.Add(feature);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des stations qualité");
            }

            return features;
        }

        private async Task<List<FeatureEntity>> GetStationsHydrometriquesAsync()
        {
            var features = new List<FeatureEntity>();
            // Codes des départements du Grand Est
            var departements = new[] { "08", "10", "51", "52", "54", "55", "57", "67", "68", "88" };

            foreach (var dept in departements)
            {
                try
                {
                    var url = $"https://hubeau.eaufrance.fr/api/v1/hydrometrie/referentiel/stations?code_departement={dept}&size=100";
                    var response = await _httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(json);

                        if (doc.RootElement.TryGetProperty("data", out var data))
                        {
                            foreach (var item in data.EnumerateArray())
                            {
                                if (TryGetCoordinates(item, "longitude_station", "latitude_station", out var lon, out var lat))
                                {
                                    var properties = new Dictionary<string, object>
                                    {
                                        ["name"] = "Station hydro - " + GetStringProperty(item, "libelle_station"),
                                        ["type"] = "Station hydrométrique",
                                        ["category"] = "hydrométrie",
                                        ["layer"] = "Hydrométrie",
                                        ["code_station"] = GetStringProperty(item, "code_station"),
                                        ["commune"] = GetStringProperty(item, "libelle_commune"),
                                        ["cours_eau"] = GetStringProperty(item, "libelle_cours_eau"),
                                        ["altitude"] = GetNumberProperty(item, "altitude_ref_alti_station"),
                                        ["color"] = "#1E90FF"
                                    };

                                    var feature = new FeatureEntity
                                    {
                                        Geometry = _geometryFactory.CreatePoint(new Coordinate(lon, lat)),
                                        PropertiesJson = JsonSerializer.Serialize(properties),
                                        ValidFromUtc = DateTime.UtcNow
                                    };

                                    features.Add(feature);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Erreur lors de la récupération des stations hydrométriques pour {dept}");
                }
            }

            return features;
        }

        private bool TryGetCoordinates(JsonElement element, string lonField, string latField, out double lon, out double lat)
        {
            lon = 0;
            lat = 0;

            if (element.TryGetProperty(lonField, out var lonElement) &&
                element.TryGetProperty(latField, out var latElement))
            {
                if (lonElement.ValueKind == JsonValueKind.Number && latElement.ValueKind == JsonValueKind.Number)
                {
                    lon = lonElement.GetDouble();
                    lat = latElement.GetDouble();
                    return true;
                }
                else if (lonElement.ValueKind == JsonValueKind.String && latElement.ValueKind == JsonValueKind.String)
                {
                    if (double.TryParse(lonElement.GetString(), out lon) &&
                        double.TryParse(latElement.GetString(), out lat))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private string GetStringProperty(JsonElement element, string propertyName, string defaultValue = "")
        {
            if (element.TryGetProperty(propertyName, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.String)
                {
                    return prop.GetString() ?? defaultValue;
                }
            }
            return defaultValue;
        }

        private double GetNumberProperty(JsonElement element, string propertyName, double defaultValue = 0)
        {
            if (element.TryGetProperty(propertyName, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.Number)
                {
                    return prop.GetDouble();
                }
                else if (prop.ValueKind == JsonValueKind.String)
                {
                    if (double.TryParse(prop.GetString(), out var value))
                    {
                        return value;
                    }
                }
            }
            return defaultValue;
        }
    }
}