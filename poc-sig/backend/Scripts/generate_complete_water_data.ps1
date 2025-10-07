# Script pour générer les données complètes du Grand Est avec les vraies données Hub'Eau
# Encodage UTF-8 correct

$outputFile = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\Scripts\grand_est_eau_complet_fixed.geojson"

Write-Host "Récupération des données réelles du Grand Est depuis Hub'Eau..." -ForegroundColor Cyan

# Départements du Grand Est
$departements = @("08", "10", "51", "52", "54", "55", "57", "67", "68", "88")
$allFeatures = @()

# 1. Récupérer les piézomètres
Write-Host "Récupération des piézomètres..." -ForegroundColor Yellow
foreach ($dept in $departements) {
    try {
        $url = "https://hubeau.eaufrance.fr/api/v1/niveaux_nappes/stations?code_departement=$dept&size=50"
        $response = Invoke-RestMethod -Uri $url -Method Get

        foreach ($station in $response.data) {
            if ($station.geometry_x -and $station.geometry_y) {
                $feature = @{
                    type = "Feature"
                    geometry = @{
                        type = "Point"
                        coordinates = @([double]$station.geometry_x, [double]$station.geometry_y)
                    }
                    properties = @{
                        name = "$($station.nom_commune) - Piézomètre $($station.code_bss)"
                        type = "Piézomètre"
                        category = "nappe_phreatique"
                        layer = "Piézomètres"
                        commune = $station.nom_commune
                        departement = $station.nom_departement
                        code_bss = $station.code_bss
                        altitude_sol = $station.altitude_station
                        date_debut = $station.date_debut_mesure
                        profondeur = if ($station.profondeur_investigation) { [double]$station.profondeur_investigation } else { 10 }
                        color = "#4169E1"
                        validFrom = "2024-01-01T00:00:00Z"
                    }
                }
                $allFeatures += $feature
            }
        }
    } catch {
        Write-Host "Erreur pour le département $dept : $_" -ForegroundColor Red
    }
}

Write-Host "Trouvé $($allFeatures.Count) piézomètres" -ForegroundColor Green

# 2. Ajouter des stations qualité d'eau
Write-Host "Récupération des stations qualité..." -ForegroundColor Yellow
try {
    # Utiliser les coordonnées approximatives du Grand Est
    $url = "https://hubeau.eaufrance.fr/api/v1/qualite_rivieres/station_pc?bbox=4.0,47.5,8.5,50.0&size=100"
    $response = Invoke-RestMethod -Uri $url -Method Get

    foreach ($station in $response.data) {
        if ($station.longitude -and $station.latitude) {
            $feature = @{
                type = "Feature"
                geometry = @{
                    type = "Point"
                    coordinates = @([double]$station.longitude, [double]$station.latitude)
                }
                properties = @{
                    name = "Station qualité - $($station.libelle_station)"
                    type = "Station qualité"
                    category = "qualite_eau"
                    layer = "Qualité de l'eau"
                    commune = $station.libelle_commune
                    code_station = $station.code_station
                    bassin = $station.libelle_bassin
                    cours_eau = $station.libelle_cours_eau
                    color = "#32CD32"
                    validFrom = "2024-01-01T00:00:00Z"
                }
            }
            $allFeatures += $feature
        }
    }
    Write-Host "Trouvé $($response.data.Count) stations qualité" -ForegroundColor Green
} catch {
    Write-Host "Erreur stations qualité : $_" -ForegroundColor Red
}

# 3. Ajouter des cours d'eau majeurs
$coursEau = @(
    @{
        name = "Rhin - Section Alsace"
        coords = @(@(7.588, 48.966), @(7.734, 48.585), @(7.909, 47.916))
        debit = 1080
        bassin = "Rhin-Meuse"
    },
    @{
        name = "Moselle"
        coords = @(@(6.179, 49.120), @(6.365, 48.689), @(6.737, 48.114))
        debit = 145
        bassin = "Rhin-Meuse"
    },
    @{
        name = "Meuse"
        coords = @(@(5.379, 49.540), @(5.168, 48.640), @(4.863, 47.980))
        debit = 230
        bassin = "Rhin-Meuse"
    },
    @{
        name = "Marne"
        coords = @(@(4.363, 48.950), @(4.796, 48.640), @(5.139, 48.260))
        debit = 110
        bassin = "Seine-Normandie"
    },
    @{
        name = "Ill"
        coords = @(@(7.339, 48.584), @(7.448, 48.318), @(7.581, 47.590))
        debit = 58
        bassin = "Rhin-Meuse"
    },
    @{
        name = "Sarre"
        coords = @(@(7.065, 49.111), @(7.032, 48.980), @(6.982, 48.795))
        debit = 78
        bassin = "Rhin-Meuse"
    }
)

