# Script pour générer un grand ensemble de données du Grand Est
# Génère 500+ éléments géographiques variés

$outputFile = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\Scripts\grand_est_eau_complet.geojson"
$runtimeFile = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\bin\Debug\net9.0\Scripts\grand_est_eau_complet.geojson"

Write-Host "Génération d'un large dataset pour le Grand Est..." -ForegroundColor Cyan

$features = @()

# Départements du Grand Est avec leurs coordonnées centrales
$departements = @{
    "08" = @{nom = "Ardennes"; lat = 49.76; lon = 4.72}
    "10" = @{nom = "Aube"; lat = 48.32; lon = 4.08}
    "51" = @{nom = "Marne"; lat = 48.96; lon = 4.37}
    "52" = @{nom = "Haute-Marne"; lat = 48.11; lon = 5.14}
    "54" = @{nom = "Meurthe-et-Moselle"; lat = 48.68; lon = 6.17}
    "55" = @{nom = "Meuse"; lat = 49.01; lon = 5.38}
    "57" = @{nom = "Moselle"; lat = 49.04; lon = 6.67}
    "67" = @{nom = "Bas-Rhin"; lat = 48.68; lon = 7.56}
    "68" = @{nom = "Haut-Rhin"; lat = 47.86; lon = 7.31}
    "88" = @{nom = "Vosges"; lat = 48.17; lon = 6.45}
}

# Génération de piézomètres (200 points)
Write-Host "Génération de 200 piézomètres..." -ForegroundColor Yellow
$piezoCount = 1
foreach ($dept in $departements.Keys) {
    $deptInfo = $departements[$dept]
    for ($i = 1; $i -le 20; $i++) {
        $lat = $deptInfo.lat + (Get-Random -Minimum -40 -Maximum 40) / 100
        $lon = $deptInfo.lon + (Get-Random -Minimum -40 -Maximum 40) / 100

        $feature = @{
            type = "Feature"
            geometry = @{
                type = "Point"
                coordinates = @([Math]::Round($lon, 6), [Math]::Round($lat, 6))
            }
            properties = @{
                name = "Piézomètre $($deptInfo.nom) #$piezoCount"
                type = "Piézomètre"
                category = "nappe_phreatique"
                layer = "Piézomètres"
                departement = $deptInfo.nom
                code_bss = "BSS$dept$(Get-Random -Minimum 1000 -Maximum 9999)"
                profondeur = Get-Random -Minimum 5 -Maximum 100
                altitude_sol = Get-Random -Minimum 100 -Maximum 500
                date_debut = "$(Get-Random -Minimum 1990 -Maximum 2023)-01-01"
                color = "#4169E1"
                validFrom = "2024-01-01T00:00:00Z"
            }
        }
        $features += $feature
        $piezoCount++
    }
}

# Génération de stations qualité d'eau (100 points)
Write-Host "Génération de 100 stations qualité..." -ForegroundColor Yellow
$stationCount = 1
foreach ($dept in $departements.Keys) {
    $deptInfo = $departements[$dept]
    for ($i = 1; $i -le 10; $i++) {
        $lat = $deptInfo.lat + (Get-Random -Minimum -30 -Maximum 30) / 100
        $lon = $deptInfo.lon + (Get-Random -Minimum -30 -Maximum 30) / 100

        $qualites = @("Très bonne", "Bonne", "Moyenne", "Médiocre")
        $feature = @{
            type = "Feature"
            geometry = @{
                type = "Point"
                coordinates = @([Math]::Round($lon, 6), [Math]::Round($lat, 6))
            }
            properties = @{
                name = "Station qualité $($deptInfo.nom) #$stationCount"
                type = "Station qualité"
                category = "qualite_eau"
                layer = "Qualité de l'eau"
                departement = $deptInfo.nom
                code_station = "STQ$dept$(Get-Random -Minimum 100 -Maximum 999)"
                qualite = $qualites[(Get-Random -Minimum 0 -Maximum $qualites.Length)]
                color = "#32CD32"
                validFrom = "2024-01-01T00:00:00Z"
            }
        }
        $features += $feature
        $stationCount++
    }
}

