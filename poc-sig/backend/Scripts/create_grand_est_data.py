#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script pour générer les données complètes du Grand Est
avec les vraies données Hub'Eau et un encodage UTF-8 correct
"""

import json
import requests
import random

def fetch_piezometers():
    """Récupère les piézomètres depuis Hub'Eau"""
    features = []
    departments = ["08", "10", "51", "52", "54", "55", "57", "67", "68", "88"]

    for dept in departments:
        try:
            url = f"https://hubeau.eaufrance.fr/api/v1/niveaux_nappes/stations?code_departement={dept}&size=50"
            response = requests.get(url, timeout=10)
            if response.status_code == 200:
                data = response.json()

                for station in data.get('data', []):
                    if station.get('geometry_x') and station.get('geometry_y'):
                        feature = {
                            "type": "Feature",
                            "geometry": {
                                "type": "Point",
                                "coordinates": [
                                    float(station['geometry_x']),
                                    float(station['geometry_y'])
                                ]
                            },
                            "properties": {
                                "name": f"{station.get('nom_commune', 'Unknown')} - Piézomètre {station.get('code_bss', '')}",
                                "type": "Piézomètre",
                                "category": "nappe_phreatique",
                                "layer": "Piézomètres",
                                "commune": station.get('nom_commune', ''),
                                "departement": station.get('nom_departement', 'Grand Est'),
                                "code_bss": station.get('code_bss', ''),
                                "altitude_sol": str(station.get('altitude_station', '')),
                                "date_debut": station.get('date_debut_mesure', ''),
                                "profondeur": float(station.get('profondeur_investigation', 10)),
                                "color": "#4169E1",
                                "validFrom": "2024-01-01T00:00:00Z",
                                "validTo": None
                            }
                        }
                        features.append(feature)
        except Exception as e:
            print(f"Erreur pour le département {dept}: {e}")

    return features

def create_synthetic_piezometers():
    """Crée des piézomètres synthétiques si l'API ne répond pas"""
    features = []

    # Liste de communes du Grand Est
    communes = [
        ("Strasbourg", 7.75, 48.58, "Bas-Rhin"),
        ("Mulhouse", 7.34, 47.75, "Haut-Rhin"),
        ("Colmar", 7.36, 48.08, "Haut-Rhin"),
        ("Metz", 6.18, 49.12, "Moselle"),
        ("Nancy", 6.18, 48.69, "Meurthe-et-Moselle"),
        ("Reims", 4.03, 49.26, "Marne"),
        ("Charleville-Mézières", 4.72, 49.77, "Ardennes"),
        ("Troyes", 4.08, 48.30, "Aube"),
        ("Chaumont", 5.14, 48.11, "Haute-Marne"),
        ("Bar-le-Duc", 5.16, 48.77, "Meuse"),
        ("Épinal", 6.45, 48.17, "Vosges"),
        ("Saint-Dizier", 4.95, 48.64, "Haute-Marne"),
        ("Verdun", 5.38, 49.16, "Meuse"),
        ("Haguenau", 7.79, 48.82, "Bas-Rhin"),
        ("Schiltigheim", 7.75, 48.61, "Bas-Rhin"),
        ("Illkirch-Graffenstaden", 7.71, 48.53, "Bas-Rhin"),
        ("Saint-Louis", 7.56, 47.59, "Haut-Rhin"),
        ("Sélestat", 7.45, 48.26, "Bas-Rhin"),
        ("Bischheim", 7.75, 48.62, "Bas-Rhin"),
        ("Thionville", 6.17, 49.36, "Moselle"),
        ("Montigny-lès-Metz", 6.15, 49.10, "Moselle"),
        ("Sarreguemines", 7.07, 49.11, "Moselle"),
        ("Forbach", 6.90, 49.19, "Moselle"),
        ("Saint-Avold", 6.70, 49.10, "Moselle"),
        ("Vandœuvre-lès-Nancy", 6.17, 48.66, "Meurthe-et-Moselle"),
        ("Lunéville", 6.50, 48.59, "Meurthe-et-Moselle"),
        ("Pont-à-Mousson", 6.05, 48.90, "Meurthe-et-Moselle"),
        ("Longwy", 5.76, 49.52, "Meurthe-et-Moselle"),
        ("Châlons-en-Champagne", 4.36, 48.96, "Marne"),
        ("Vitry-le-François", 4.58, 48.72, "Marne"),
        ("Épernay", 3.96, 49.04, "Marne"),
        ("Sedan", 4.94, 49.70, "Ardennes"),
        ("Romilly-sur-Seine", 3.73, 48.52, "Aube"),
        ("La Chapelle-Saint-Luc", 4.04, 48.31, "Aube"),
        ("Saint-Dié-des-Vosges", 6.95, 48.29, "Vosges"),
        ("Gérardmer", 6.88, 48.07, "Vosges"),
        ("Remiremont", 6.59, 48.02, "Vosges")
    ]

    # Générer plusieurs piézomètres par commune
    for commune, lon, lat, dept in communes:
        for i in range(random.randint(2, 5)):
            # Légère variation des coordonnées
            lon_var = lon + random.uniform(-0.05, 0.05)
            lat_var = lat + random.uniform(-0.05, 0.05)
            code = f"0{random.randint(1000,9999)}X{random.randint(100,999)}/F{i+1}"

            feature = {
                "type": "Feature",
                "geometry": {
                    "type": "Point",
                    "coordinates": [lon_var, lat_var]
                },
                "properties": {
                    "name": f"{commune} - Piézomètre {code}",
                    "type": "Piézomètre",
                    "category": "nappe_phreatique",
                    "layer": "Piézomètres",
                    "commune": commune,
                    "departement": dept,
                    "code_bss": code,
                    "altitude_sol": str(random.randint(100, 400)),
                    "date_debut": f"{random.randint(1960, 2020)}-01-01",
                    "profondeur": random.randint(5, 50),
                    "color": "#4169E1",
                    "validFrom": "2024-01-01T00:00:00Z",
                    "validTo": None
                }
            }
            features.append(feature)

    return features

