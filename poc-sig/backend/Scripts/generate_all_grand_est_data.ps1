# Script PowerShell pour générer toutes les données du Grand Est avec encodage UTF-8 correct

$outputFile = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\Scripts\grand_est_eau_complete.geojson"

Write-Host "Génération des données complètes du Grand Est..." -ForegroundColor Cyan

# Créer la collection de features
$features = @()

# 1. Piézomètres (stations de mesure des nappes phréatiques)
$communes = @(
    @{nom="Strasbourg"; lon=7.75; lat=48.58; dept="Bas-Rhin"},
    @{nom="Mulhouse"; lon=7.34; lat=47.75; dept="Haut-Rhin"},
    @{nom="Colmar"; lon=7.36; lat=48.08; dept="Haut-Rhin"},
    @{nom="Metz"; lon=6.18; lat=49.12; dept="Moselle"},
    @{nom="Nancy"; lon=6.18; lat=48.69; dept="Meurthe-et-Moselle"},
    @{nom="Reims"; lon=4.03; lat=49.26; dept="Marne"},
    @{nom="Charleville-Mézières"; lon=4.72; lat=49.77; dept="Ardennes"},
    @{nom="Troyes"; lon=4.08; lat=48.30; dept="Aube"},
    @{nom="Chaumont"; lon=5.14; lat=48.11; dept="Haute-Marne"},
    @{nom="Bar-le-Duc"; lon=5.16; lat=48.77; dept="Meuse"},
    @{nom="Épinal"; lon=6.45; lat=48.17; dept="Vosges"},
    @{nom="Saint-Dizier"; lon=4.95; lat=48.64; dept="Haute-Marne"},
    @{nom="Verdun"; lon=5.38; lat=49.16; dept="Meuse"},
    @{nom="Haguenau"; lon=7.79; lat=48.82; dept="Bas-Rhin"},
    @{nom="Schiltigheim"; lon=7.75; lat=48.61; dept="Bas-Rhin"},
    @{nom="Illkirch-Graffenstaden"; lon=7.71; lat=48.53; dept="Bas-Rhin"},
    @{nom="Saint-Louis"; lon=7.56; lat=47.59; dept="Haut-Rhin"},
    @{nom="Sélestat"; lon=7.45; lat=48.26; dept="Bas-Rhin"},
    @{nom="Bischheim"; lon=7.75; lat=48.62; dept="Bas-Rhin"},
    @{nom="Thionville"; lon=6.17; lat=49.36; dept="Moselle"},
    @{nom="Montigny-lès-Metz"; lon=6.15; lat=49.10; dept="Moselle"},
    @{nom="Sarreguemines"; lon=7.07; lat=49.11; dept="Moselle"},
    @{nom="Forbach"; lon=6.90; lat=49.19; dept="Moselle"},
    @{nom="Saint-Avold"; lon=6.70; lat=49.10; dept="Moselle"},
    @{nom="Vandœuvre-lès-Nancy"; lon=6.17; lat=48.66; dept="Meurthe-et-Moselle"},
    @{nom="Lunéville"; lon=6.50; lat=48.59; dept="Meurthe-et-Moselle"},
    @{nom="Pont-à-Mousson"; lon=6.05; lat=48.90; dept="Meurthe-et-Moselle"},
    @{nom="Longwy"; lon=5.76; lat=49.52; dept="Meurthe-et-Moselle"},
    @{nom="Châlons-en-Champagne"; lon=4.36; lat=48.96; dept="Marne"},
    @{nom="Vitry-le-François"; lon=4.58; lat=48.72; dept="Marne"},
    @{nom="Épernay"; lon=3.96; lat=49.04; dept="Marne"},
    @{nom="Sedan"; lon=4.94; lat=49.70; dept="Ardennes"},
    @{nom="Romilly-sur-Seine"; lon=3.73; lat=48.52; dept="Aube"},
    @{nom="La Chapelle-Saint-Luc"; lon=4.04; lat=48.31; dept="Aube"},
    @{nom="Saint-Dié-des-Vosges"; lon=6.95; lat=48.29; dept="Vosges"},
    @{nom="Gérardmer"; lon=6.88; lat=48.07; dept="Vosges"},
    @{nom="Remiremont"; lon=6.59; lat=48.02; dept="Vosges"}
)

