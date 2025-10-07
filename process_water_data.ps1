# Process water data and create GeoJSON
$features = @()

# Process water quality stations
$stationsFile = "C:\Users\MaximeRAZAFINJATO\POC-SIG\stations_grand_est.json"
if (Test-Path $stationsFile) {
    Write-Host "Processing water quality stations..."
    $data = Get-Content $stationsFile -Raw | ConvertFrom-Json

    foreach ($station in $data.data) {
        if ($station.longitude_station -and $station.latitude_station) {
            $feature = @{
                type = "Feature"
                properties = @{
                    name = if ($station.libelle_station) { $station.libelle_station } else { "Station inconnue" }
                    code_station = $station.code_station
                    type = "station_qualite"
                    category = "surveillance"
                    commune = $station.libelle_commune
                    code_commune = $station.code_commune
                    departement = $station.libelle_departement
                    cours_eau = $station.libelle_cours_eau
                    bassin = $station.libelle_bassin
                    layer = "Stations qualité eau"
                    validFrom = "2024-01-01T00:00:00Z"
                    validTo = $null
                    color = "#00FF00"
                }
                geometry = @{
                    type = "Point"
                    coordinates = @([double]$station.longitude_station, [double]$station.latitude_station)
                }
            }
            $features += $feature
        }
    }
    Write-Host "Processed $($features.Count) water quality stations"
}

# Process hydrometric stations
$hydroFile = "C:\Users\MaximeRAZAFINJATO\POC-SIG\stations_hydro_grand_est.json"
if (Test-Path $hydroFile) {
    Write-Host "Processing hydrometric stations..."
    $data = Get-Content $hydroFile -Raw | ConvertFrom-Json

    foreach ($station in $data.data) {
        if ($station.longitude_station -and $station.latitude_station) {
            $feature = @{
                type = "Feature"
                properties = @{
                    name = if ($station.libelle_station) { $station.libelle_station } else { "Station hydro inconnue" }
                    code_station = $station.code_station
                    type = "station_hydrometrie"
                    category = "mesure_debit"
                    commune = $station.libelle_commune
                    departement = $station.libelle_departement
                    cours_eau = $station.libelle_cours_eau
                    bassin = $station.libelle_bassin
                    altitude = if ($station.altitude_ref_alti_station) { $station.altitude_ref_alti_station } else { 0 }
                    en_service = if ($station.en_service) { $station.en_service } else { $true }
                    layer = "Stations hydrométriques"
                    validFrom = "2024-01-01T00:00:00Z"
                    validTo = $null
                    color = "#0066CC"
                }
                geometry = @{
                    type = "Point"
                    coordinates = @([double]$station.longitude_station, [double]$station.latitude_station)
                }
            }
            $features += $feature
        }
    }
    Write-Host "Added hydrometric stations. Total features: $($features.Count)"
}

# Process piezometric stations
$piezoFile = "C:\Users\MaximeRAZAFINJATO\POC-SIG\piezometres_grand_est.json"
if (Test-Path $piezoFile) {
    Write-Host "Processing piezometric stations..."
    $data = Get-Content $piezoFile -Raw | ConvertFrom-Json

    foreach ($station in $data.data) {
        if ($station.x -and $station.y) {
            $feature = @{
                type = "Feature"
                properties = @{
                    name = "$($station.nom_commune) - Piézomètre $($station.code_bss)"
                    code_bss = $station.code_bss
                    type = "piezometre"
                    category = "nappe_phreatique"
                    commune = $station.nom_commune
                    departement = $station.nom_departement
                    nappe = $station.libelle_pe
                    profondeur = if ($station.profondeur_investigation) { $station.profondeur_investigation } else { 0 }
                    altitude_sol = if ($station.altitude_station) { $station.altitude_station } else { 0 }
                    date_debut = $station.date_debut_mesure
                    layer = "Piézomètres"
                    validFrom = "2024-01-01T00:00:00Z"
                    validTo = $null
                    color = "#4169E1"
                }
                geometry = @{
                    type = "Point"
                    coordinates = @([double]$station.x, [double]$station.y)
                }
            }
            $features += $feature
        }
    }
    Write-Host "Added piezometric stations. Total features: $($features.Count)"
}