# Génération de lacs et plans d'eau (50 points)
Write-Host "Génération de 50 lacs..." -ForegroundColor Yellow
$lacCount = 1
foreach ($dept in $departements.Keys) {
    $deptInfo = $departements[$dept]
    for ($i = 1; $i -le 5; $i++) {
        $lat = $deptInfo.lat + (Get-Random -Minimum -25 -Maximum 25) / 100
        $lon = $deptInfo.lon + (Get-Random -Minimum -25 -Maximum 25) / 100

        $feature = @{
            type = "Feature"
            geometry = @{
                type = "Point"
                coordinates = @([Math]::Round($lon, 6), [Math]::Round($lat, 6))
            }
            properties = @{
                name = "Lac $($deptInfo.nom) #$lacCount"
                type = "Lac"
                category = "plan_eau"
                layer = "Lacs"
                departement = $deptInfo.nom
                surface_ha = Get-Random -Minimum 10 -Maximum 500
                profondeur_max = Get-Random -Minimum 5 -Maximum 50
                color = "#00CED1"
                validFrom = "2024-01-01T00:00:00Z"
            }
        }
        $features += $feature
        $lacCount++
    }
}

# Génération de stations d'épuration (80 points)
Write-Host "Génération de 80 stations d'épuration..." -ForegroundColor Yellow
$stepCount = 1
foreach ($dept in $departements.Keys) {
    $deptInfo = $departements[$dept]
    for ($i = 1; $i -le 8; $i++) {
        $lat = $deptInfo.lat + (Get-Random -Minimum -20 -Maximum 20) / 100
        $lon = $deptInfo.lon + (Get-Random -Minimum -20 -Maximum 20) / 100

        $feature = @{
            type = "Feature"
            geometry = @{
                type = "Point"
                coordinates = @([Math]::Round($lon, 6), [Math]::Round($lat, 6))
            }
            properties = @{
                name = "Station épuration $($deptInfo.nom) #$stepCount"
                type = "Station épuration"
                category = "infrastructure"
                layer = "Infrastructures"
                departement = $deptInfo.nom
                capacite = "$(Get-Random -Minimum 1000 -Maximum 50000) EH"
                color = "#FFD700"
                validFrom = "2024-01-01T00:00:00Z"
            }
        }
        $features += $feature
        $stepCount++
    }
}

# Génération de barrages (30 points)
Write-Host "Génération de 30 barrages..." -ForegroundColor Yellow
$barrageCount = 1
foreach ($dept in $departements.Keys) {
    $deptInfo = $departements[$dept]
    for ($i = 1; $i -le 3; $i++) {
        $lat = $deptInfo.lat + (Get-Random -Minimum -35 -Maximum 35) / 100
        $lon = $deptInfo.lon + (Get-Random -Minimum -35 -Maximum 35) / 100

        $types = @("Petit", "Moyen", "Grand")
        $feature = @{
            type = "Feature"
            geometry = @{
                type = "Point"
                coordinates = @([Math]::Round($lon, 6), [Math]::Round($lat, 6))
            }
            properties = @{
                name = "Barrage $($deptInfo.nom) #$barrageCount"
                type = "Barrage"
                category = "infrastructure"
                layer = "Infrastructures"
                departement = $deptInfo.nom
                capacite = $types[(Get-Random -Minimum 0 -Maximum $types.Length)]
                hauteur_m = Get-Random -Minimum 10 -Maximum 100
                color = "#FFD700"
                validFrom = "2024-01-01T00:00:00Z"
            }
        }
        $features += $feature
        $barrageCount++
    }
}