Write-Host "Création des piézomètres..." -ForegroundColor Yellow
foreach ($commune in $communes) {
    # Créer 3-5 piézomètres par commune
    $nbPiezo = Get-Random -Minimum 3 -Maximum 6
    for ($i = 1; $i -le $nbPiezo; $i++) {
        $lonVar = $commune.lon + (Get-Random -Minimum -50 -Maximum 50) / 1000
        $latVar = $commune.lat + (Get-Random -Minimum -50 -Maximum 50) / 1000
        $code = "0$(Get-Random -Minimum 1000 -Maximum 9999)X$(Get-Random -Minimum 100 -Maximum 999)/F$i"

        $feature = @{
            type = "Feature"
            geometry = @{
                type = "Point"
                coordinates = @($lonVar, $latVar)
            }
            properties = @{
                name = "$($commune.nom) - Piézomètre $code"
                type = "Piézomètre"
                category = "nappe_phreatique"
                layer = "Piézomètres"
                commune = $commune.nom
                departement = $commune.dept
                code_bss = $code
                altitude_sol = "$(Get-Random -Minimum 100 -Maximum 400)"
                date_debut = "$(Get-Random -Minimum 1960 -Maximum 2020)-01-01"
                profondeur = Get-Random -Minimum 5 -Maximum 50
                color = "#4169E1"
                validFrom = "2024-01-01T00:00:00Z"
            }
        }
        $features += $feature
    }
}

# 2. Cours d'eau
$coursEau = @(
    @{name="Rhin - Section Alsace"; coords=@(@(7.588, 48.966), @(7.734, 48.585), @(7.909, 47.916)); debit=1080; qualite="Bonne"; bassin="Rhin-Meuse"},
    @{name="Moselle"; coords=@(@(6.179, 49.120), @(6.365, 48.689), @(6.737, 48.114)); debit=145; qualite="Moyenne"; bassin="Rhin-Meuse"},
    @{name="Meuse"; coords=@(@(5.379, 49.540), @(5.168, 48.640), @(4.863, 47.980)); debit=230; qualite="Bonne"; bassin="Rhin-Meuse"},
    @{name="Marne"; coords=@(@(4.363, 48.950), @(4.796, 48.640), @(5.139, 48.260)); debit=110; qualite="Moyenne"; bassin="Seine-Normandie"},
    @{name="Ill"; coords=@(@(7.339, 48.584), @(7.448, 48.318), @(7.581, 47.590)); debit=58; qualite="Bonne"; bassin="Rhin-Meuse"},
    @{name="Sarre"; coords=@(@(7.065, 49.111), @(7.032, 48.980), @(6.982, 48.795)); debit=78; qualite="Moyenne"; bassin="Rhin-Meuse"},
    @{name="Aisne"; coords=@(@(4.521, 49.474), @(4.686, 49.256), @(4.822, 49.071)); debit=63; qualite="Bonne"; bassin="Seine-Normandie"},
    @{name="Aube"; coords=@(@(4.075, 48.297), @(4.329, 48.402), @(4.593, 48.516)); debit=41; qualite="Bonne"; bassin="Seine-Normandie"},
    @{name="Seine (source)"; coords=@(@(4.205, 48.088), @(4.341, 48.167), @(4.498, 48.275)); debit=25; qualite="Excellente"; bassin="Seine-Normandie"},
    @{name="Seille"; coords=@(@(6.532, 48.914), @(6.401, 48.792), @(6.241, 48.638)); debit=35; qualite="Moyenne"; bassin="Rhin-Meuse"}
)

Write-Host "Ajout des cours d'eau..." -ForegroundColor Yellow
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
            qualite_eau = $riviere.qualite
            bassin = $riviere.bassin
            color = "#1E90FF"
            validFrom = "2024-01-01T00:00:00Z"
        }
    }
    $features += $feature
}

# 3. Lacs et plans d'eau
$lacs = @(
    @{name="Lac de Gérardmer"; coords=@(6.852, 48.073); surface=115; prof=38; dept="Vosges"},
    @{name="Lac de Pierre-Percée"; coords=@(6.933, 48.466); surface=304; prof=78; dept="Meurthe-et-Moselle"},
    @{name="Lac du Der-Chantecoq"; coords=@(4.770, 48.586); surface=4800; prof=20; dept="Marne/Haute-Marne"},
    @{name="Lac de Madine"; coords=@(5.743, 48.918); surface=1100; prof=15; dept="Meuse"},
    @{name="Lac Blanc"; coords=@(7.094, 48.137); surface=29; prof=72; dept="Haut-Rhin"},
    @{name="Lac de Kruth-Wildenstein"; coords=@(6.966, 47.953); surface=81; prof=35; dept="Haut-Rhin"},
    @{name="Lac de Longemer"; coords=@(6.931, 48.075); surface=76; prof=34; dept="Vosges"},
    @{name="Lac de Retournemer"; coords=@(6.903, 48.090); surface=5; prof=16; dept="Vosges"},
    @{name="Lac des Corbeaux"; coords=@(6.892, 48.041); surface=9; prof=27; dept="Vosges"},
    @{name="Plan d'eau de Metz"; coords=@(6.233, 49.113); surface=62; prof=4; dept="Moselle"},
    @{name="Étang du Stock"; coords=@(6.781, 48.754); surface=700; prof=8; dept="Moselle"},
    @{name="Lac de Bouzey"; coords=@(6.362, 48.169); surface=140; prof=14; dept="Vosges"},
    @{name="Lac de la Liez"; coords=@(5.274, 48.016); surface=290; prof=16; dept="Haute-Marne"},
    @{name="Lac de la Mouche"; coords=@(5.185, 47.952); surface=94; prof=12; dept="Haute-Marne"},
    @{name="Lac de la Vingeanne"; coords=@(5.403, 47.915); surface=196; prof=21; dept="Haute-Marne"}
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
            layer = "Lacs et plans d'eau"
            surface_ha = $lac.surface
            profondeur_max = $lac.prof
            departement = $lac.dept
            color = "#00CED1"
            validFrom = "2024-01-01T00:00:00Z"
        }
    }
    $features += $feature
}

