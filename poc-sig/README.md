# POC SIG - Système d'Information Géographique

POC Full-stack d'un système SIG avec backend .NET 9 API, SQL Server 2022 avec données spatiales, et frontend React 19 + Vite avec carte Leaflet.

## Fonctionnalités

- 🗺️ **Carte interactive** : Visualisation des données géospatiales avec Leaflet
- 🔍 **Requêtes spatiales** : Bbox, within, intersects, buffer directement dans SQL Server
- 📅 **Filtre temporel** : Filtrage par dates de validité
- 📥 **Import GeoJSON** : ETL intégré pour importer des fichiers GeoJSON
- 📤 **Exports** : GeoJSON et CSV avec filtres appliqués
- 🚀 **Performance** : Index spatiaux SQL Server, requêtes optimisées

## Prérequis

- Docker Desktop
- .NET SDK 9.0
- Node.js 20+
- pnpm

## Démarrage rapide

### 1. Lancer SQL Server

```bash
cd poc-sig
docker compose up -d
```

Attendre que SQL Server soit prêt (vérifier avec `docker compose logs mssql`).

### 2. Backend API

```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
```

L'API démarre sur http://localhost:5050

### 3. Frontend

```bash
cd frontend
pnpm install
pnpm dev
```

Le frontend démarre sur http://localhost:5174

## Architecture

```
poc-sig/
├── backend/
│   ├── Domain/Entities/      # Entités Layer et FeatureEntity
│   ├── Infrastructure/       # DbContext et configurations EF Core
│   ├── Controllers/          # API REST (Layers, Features, Export)
│   ├── ETL/                 # Import GeoJSON
│   └── Scripts/             # sample.geojson
├── frontend/
│   ├── src/
│   │   ├── api/            # Client API
│   │   ├── map/            # Composants carte
│   │   └── types.ts        # Types TypeScript
└── docker-compose.yml
```

## Variables d'environnement

### Backend (appsettings.json)

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost,1434;Database=poc_sig;User Id=sa;Password=Passw0rd!;TrustServerCertificate=True"
  }
}
```

### Frontend (.env)

```
VITE_API_BASEURL=http://localhost:5050/api
```

## Format des paramètres

### Bbox

Format : `minX,minY,maxX,maxY`
Exemple : `2.29,48.85,2.36,48.87` (Paris centre)

### Opérations spatiales

- `intersects` : Retourne les entités qui intersectent la bbox
- `within` : Retourne les entités entièrement contenues dans la bbox

### Buffer

En mètres, appliqué à la bbox avant filtrage.
Exemple : `500` pour un buffer de 500m

## Guide ETL

### Import d'un fichier GeoJSON

1. Placer votre fichier dans `backend/Scripts/sample.geojson`
2. Via l'interface : cliquer sur "Importer sample.geojson"
3. Via l'API : `POST /api/layers/import?layerId=1`

Le système :
- Parse le GeoJSON
- Convertit automatiquement vers SRID 4326
- Insère par lots de 100 entités
- Reconstruit l'index spatial

### Format GeoJSON attendu

```json
{
  "type": "FeatureCollection",
  "features": [
    {
      "type": "Feature",
      "geometry": {
        "type": "Polygon",
        "coordinates": [[[...]]]
      },
      "properties": {
        "name": "...",
        "type": "..."
      }
    }
  ]
}
```

## Tests API

Utiliser le fichier `backend/Requests/test-api.http` avec un client REST (VS Code REST Client, Postman, etc.)

## Dépannage

### SRID non conforme

Si vos données ont un SRID différent de 4326, l'import effectue une conversion automatique.

### Géométrie invalide

Vérifier que les coordonnées sont dans l'ordre [longitude, latitude] et que les polygones sont fermés.

### Bbox vide

Vérifier l'ordre des coordonnées : minX < maxX et minY < maxY.

### Buffer trop grand

Un buffer important peut créer des géométries très larges. Limiter à quelques kilomètres maximum.

### Base de données non accessible

Vérifier que :
- Docker est lancé
- Le container mssql est en état "healthy"
- Le port 1434 n'est pas utilisé par une autre application

## Performances

- Index spatial sur la colonne Geometry
- Requêtes spatiales exécutées côté SQL Server
- Pagination des résultats (100 par défaut)
- Journalisation des temps d'exécution

## Développement

### Ajouter une migration

```bash
cd backend
dotnet ef migrations add NomDeLaMigration
dotnet ef database update
```

### Build production

Backend :
```bash
cd backend
dotnet publish -c Release
```

Frontend :
```bash
cd frontend
pnpm build
```

## CI/CD

Le workflow GitHub Actions :
- Build et test le backend .NET
- Build et type-check le frontend
- Valide docker-compose
- Génère les artefacts de build

## License

MIT