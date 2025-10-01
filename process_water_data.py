import json
import os
from datetime import datetime

def process_water_stations():
    """Process water quality stations data from Hub'Eau API"""
    features = []

    # Process water quality stations
    stations_file = r"C:\Users\MaximeRAZAFINJATO\POC-SIG\stations_grand_est.json"
    if os.path.exists(stations_file):
        with open(stations_file, 'r', encoding='utf-8') as f:
            data = json.load(f)

        for station in data.get('data', []):
            if station.get('longitude_station') and station.get('latitude_station'):
                feature = {
                    "type": "Feature",
                    "properties": {
                        "name": station.get('libelle_station', 'Station inconnue'),
                        "code_station": station.get('code_station', ''),
                        "type": "station_qualite",
                        "category": "surveillance",
                        "commune": station.get('libelle_commune', ''),
                        "code_commune": station.get('code_commune', ''),
                        "departement": station.get('libelle_departement', ''),
                        "cours_eau": station.get('libelle_cours_eau', ''),
                        "bassin": station.get('libelle_bassin', ''),
                        "layer": "Stations qualité eau",
                        "validFrom": "2024-01-01T00:00:00Z",
                        "validTo": None,
                        "color": "#00FF00"
                    },
                    "geometry": {
                        "type": "Point",
                        "coordinates": [
                            float(station.get('longitude_station')),
                            float(station.get('latitude_station'))
                        ]
                    }
                }
                features.append(feature)
        print(f"Processed {len(features)} water quality stations")

    # Process hydrometric stations
    hydro_file = r"C:\Users\MaximeRAZAFINJATO\POC-SIG\stations_hydro_grand_est.json"
    if os.path.exists(hydro_file):
        with open(hydro_file, 'r', encoding='utf-8') as f:
            data = json.load(f)

        hydro_count = 0
        for station in data.get('data', []):
            if station.get('longitude_station') and station.get('latitude_station'):
                feature = {
                    "type": "Feature",
                    "properties": {
                        "name": station.get('libelle_station', 'Station hydro inconnue'),
                        "code_station": station.get('code_station', ''),
                        "type": "station_hydrometrie",
                        "category": "mesure_debit",
                        "commune": station.get('libelle_commune', ''),
                        "departement": station.get('libelle_departement', ''),
                        "cours_eau": station.get('libelle_cours_eau', ''),
                        "bassin": station.get('libelle_bassin', ''),
                        "altitude": station.get('altitude_ref_alti_station', 0),
                        "en_service": station.get('en_service', True),
                        "layer": "Stations hydrométriques",
                        "validFrom": "2024-01-01T00:00:00Z",
                        "validTo": None,
                        "color": "#0066CC"
                    },
                    "geometry": {
                        "type": "Point",
                        "coordinates": [
                            float(station.get('longitude_station')),
                            float(station.get('latitude_station'))
                        ]
                    }
                }
                features.append(feature)
                hydro_count += 1
        print(f"Processed {hydro_count} hydrometric stations")

    # Process piezometric stations (groundwater)
    piezo_file = r"C:\Users\MaximeRAZAFINJATO\POC-SIG\piezometres_grand_est.json"
    if os.path.exists(piezo_file):
        with open(piezo_file, 'r', encoding='utf-8') as f:
            data = json.load(f)

        piezo_count = 0
        for station in data.get('data', []):
            if station.get('x') and station.get('y'):
                feature = {
                    "type": "Feature",
                    "properties": {
                        "name": station.get('nom_commune', '') + ' - Piézomètre ' + station.get('code_bss', ''),
                        "code_bss": station.get('code_bss', ''),
                        "type": "piezometre",
                        "category": "nappe_phreatique",
                        "commune": station.get('nom_commune', ''),
                        "departement": station.get('nom_departement', ''),
                        "nappe": station.get('libelle_pe', ''),
                        "profondeur": station.get('profondeur_investigation', 0),
                        "altitude_sol": station.get('altitude_station', 0),
                        "date_debut": station.get('date_debut_mesure', ''),
                        "layer": "Piézomètres",
                        "validFrom": "2024-01-01T00:00:00Z",
                        "validTo": None,
                        "color": "#4169E1"
                    },
                    "geometry": {
                        "type": "Point",
                        "coordinates": [
                            float(station.get('x')),
                            float(station.get('y'))
                        ]
                    }
                }
                features.append(feature)
                piezo_count += 1
        print(f"Processed {piezo_count} piezometric stations")

    # Add major rivers of Grand Est
    rivers = [
        {
            "name": "Rhin",
            "type": "fleuve",
            "debit_moyen": 1080,
            "longueur_km": 185,
            "bassin": "Rhin-Meuse",
            "coords": [[7.5885, 48.9660], [7.7342, 48.5849], [7.9090, 47.9163], [8.2324, 47.5905]]
        },
        {
            "name": "Moselle",
            "type": "riviere",
            "debit_moyen": 145,
            "longueur_km": 314,
            "bassin": "Rhin-Meuse",
            "coords": [[6.1786, 49.1196], [6.3647, 48.6891], [6.7369, 48.1138], [7.3608, 47.8148]]
        },
        {
            "name": "Meuse",
            "type": "fleuve",
            "debit_moyen": 230,
            "longueur_km": 272,
            "bassin": "Rhin-Meuse",
            "coords": [[5.1657, 49.5695], [5.3689, 49.2924], [5.5264, 48.7901], [5.8923, 48.2345]]
        },
        {
            "name": "Marne",
            "type": "riviere",
            "debit_moyen": 110,
            "longueur_km": 180,
            "bassin": "Seine-Normandie",
            "coords": [[4.3634, 49.2739], [4.7256, 48.9567], [5.1367, 48.6389]]
        },
        {
            "name": "Ill",
            "type": "riviere",
            "debit_moyen": 58,
            "longueur_km": 223,
            "bassin": "Rhin-Meuse",
            "coords": [[7.2384, 47.4523], [7.4485, 48.2698], [7.7528, 48.5825]]
        },
        {
            "name": "Meurthe",
            "type": "riviere",
            "debit_moyen": 40,
            "longueur_km": 161,
            "bassin": "Rhin-Meuse",
            "coords": [[6.1849, 48.6921], [6.4567, 48.5234], [6.7234, 48.4123]]
        },
        {
            "name": "Sarre",
            "type": "riviere",
            "debit_moyen": 75,
            "longueur_km": 126,
            "bassin": "Rhin-Meuse",
            "coords": [[7.0234, 49.1123], [6.8456, 48.9234], [6.6789, 48.7345]]
        }
    ]

    for river in rivers:
        feature = {
            "type": "Feature",
            "properties": {
                "name": river["name"],
                "type": river["type"],
                "category": "cours_eau",
                "debit_moyen_m3s": river["debit_moyen"],
                "longueur_km": river["longueur_km"],
                "bassin": river["bassin"],
                "layer": "Cours d'eau",
                "validFrom": "2024-01-01T00:00:00Z",
                "validTo": None,
                "color": "#0099FF"
            },
            "geometry": {
                "type": "LineString",
                "coordinates": river["coords"]
            }
        }
        features.append(feature)
    print(f"Added {len(rivers)} major rivers")

    # Add major lakes
    lakes = [
        {
            "name": "Lac de Gérardmer",
            "surface_ha": 115,
            "profondeur_max": 38,
            "altitude": 660,
            "type": "naturel",
            "center": [6.8623, 48.0789],
            "bounds": [[6.8458, 48.0723], [6.8789, 48.0723], [6.8789, 48.0856], [6.8458, 48.0856], [6.8458, 48.0723]]
        },
        {
            "name": "Lac du Der-Chantecoq",
            "surface_ha": 4800,
            "volume_millions_m3": 350,
            "type": "artificiel",
            "usage": "régulation_crues",
            "center": [4.7678, 48.5718],
            "bounds": [[4.7234, 48.5423], [4.8123, 48.5423], [4.8123, 48.6012], [4.7234, 48.6012], [4.7234, 48.5423]]
        },
        {
            "name": "Lac de Madine",
            "surface_ha": 1100,
            "type": "artificiel",
            "usage": "loisirs",
            "center": [5.7456, 48.9234],
            "bounds": [[5.7234, 48.9123], [5.7678, 48.9123], [5.7678, 48.9345], [5.7234, 48.9345], [5.7234, 48.9123]]
        }
    ]

    for lake in lakes:
        feature = {
            "type": "Feature",
            "properties": {
                "name": lake["name"],
                "type": f"lac_{lake['type']}",
                "category": "plan_eau",
                "surface_ha": lake["surface_ha"],
                "layer": "Plans d'eau",
                "validFrom": "2024-01-01T00:00:00Z",
                "validTo": None,
                "color": "#00CCFF"
            },
            "geometry": {
                "type": "Polygon",
                "coordinates": [lake["bounds"]]
            }
        }
        features.append(feature)
    print(f"Added {len(lakes)} major lakes")

    # Create the final GeoJSON structure
    geojson = {
        "type": "FeatureCollection",
        "features": features
    }

    # Save to file
    output_file = r"C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\Scripts\grand_est_eau_complet.geojson"
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(geojson, f, ensure_ascii=False, indent=2)

    print(f"\nTotal features: {len(features)}")
    print(f"File saved to: {output_file}")

    # Also save to build directory
    build_output = r"C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\bin\Debug\net9.0\Scripts\grand_est_eau_complet.geojson"
    with open(build_output, 'w', encoding='utf-8') as f:
        json.dump(geojson, f, ensure_ascii=False, indent=2)
    print(f"Also saved to build directory: {build_output}")

if __name__ == "__main__":
    process_water_stations()