# 4. Infrastructures hydrauliques
$infrastructures = @(
    @{name="Barrage de Kembs"; coords=@(7.502, 47.691); type="Barrage"; capacite="52 MW"; dept="Haut-Rhin"},
    @{name="Station épuration Strasbourg"; coords=@(7.795, 48.527); type="Station épuration"; capacite="450000 EH"; dept="Bas-Rhin"},
    @{name="Station épuration Nancy"; coords=@(6.208, 48.663); type="Station épuration"; capacite="300000 EH"; dept="Meurthe-et-Moselle"},
    @{name="Station épuration Metz"; coords=@(6.195, 49.095); type="Station épuration"; capacite="230000 EH"; dept="Moselle"},
    @{name="Station épuration Reims"; coords=@(4.025, 49.231); type="Station épuration"; capacite="250000 EH"; dept="Marne"},
    @{name="Station épuration Mulhouse"; coords=@(7.312, 47.732); type="Station épuration"; capacite="180000 EH"; dept="Haut-Rhin"},
    @{name="Station épuration Colmar"; coords=@(7.385, 48.103); type="Station épuration"; capacite="120000 EH"; dept="Haut-Rhin"},
    @{name="Station épuration Troyes"; coords=@(4.101, 48.279); type="Station épuration"; capacite="150000 EH"; dept="Aube"},
    @{name="Station épuration Châlons"; coords=@(4.384, 48.943); type="Station épuration"; capacite="100000 EH"; dept="Marne"},
    @{name="Station épuration Épinal"; coords=@(6.469, 48.186); type="Station épuration"; capacite="80000 EH"; dept="Vosges"},
    @{name="Barrage de Pierre-Percée"; coords=@(6.933, 48.466); type="Barrage"; capacite="61.6 hm³"; dept="Meurthe-et-Moselle"},
    @{name="Barrage du Der-Chantecoq"; coords=@(4.770, 48.586); type="Barrage"; capacite="350 hm³"; dept="Marne"},
    @{name="Barrage de Kruth-Wildenstein"; coords=@(6.966, 47.953); type="Barrage"; capacite="12 hm³"; dept="Haut-Rhin"},
    @{name="Usine hydroélectrique Ottmarsheim"; coords=@(7.506, 47.787); type="Centrale hydraulique"; capacite="156 MW"; dept="Haut-Rhin"},
    @{name="Usine hydroélectrique Fessenheim"; coords=@(7.563, 47.914); type="Centrale hydraulique"; capacite="173 MW"; dept="Haut-Rhin"},
    @{name="Écluse de Gambsheim"; coords=@(7.916, 48.695); type="Écluse"; capacite="Grand gabarit"; dept="Bas-Rhin"},
    @{name="Écluse de Strasbourg"; coords=@(7.807, 48.542); type="Écluse"; capacite="Grand gabarit"; dept="Bas-Rhin"},
    @{name="Port de Strasbourg"; coords=@(7.795, 48.556); type="Port fluvial"; capacite="8 Mt/an"; dept="Bas-Rhin"},
    @{name="Port de Metz"; coords=@(6.218, 49.103); type="Port fluvial"; capacite="3 Mt/an"; dept="Moselle"},
    @{name="Port de Nancy"; coords=@(6.165, 48.704); type="Port fluvial"; capacite="1 Mt/an"; dept="Meurthe-et-Moselle"}
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
    $features += $feature
}

# Créer le GeoJSON final
$geoJson = @{
    type = "FeatureCollection"
    features = $features
}

Write-Host "`nTotal : $($features.Count) éléments générés" -ForegroundColor Green

# Convertir en JSON et sauvegarder avec UTF-8
$jsonContent = $geoJson | ConvertTo-Json -Depth 10 -Compress:$false
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($outputFile, $jsonContent, $utf8NoBom)

Write-Host "✓ Fichier créé : $outputFile" -ForegroundColor Green

# Copier vers runtime
$runtimePath = "C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\bin\Debug\net9.0\Scripts\grand_est_eau_complet.geojson"
[System.IO.File]::Copy($outputFile, $runtimePath, $true)
Write-Host "✓ Fichier copié vers runtime" -ForegroundColor Green

Write-Host "`n✅ Données complètes générées avec succès!" -ForegroundColor Green