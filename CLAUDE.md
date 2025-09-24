# POC-SIG - Système d'Information Géographique

## Description du Projet
POC-SIG est une application de démonstration d'un système d'information géographique moderne utilisant .NET 9 pour le backend et React TypeScript pour le frontend.

## Architecture
- **Frontend**: React TypeScript avec Vite, Leaflet pour la cartographie
- **Backend**: .NET 9 Web API avec Entity Framework Core
- **Base de données**: SQL Server avec support spatial (NetTopologySuite)
- **Containerisation**: Docker pour SQL Server

## Ports de développement
- **Frontend**: Port 3000 (CORS configuré)
- **Backend**: Port 5050
- **SQL Server**: Port 1433 (Docker)

## Fonctionnalités principales
- Affichage de cartes interactives avec Leaflet
- Sélection spatiale d'éléments géographiques (rectangle de sélection)
- Interface moderne avec design glassmorphism
- Support des requêtes spatiales (intersects, within)
- Pagination des résultats
- Gestion de couches (layers) géographiques
- **Export de données** :
  - Export complet par couche (GeoJSON/CSV)
  - Export des éléments sélectionnés spatialement (GeoJSON/CSV)
  - Gestion spéciale de la couche "Default" (agrégation de toutes les couches)

## Commandes de démarrage

### Base de données
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Strong_password_123!" -p 1433:1433 --name sqlserver-poc-sig -d mcr.microsoft.com/mssql/server:2022-latest
```

### Backend (.NET 9)
```bash
cd poc-sig/backend
dotnet run
```

### Frontend (React TypeScript)
```bash
cd poc-sig/frontend
pnpm dev --port 3000
```

## Structure du projet
```
POC-SIG/
├── poc-sig/
│   ├── backend/
│   │   ├── Controllers/
│   │   │   ├── FeaturesController.cs
│   │   │   └── AdminController.cs
│   │   ├── Domain/
│   │   ├── Infrastructure/
│   │   └── Program.cs
│   └── frontend/
│       ├── src/
│       │   ├── components/
│       │   │   └── SelectionPanel.tsx (UI premium redesigné)
│       │   ├── types/
│       │   │   └── api.ts
│       │   └── pages/
│       └── package.json
└── CLAUDE.md
```

## API Endpoints
- `GET /api/features/{layerId}` - Récupérer les features d'une couche
- `GET /api/features/{layerId}/stats` - Statistiques d'une couche
- `POST /api/features` - Créer une nouvelle feature
- `GET /api/export/{layerId}/geojson` - Exporter une couche en GeoJSON
- `GET /api/export/{layerId}/csv` - Exporter une couche en CSV
- `POST /api/admin/clean-database` - Nettoyer la base de données
- `POST /api/admin/load-demo-data` - Charger les données de démonstration

## Types TypeScript
```typescript
interface Feature {
  id: number;
  layerId: number;
  propertiesJson?: string;
  properties?: Record<string, any>;
  geometry: any;
  validFromUtc: string;
  validToUtc?: string;
}
```

## Composants UI principaux
- **SelectionPanel**: Panel flottant avec design glassmorphism pour la sélection spatiale
- **ModernMapView**: Composant carte avec Leaflet et contrôles de sélection
  - Section "Export des données" : Export complet par couche
  - Section "Export sélection" : Export des éléments sélectionnés spatialement
  - Gestion automatique de la couche "Default" (agrégation des autres couches)

## Configuration CORS
Le backend est configuré pour accepter les requêtes du frontend sur localhost:3000.

## Notes de développement
- Utiliser Docker pour SQL Server en développement
- Le frontend doit tourner sur le port 3000 pour la configuration CORS
- Les migrations EF Core sont automatiquement appliquées au démarrage