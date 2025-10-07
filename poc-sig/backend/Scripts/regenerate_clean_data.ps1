# Script pour régénérer les données proprement avec encodage UTF-8

$outputFile = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\Scripts\grand_est_eau_clean.geojson"

Write-Host "Génération de données propres pour le Grand Est..." -ForegroundColor Cyan

# Créer la structure GeoJSON avec les bonnes données
$geoJson = @{
    type = "FeatureCollection"
    features = @(
        @{
            type = "Feature"
            geometry = @{
                type = "Point"
                coordinates = @(4.093781595, 48.961999858)
            }
            properties = @{
                name = "Saint-Mard-lès-Rouffy - Piézomètre 01586X0064/F.E.D."
                type = "Piézomètre"
                category = "nappe_phreatique"
                layer = "Piézomètres"
                commune = "Saint-Mard-lès-Rouffy"
                departement = "Marne"
                code_bss = "01586X0064/F.E.D."
                altitude_sol = "89.0"
                date_debut = "2002-04-15"
                profondeur = 25.0
                color = "#4169E1"
                validFrom = "2024-01-01T00:00:00Z"
            }
        },
        @{
            type = "Feature"
            geometry = @{
                type = "Point"
                coordinates = @(7.507684613, 47.699311779)
            }
            properties = @{
                name = "Kembs - Piézomètre 04454X0089/PK11.0"
                type = "Piézomètre"
                category = "nappe_phreatique"
                layer = "Piézomètres"
                commune = "Kembs"
                departement = "Haut-Rhin"
                code_bss = "04454X0089/PK11.0"
                altitude_sol = "232.12"
                date_debut = "1966-11-15"
                profondeur = 0
                color = "#4169E1"
                validFrom = "2024-01-01T00:00:00Z"
            }
        },
        @{
            type = "Feature"
            geometry = @{
                type = "Point"
                coordinates = @(6.2, 48.9)
            }
            properties = @{
                name = "Lac de Gérardmer"
                type = "Lac"
                category = "lac"
                layer = "Lacs et plans d'eau"
                commune = "Gérardmer"
                departement = "Vosges"
                surface_ha = 115
                profondeur_max = 38
                color = "#00CED1"
                validFrom = "2024-01-01T00:00:00Z"
            }
        },
        @{
            type = "Feature"
            geometry = @{
                type = "LineString"
                coordinates = @(
                    @(7.34, 48.58),
                    @(7.35, 48.57),
                    @(7.36, 48.56)
                )
            }
            properties = @{
                name = "Rhin - Section Strasbourg"
                type = "Fleuve"
                category = "cours_eau"
                layer = "Rivières"
                departement = "Bas-Rhin"
                debit_moyen_m3s = 1080
                qualite_eau = "Bonne"
                color = "#4682B4"
                validFrom = "2024-01-01T00:00:00Z"
            }
        },
        @{
            type = "Feature"
            geometry = @{
                type = "Point"
                coordinates = @(5.9, 49.2)
            }
            properties = @{
                name = "Station de traitement de Metz"
                type = "Station épuration"
                category = "infrastructure"
                layer = "Infrastructures"
                commune = "Metz"
                departement = "Moselle"
                capacite = "150000 EH"
                color = "#FFD700"
                validFrom = "2024-01-01T00:00:00Z"
            }
        }
    )
}

# Ajouter plus de piézomètres
$piezometers = @(
    @{coords = @(7.521336367, 47.714657851); commune = "Niffer"; code = "04138X0067/PK13.0"},
    @{coords = @(7.530654934, 47.797436045); commune = "Ottmarsheim"; code = "04134X0057/PK23.0"},
    @{coords = @(7.546157681, 47.634316666); commune = "Rosenau"; code = "04454X0064/PK3.0"},
    @{coords = @(7.530515635, 47.721098627); commune = "Petit-Landau"; code = "04138X0071/PK14.0"},
    @{coords = @(7.536154514, 47.805753323); commune = "Bantzenheim"; code = "04134X0059/PK24.0"},
    @{coords = @(7.541247311, 47.813998421); commune = "Chalampé"; code = "04134X0060/PK25.0"},
    @{coords = @(7.396858871, 48.190278803); commune = "Guémar"; code = "03423X0020/100"},
    @{coords = @(6.950309917, 48.291953522); commune = "Épinal"; code = "02282X0023/F"},
    @{coords = @(5.137734223, 48.768835765); commune = "Bar-le-Duc"; code = "01125X0114/S1"},
    @{coords = @(4.363206509, 49.050097165); commune = "Reims"; code = "01072X0085/S2"}
)

foreach ($piez in $piezometers) {
    $geoJson.features += @{
        type = "Feature"
        geometry = @{
            type = "Point"
            coordinates = $piez.coords
        }
        properties = @{
            name = "$($piez.commune) - Piézomètre $($piez.code)"
            type = "Piézomètre"
            category = "nappe_phreatique"
            layer = "Piézomètres"
            commune = $piez.commune
            departement = "Grand Est"
            code_bss = $piez.code
            profondeur = Get-Random -Minimum 5 -Maximum 50
            color = "#4169E1"
            validFrom = "2024-01-01T00:00:00Z"
        }
    }
}

# Ajouter des rivières
$rivers = @(
    @{name = "Moselle"; coords = @(@(6.16, 49.12), @(6.18, 49.10), @(6.20, 49.08))},
    @{name = "Meuse"; coords = @(@(5.38, 49.54), @(5.40, 49.52), @(5.42, 49.50))},
    @{name = "Marne"; coords = @(@(4.36, 48.95), @(4.38, 48.93), @(4.40, 48.91))},
    @{name = "Ill"; coords = @(@(7.75, 48.58), @(7.73, 48.56), @(7.71, 48.54))}
)

foreach ($river in $rivers) {
    $geoJson.features += @{
        type = "Feature"
        geometry = @{
            type = "LineString"
            coordinates = $river.coords
        }
        properties = @{
            name = "Rivière $($river.name)"
            type = "Rivière"
            category = "cours_eau"
            layer = "Rivières"
            departement = "Grand Est"
            debit_moyen_m3s = Get-Random -Minimum 10 -Maximum 200
            qualite_eau = @("Excellente", "Bonne", "Moyenne")[(Get-Random -Maximum 3)]
            color = "#4682B4"
            validFrom = "2024-01-01T00:00:00Z"
        }
    }
}

Write-Host "Génération de $($geoJson.features.Count) éléments..." -ForegroundColor Green

# Convertir en JSON et sauvegarder avec UTF-8
$jsonContent = $geoJson | ConvertTo-Json -Depth 10 -Compress:$false
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($outputFile, $jsonContent, $utf8NoBom)

Write-Host "✓ Fichier créé: $outputFile" -ForegroundColor Green

# Copier vers le dossier de runtime
$runtimePath = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\bin\Debug\net9.0\Scripts\grand_est_eau_complet.geojson"
[System.IO.File]::Copy($outputFile, $runtimePath, $true)
Write-Host "✓ Fichier copié vers runtime: $runtimePath" -ForegroundColor Green

Write-Host "`n✅ Données régénérées avec succès!" -ForegroundColor Green
Write-Host "Maintenant, rechargez les données dans la base." -ForegroundColor Yellow