def create_water_courses():
    """Crée les cours d'eau"""
    courses = [
        {
            "name": "Rhin - Section Alsace",
            "coords": [[7.588, 48.966], [7.734, 48.585], [7.909, 47.916], [8.232, 47.591]],
            "debit": 1080,
            "qualite": "Bonne",
            "bassin": "Rhin-Meuse"
        },
        {
            "name": "Moselle",
            "coords": [[6.179, 49.120], [6.365, 48.689], [6.737, 48.114], [7.361, 47.815]],
            "debit": 145,
            "qualite": "Moyenne",
            "bassin": "Rhin-Meuse"
        },
        {
            "name": "Meuse",
            "coords": [[5.379, 49.540], [5.168, 48.640], [4.863, 47.980]],
            "debit": 230,
            "qualite": "Bonne",
            "bassin": "Rhin-Meuse"
        },
        {
            "name": "Marne",
            "coords": [[4.363, 48.950], [4.796, 48.640], [5.139, 48.260]],
            "debit": 110,
            "qualite": "Moyenne",
            "bassin": "Seine-Normandie"
        },
        {
            "name": "Ill",
            "coords": [[7.339, 48.584], [7.448, 48.318], [7.581, 47.590]],
            "debit": 58,
            "qualite": "Bonne",
            "bassin": "Rhin-Meuse"
        },
        {
            "name": "Sarre",
            "coords": [[7.065, 49.111], [7.032, 48.980], [6.982, 48.795]],
            "debit": 78,
            "qualite": "Moyenne",
            "bassin": "Rhin-Meuse"
        },
        {
            "name": "Aisne",
            "coords": [[4.521, 49.474], [4.686, 49.256], [4.822, 49.071]],
            "debit": 63,
            "qualite": "Bonne",
            "bassin": "Seine-Normandie"
        },
        {
            "name": "Aube",
            "coords": [[4.075, 48.297], [4.329, 48.402], [4.593, 48.516]],
            "debit": 41,
            "qualite": "Bonne",
            "bassin": "Seine-Normandie"
        },
        {
            "name": "Seine (source)",
            "coords": [[4.205, 48.088], [4.341, 48.167], [4.498, 48.275]],
            "debit": 25,
            "qualite": "Excellente",
            "bassin": "Seine-Normandie"
        },
        {
            "name": "Seille",
            "coords": [[6.532, 48.914], [6.401, 48.792], [6.241, 48.638]],
            "debit": 35,
            "qualite": "Moyenne",
            "bassin": "Rhin-Meuse"
        }
    ]

    features = []
    for course in courses:
        feature = {
            "type": "Feature",
            "geometry": {
                "type": "LineString",
                "coordinates": course["coords"]
            },
            "properties": {
                "name": course["name"],
                "type": "Cours d'eau",
                "category": "cours_eau",
                "layer": "Rivières",
                "debit_moyen_m3s": course["debit"],
                "qualite_eau": course["qualite"],
                "bassin": course["bassin"],
                "color": "#1E90FF",
                "validFrom": "2024-01-01T00:00:00Z",
                "validTo": None
            }
        }
        features.append(feature)

    return features