# Génération de sources (40 points)
Write-Host "Génération de 40 sources..." -ForegroundColor Yellow
$sourceCount = 1
foreach ($dept in $departements.Keys) {
    $deptInfo = $departements[$dept]
    for ($i = 1; $i -le 4; $i++) {
        $lat = $deptInfo.lat + (Get-Random -Minimum -30 -Maximum 30) / 100
        $lon = $deptInfo.lon + (Get-Random -Minimum -30 -Maximum 30) / 100

        $feature = @{
            type = "Feature"
            geometry = @{
                type = "Point"
                coordinates = @([Math]::Round($lon, 6), [Math]::Round($lat, 6))
            }
            properties = @{
                name = "Source $($deptInfo.nom) #$sourceCount"
                type = "Source"
                category = "source_eau"
                layer = "Sources"
                departement = $deptInfo.nom
                debit_l_s = Get-Random -Minimum 1 -Maximum 100
                temperature = Get-Random -Minimum 8 -Maximum 15
                color = "#87CEEB"
                validFrom = "2024-01-01T00:00:00Z"
            }
        }
        $features += $feature
        $sourceCount++
    }
}

# Ajout de rivières principales (LineString)
Write-Host "Ajout des rivières principales..." -ForegroundColor Yellow
$rivieres = @(
    @{
        name = "Rhin"
        coords = @(
            @(7.588, 48.966),
            @(7.734, 48.585),
            @(7.809, 48.100),
            @(7.909, 47.716)
        )
        debit = 1080
    },
    @{
        name = "Moselle"
        coords = @(
            @(6.179, 49.120),
            @(6.365, 48.689),
            @(6.537, 48.314),
            @(6.737, 48.114)
        )
        debit = 145
    },
    @{
        name = "Meuse"
        coords = @(
            @(5.379, 49.540),
            @(5.268, 49.123),
            @(5.168, 48.640),
            @(4.863, 47.980)
        )
        debit = 230
    },
    @{
        name = "Marne"
        coords = @(
            @(4.363, 48.950),
            @(4.596, 48.740),
            @(4.796, 48.640),
            @(5.139, 48.260)
        )
        debit = 110
    },
    @{
        name = "Ill"
        coords = @(
            @(7.339, 48.584),
            @(7.448, 48.318),
            @(7.521, 47.954),
            @(7.581, 47.590)
        )
        debit = 58
    },
    @{
        name = "Sarre"
        coords = @(
            @(7.065, 49.111),
            @(7.032, 48.980),
            @(6.982, 48.795),
            @(6.923, 48.612)
        )
        debit = 78
    },
    @{
        name = "Meurthe"
        coords = @(
            @(6.493, 48.593),
            @(6.384, 48.523),
            @(6.231, 48.485),
            @(6.152, 48.391)
        )
        debit = 42
    },
    @{
        name = "Seille"
        coords = @(
            @(6.534, 48.795),
            @(6.432, 48.723),
            @(6.312, 48.634),
            @(6.195, 48.556)
        )
        debit = 25
    },
    @{
        name = "Bruche"
        coords = @(
            @(7.724, 48.583),
            @(7.612, 48.512),
            @(7.485, 48.456),
            @(7.321, 48.401)
        )
        debit = 15
    },
    @{
        name = "Aisne"
        coords = @(
            @(4.798, 49.412),
            @(4.679, 49.331),
            @(4.512, 49.245),
            @(4.423, 49.178)
        )
        debit = 65
    }
)

foreach ($riviere in $rivieres) {
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
            qualite_eau = "Bonne"
            color = "#1E90FF"
            validFrom = "2024-01-01T00:00:00Z"
        }
    }
    $features += $feature
}

# Créer le GeoJSON final
$geoJson = @{
    type = "FeatureCollection"
    features = $features
}

Write-Host "`nGénération totale : $($features.Count) éléments" -ForegroundColor Green

# Convertir en JSON et sauvegarder avec UTF-8
$jsonContent = $geoJson | ConvertTo-Json -Depth 10 -Compress:$false
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($outputFile, $jsonContent, $utf8NoBom)

Write-Host "✓ Fichier créé : $outputFile" -ForegroundColor Green

# Copier vers le dossier runtime
[System.IO.File]::Copy($outputFile, $runtimeFile, $true)
Write-Host "✓ Fichier copié vers runtime" -ForegroundColor Green

Write-Host "`n✅ Dataset complet généré avec succès!" -ForegroundColor Green
Write-Host "Total : $($features.Count) éléments géographiques" -ForegroundColor Cyan