# Add major rivers
$rivers = @(
    @{name="Rhin"; type="fleuve"; debit=1080; longueur=185; bassin="Rhin-Meuse"; coords=@(@(7.5885, 48.9660), @(7.7342, 48.5849), @(7.9090, 47.9163), @(8.2324, 47.5905))},
    @{name="Moselle"; type="riviere"; debit=145; longueur=314; bassin="Rhin-Meuse"; coords=@(@(6.1786, 49.1196), @(6.3647, 48.6891), @(6.7369, 48.1138), @(7.3608, 47.8148))},
    @{name="Meuse"; type="fleuve"; debit=230; longueur=272; bassin="Rhin-Meuse"; coords=@(@(5.1657, 49.5695), @(5.3689, 49.2924), @(5.5264, 48.7901), @(5.8923, 48.2345))},
    @{name="Marne"; type="riviere"; debit=110; longueur=180; bassin="Seine-Normandie"; coords=@(@(4.3634, 49.2739), @(4.7256, 48.9567), @(5.1367, 48.6389))},
    @{name="Ill"; type="riviere"; debit=58; longueur=223; bassin="Rhin-Meuse"; coords=@(@(7.2384, 47.4523), @(7.4485, 48.2698), @(7.7528, 48.5825))},
    @{name="Meurthe"; type="riviere"; debit=40; longueur=161; bassin="Rhin-Meuse"; coords=@(@(6.1849, 48.6921), @(6.4567, 48.5234), @(6.7234, 48.4123))},
    @{name="Sarre"; type="riviere"; debit=75; longueur=126; bassin="Rhin-Meuse"; coords=@(@(7.0234, 49.1123), @(6.8456, 48.9234), @(6.6789, 48.7345))}
)

foreach ($river in $rivers) {
    $feature = @{
        type = "Feature"
        properties = @{
            name = $river.name
            type = $river.type
            category = "cours_eau"
            debit_moyen_m3s = $river.debit
            longueur_km = $river.longueur
            bassin = $river.bassin
            layer = "Cours d'eau"
            validFrom = "2024-01-01T00:00:00Z"
            validTo = $null
            color = "#0099FF"
        }
        geometry = @{
            type = "LineString"
            coordinates = $river.coords
        }
    }
    $features += $feature
}
Write-Host "Added $($rivers.Count) major rivers. Total features: $($features.Count)"

# Add major lakes
$lakes = @(
    @{name="Lac de Gérardmer"; surface=115; bounds=@(@(6.8458, 48.0723), @(6.8789, 48.0723), @(6.8789, 48.0856), @(6.8458, 48.0856), @(6.8458, 48.0723))},
    @{name="Lac du Der-Chantecoq"; surface=4800; bounds=@(@(4.7234, 48.5423), @(4.8123, 48.5423), @(4.8123, 48.6012), @(4.7234, 48.6012), @(4.7234, 48.5423))},
    @{name="Lac de Madine"; surface=1100; bounds=@(@(5.7234, 48.9123), @(5.7678, 48.9123), @(5.7678, 48.9345), @(5.7234, 48.9345), @(5.7234, 48.9123))}
)

foreach ($lake in $lakes) {
    $feature = @{
        type = "Feature"
        properties = @{
            name = $lake.name
            type = "lac"
            category = "plan_eau"
            surface_ha = $lake.surface
            layer = "Plans d'eau"
            validFrom = "2024-01-01T00:00:00Z"
            validTo = $null
            color = "#00CCFF"
        }
        geometry = @{
            type = "Polygon"
            coordinates = @(,$lake.bounds)
        }
    }
    $features += $feature
}
Write-Host "Added $($lakes.Count) major lakes. Total features: $($features.Count)"

# Create GeoJSON structure
$geojson = @{
    type = "FeatureCollection"
    features = $features
}

# Save to files
$outputFile = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\Scripts\grand_est_eau_complet.geojson"
$geojson | ConvertTo-Json -Depth 10 | Out-File -FilePath $outputFile -Encoding UTF8
Write-Host "File saved to: $outputFile"

$buildOutput = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\bin\Debug\net9.0\Scripts\grand_est_eau_complet.geojson"
$geojson | ConvertTo-Json -Depth 10 | Out-File -FilePath $buildOutput -Encoding UTF8
Write-Host "Also saved to build directory: $buildOutput"

Write-Host "`nTotal features: $($features.Count)"