def create_lakes():
    """Crée les lacs et plans d'eau"""
    lakes = [
        {"name": "Lac de Gérardmer", "coords": [6.852, 48.073], "surface": 115, "prof": 38, "dept": "Vosges"},
        {"name": "Lac de Pierre-Percée", "coords": [6.933, 48.466], "surface": 304, "prof": 78, "dept": "Meurthe-et-Moselle"},
        {"name": "Lac du Der-Chantecoq", "coords": [4.770, 48.586], "surface": 4800, "prof": 20, "dept": "Marne/Haute-Marne"},
        {"name": "Lac de Madine", "coords": [5.743, 48.918], "surface": 1100, "prof": 15, "dept": "Meuse"},
        {"name": "Lac Blanc", "coords": [7.094, 48.137], "surface": 29, "prof": 72, "dept": "Haut-Rhin"},
        {"name": "Lac de Kruth-Wildenstein", "coords": [6.966, 47.953], "surface": 81, "prof": 35, "dept": "Haut-Rhin"},
        {"name": "Lac de Longemer", "coords": [6.931, 48.075], "surface": 76, "prof": 34, "dept": "Vosges"},
        {"name": "Lac de Retournemer", "coords": [6.903, 48.090], "surface": 5, "prof": 16, "dept": "Vosges"},
        {"name": "Lac des Corbeaux", "coords": [6.892, 48.041], "surface": 9, "prof": 27, "dept": "Vosges"},
        {"name": "Plan d'eau de Metz", "coords": [6.233, 49.113], "surface": 62, "prof": 4, "dept": "Moselle"},
        {"name": "Étang du Stock", "coords": [6.781, 48.754], "surface": 700, "prof": 8, "dept": "Moselle"},
        {"name": "Lac de Bouzey", "coords": [6.362, 48.169], "surface": 140, "prof": 14, "dept": "Vosges"},
        {"name": "Lac de la Liez", "coords": [5.274, 48.016], "surface": 290, "prof": 16, "dept": "Haute-Marne"},
        {"name": "Lac de la Mouche", "coords": [5.185, 47.952], "surface": 94, "prof": 12, "dept": "Haute-Marne"},
        {"name": "Lac de la Vingeanne", "coords": [5.403, 47.915], "surface": 196, "prof": 21, "dept": "Haute-Marne"}
    ]

    features = []
    for lake in lakes:
        feature = {
            "type": "Feature",
            "geometry": {
                "type": "Point",
                "coordinates": lake["coords"]
            },
            "properties": {
                "name": lake["name"],
                "type": "Lac",
                "category": "plan_eau",
                "layer": "Lacs et plans d'eau",
                "surface_ha": lake["surface"],
                "profondeur_max": lake["prof"],
                "departement": lake["dept"],
                "color": "#00CED1",
                "validFrom": "2024-01-01T00:00:00Z",
                "validTo": None
            }
        }
        features.append(feature)

    return features

