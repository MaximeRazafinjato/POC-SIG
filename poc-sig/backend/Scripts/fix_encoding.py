#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Fix UTF-8 encoding issues in GeoJSON files"""

import json
import codecs

input_file = r"C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\Scripts\grand_est_eau_complet_fixed.geojson"
output_file = r"C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\Scripts\grand_est_eau_complet.geojson"
runtime_file = r"C:\Users\MaximeRAZAFINJATO\POC-SIG\poc-sig\backend\bin\Debug\net9.0\Scripts\grand_est_eau_complet.geojson"

print("Fixing UTF-8 encoding issues...")

# Read the file with potential encoding issues
with open(input_file, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

# Fix common mojibake patterns
replacements = {
    'Ã©': 'é',
    'Ã¨': 'è',
    'Ãª': 'ê',
    'Ã ': 'à',
    'Ã¢': 'â',
    'Ã´': 'ô',
    'Ã»': 'û',
    'Ã§': 'ç',
    'Ã®': 'î',
    'Ã¯': 'ï',
    'Ã‰': 'É',
    'Ãˆ': 'È',
    'ÃŠ': 'Ê',
    'Ã€': 'À',
    'Ã‚': 'Â',
    'Ã"': 'Ô',
    'Ã›': 'Û',
    'Ã‡': 'Ç',
    'ÃŽ': 'Î',
    'Ã': 'Ï',
    'PiÃ©zomÃ¨tre': 'Piézomètre',
    'RiviÃ¨res': 'Rivières',
    'GÃ©rardmer': 'Gérardmer',
    'Pierre-PercÃ©e': 'Pierre-Percée',
    'Ã©puration': 'épuration'
}

for old, new in replacements.items():
    content = content.replace(old, new)

# Parse as JSON to validate
try:
    data = json.loads(content)

    # Additional cleanup in properties
    for feature in data.get('features', []):
        props = feature.get('properties', {})
        for key, value in props.items():
            if isinstance(value, str):
                for old, new in replacements.items():
                    if old in value:
                        props[key] = value.replace(old, new)

    # Save with proper UTF-8 encoding
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=2)

    # Copy to runtime directory
    with open(runtime_file, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=2)

    print(f"✓ Fixed encoding and saved to: {output_file}")
    print(f"✓ Copied to runtime directory: {runtime_file}")
    print("✅ UTF-8 encoding successfully fixed!")

except json.JSONDecodeError as e:
    print(f"Error parsing JSON: {e}")
except Exception as e:
    print(f"Error: {e}")