Write-Host "Ajout des cours d'eau principaux..." -ForegroundColor Yellow
foreach ($riviere in $coursEau) {
    $feature = @{
        type = "Feature"
        geometry = @{
            type = "LineString"
            coordinates = $riviere.coords
        }
        properties = @{
            name = $riviere.name
            type = "Cours d'eau"
            category = "cours_eau"
            layer = "Rivières"
            debit_moyen_m3s = $riviere.debit
            bassin = $riviere.bassin
            qualite_eau = "Bonne"
            color = "#1E90FF"
            validFrom = "2024-01-01T00:00:00Z"
        }
    }
    $allFeatures += $feature
}

# 4. Ajouter des lacs
$lacs = @(
    @{name = "Lac de Gérardmer"; coords = @(6.852, 48.073); surface = 115; prof = 38; dept = "Vosges"},
    @{name = "Lac de Pierre-Percée"; coords = @(6.933, 48.466); surface = 304; prof = 78; dept = "Meurthe-et-Moselle"},
    @{name = "Lac du Der-Chantecoq"; coords = @(4.770, 48.586); surface = 4800; prof = 20; dept = "Marne/Haute-Marne"},
    @{name = "Lac de Madine"; coords = @(5.743, 48.918); surface = 1100; prof = 15; dept = "Meuse"},
    @{name = "Lac Blanc"; coords = @(7.094, 48.137); surface = 29; prof = 72; dept = "Haut-Rhin"},
    @{name = "Lac de Kruth-Wildenstein"; coords = @(6.966, 47.953); surface = 81; prof = 35; dept = "Haut-Rhin"}
)

Write-Host "Ajout des lacs..." -ForegroundColor Yellow
foreach ($lac in $lacs) {
    $feature = @{
        type = "Feature"
        geometry = @{
            type = "Point"
            coordinates = $lac.coords
        }
        properties = @{
            name = $lac.name
            type = "Lac"
            category = "plan_eau"
            layer = "Lacs"
            surface_ha = $lac.surface
            profondeur_max = $lac.prof
            departement = $lac.dept
            color = "#00CED1"
            validFrom = "2024-01-01T00:00:00Z"
        }
    }
    $allFeatures += $feature
}

# 5. Ajouter des infrastructures hydrauliques
$infrastructures = @(
    @{name = "Barrage de Kembs"; coords = @(7.502, 47.691); type = "Barrage"; capacite = "Grand"; dept = "Haut-Rhin"},
    @{name = "Station épuration Strasbourg"; coords = @(7.795, 48.527); type = "Station épuration"; capacite = "450000 EH"; dept = "Bas-Rhin"},
    @{name = "Station épuration Nancy"; coords = @(6.208, 48.663); type = "Station épuration"; capacite = "300000 EH"; dept = "Meurthe-et-Moselle"},
    @{name = "Station épuration Metz"; coords = @(6.195, 49.095); type = "Station épuration"; capacite = "230000 EH"; dept = "Moselle"},
    @{name = "Barrage de Pierre-Percée"; coords = @(6.933, 48.466); type = "Barrage"; capacite = "Moyen"; dept = "Meurthe-et-Moselle"}
)

Write-Host "Ajout des infrastructures..." -ForegroundColor Yellow
foreach ($infra in $infrastructures) {
    $feature = @{
        type = "Feature"
        geometry = @{
            type = "Point"
            coordinates = $infra.coords
        }
        properties = @{
            name = $infra.name
            type = $infra.type
            category = "infrastructure"
            layer = "Infrastructures"
            capacite = $infra.capacite
            departement = $infra.dept
            color = "#FFD700"
            validFrom = "2024-01-01T00:00:00Z"
        }
    }
    $allFeatures += $feature
}

# Créer le GeoJSON final
$geoJson = @{
    type = "FeatureCollection"
    features = $allFeatures
}

Write-Host "`nGénération totale : $($allFeatures.Count) éléments" -ForegroundColor Green

# Convertir en JSON et sauvegarder avec UTF-8
$jsonContent = $geoJson | ConvertTo-Json -Depth 10 -Compress:$false
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($outputFile, $jsonContent, $utf8NoBom)

Write-Host "✓ Fichier créé : $outputFile" -ForegroundColor Green

# Copier vers le dossier runtime
$runtimePath = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\bin\Debug\net9.0\Scripts\grand_est_eau_complet.geojson"
[System.IO.File]::Copy($outputFile, $runtimePath, $true)
Write-Host "✓ Fichier copié vers runtime" -ForegroundColor Green

Write-Host "`n✅ Données complètes générées avec succès!" -ForegroundColor Green
Write-Host "Total : $($allFeatures.Count) éléments géographiques" -ForegroundColor Cyan