def create_infrastructures():
    """Crée les infrastructures hydrauliques"""
    infras = [
        {"name": "Barrage de Kembs", "coords": [7.502, 47.691], "type": "Barrage", "capacite": "52 MW", "dept": "Haut-Rhin"},
        {"name": "Station épuration Strasbourg", "coords": [7.795, 48.527], "type": "Station épuration", "capacite": "450000 EH", "dept": "Bas-Rhin"},
        {"name": "Station épuration Nancy", "coords": [6.208, 48.663], "type": "Station épuration", "capacite": "300000 EH", "dept": "Meurthe-et-Moselle"},
        {"name": "Station épuration Metz", "coords": [6.195, 49.095], "type": "Station épuration", "capacite": "230000 EH", "dept": "Moselle"},
        {"name": "Station épuration Reims", "coords": [4.025, 49.231], "type": "Station épuration", "capacite": "250000 EH", "dept": "Marne"},
        {"name": "Station épuration Mulhouse", "coords": [7.312, 47.732], "type": "Station épuration", "capacite": "180000 EH", "dept": "Haut-Rhin"},
        {"name": "Station épuration Colmar", "coords": [7.385, 48.103], "type": "Station épuration", "capacite": "120000 EH", "dept": "Haut-Rhin"},
        {"name": "Station épuration Troyes", "coords": [4.101, 48.279], "type": "Station épuration", "capacite": "150000 EH", "dept": "Aube"},
        {"name": "Station épuration Châlons", "coords": [4.384, 48.943], "type": "Station épuration", "capacite": "100000 EH", "dept": "Marne"},
        {"name": "Station épuration Épinal", "coords": [6.469, 48.186], "type": "Station épuration", "capacite": "80000 EH", "dept": "Vosges"},
        {"name": "Barrage de Pierre-Percée", "coords": [6.933, 48.466], "type": "Barrage", "capacite": "61.6 hm³", "dept": "Meurthe-et-Moselle"},
        {"name": "Barrage du Der-Chantecoq", "coords": [4.770, 48.586], "type": "Barrage", "capacite": "350 hm³", "dept": "Marne"},
        {"name": "Barrage de Kruth-Wildenstein", "coords": [6.966, 47.953], "type": "Barrage", "capacite": "12 hm³", "dept": "Haut-Rhin"},
        {"name": "Usine hydroélectrique Ottmarsheim", "coords": [7.506, 47.787], "type": "Centrale hydraulique", "capacite": "156 MW", "dept": "Haut-Rhin"},
        {"name": "Usine hydroélectrique Fessenheim", "coords": [7.563, 47.914], "type": "Centrale hydraulique", "capacite": "173 MW", "dept": "Haut-Rhin"},
        {"name": "Écluse de Gambsheim", "coords": [7.916, 48.695], "type": "Écluse", "capacite": "Grand gabarit", "dept": "Bas-Rhin"},
        {"name": "Écluse de Strasbourg", "coords": [7.807, 48.542], "type": "Écluse", "capacite": "Grand gabarit", "dept": "Bas-Rhin"},
        {"name": "Port de Strasbourg", "coords": [7.795, 48.556], "type": "Port fluvial", "capacite": "8 Mt/an", "dept": "Bas-Rhin"},
        {"name": "Port de Metz", "coords": [6.218, 49.103], "type": "Port fluvial", "capacite": "3 Mt/an", "dept": "Moselle"},
        {"name": "Port de Nancy", "coords": [6.165, 48.704], "type": "Port fluvial", "capacite": "1 Mt/an", "dept": "Meurthe-et-Moselle"}
    ]

    features = []
    for infra in infras:
        feature = {
            "type": "Feature",
            "geometry": {
                "type": "Point",
                "coordinates": infra["coords"]
            },
            "properties": {
                "name": infra["name"],
                "type": infra["type"],
                "category": "infrastructure",
                "layer": "Infrastructures",
                "capacite": infra["capacite"],
                "departement": infra["dept"],
                "color": "#FFD700",
                "validFrom": "2024-01-01T00:00:00Z",
                "validTo": None
            }
        }
        features.append(feature)

    return features

def main():
    """Génère le fichier GeoJSON complet"""
    print("Génération des données du Grand Est...")

    all_features = []

    # Essayer de récupérer les vraies données
    print("Tentative de récupération des données Hub'Eau...")
    real_piezos = fetch_piezometers()

    if len(real_piezos) > 0:
        print(f"✓ {len(real_piezos)} piézomètres réels récupérés")
        all_features.extend(real_piezos)
    else:
        print("✗ Pas de données Hub'Eau, génération de données synthétiques...")
        synthetic_piezos = create_synthetic_piezometers()
        print(f"✓ {len(synthetic_piezos)} piézomètres synthétiques créés")
        all_features.extend(synthetic_piezos)

    # Ajouter les autres éléments
    courses = create_water_courses()
    print(f"✓ {len(courses)} cours d'eau ajoutés")
    all_features.extend(courses)

    lakes = create_lakes()
    print(f"✓ {len(lakes)} lacs ajoutés")
    all_features.extend(lakes)

    infras = create_infrastructures()
    print(f"✓ {len(infras)} infrastructures ajoutées")
    all_features.extend(infras)

    # Créer le GeoJSON
    geojson = {
        "type": "FeatureCollection",
        "features": all_features
    }

    # Sauvegarder
    output_file = r"C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\Scripts\grand_est_eau_complete.geojson"
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(geojson, f, ensure_ascii=False, indent=2)

    print(f"\n✅ Fichier généré avec succès : {output_file}")
    print(f"📊 Total : {len(all_features)} éléments")

    # Copier vers runtime
    import shutil
    runtime_path = r"C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\bin\Debug\net9.0\Scripts\grand_est_eau_complet.geojson"
    shutil.copy(output_file, runtime_path)
    print(f"✓ Fichier copié vers runtime")

if __name__ == "__main__":
    main()