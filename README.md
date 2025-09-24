# POC-SIG - Système d'Information Géographique

Un système d'information géographique moderne utilisant .NET 9 et React TypeScript avec des capacités d'export avancées.

## 🚀 Fonctionnalités

### Cartographie Interactive
- Cartes interactives avec Leaflet
- Sélection spatiale d'éléments géographiques (rectangle de sélection)
- Interface moderne avec design glassmorphism
- Mode sombre/clair
- Popups détaillés pour chaque élément

### Gestion de Couches
- Gestion de couches géographiques multiples
- Couche "Default" pour vue d'ensemble de toutes les couches
- Création de nouvelles couches dynamique
- Chargement de données de démonstration

### Export de Données Avancé
- **Export complet par couche** (GeoJSON/CSV)
- **Export des éléments sélectionnés spatialement** (GeoJSON/CSV)
- Gestion spéciale de la couche "Default" (agrégation de toutes les couches)
- Export avec métadonnées complètes

### Requêtes Spatiales
- Support des opérations spatiales (intersects, within)
- Filtrages spatiaux et temporels
- Pagination des résultats
- Buffer distance pour les requêtes

## 🏗️ Architecture Technique

- **Frontend**: React 18 + TypeScript + Vite
- **Backend**: .NET 9 Web API
- **Base de données**: SQL Server avec support spatial (NetTopologySuite)
- **Cartographie**: Leaflet avec plugins
- **Containerisation**: Docker pour SQL Server
- **Styling**: CSS moderne avec glassmorphism

## 🛠️ Installation et Démarrage

### Prérequis
- Node.js 18+ et pnpm
- .NET 9 SDK
- Docker Desktop

### 1. Base de données
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Strong_password_123!" -p 1433:1433 --name sqlserver-poc-sig -d mcr.microsoft.com/mssql/server:2022-latest
```

### 2. Backend (.NET 9)
```bash
cd poc-sig/backend
dotnet restore
dotnet run
```
Le backend sera accessible sur `http://localhost:5050`

### 3. Frontend (React TypeScript)
```bash
cd poc-sig/frontend
pnpm install
pnpm dev --port 3000
```
Le frontend sera accessible sur `http://localhost:3000`

## 📁 Structure du Projet

```
POC-SIG/
├── poc-sig/
│   ├── backend/                    # API .NET 9
│   │   ├── Controllers/
│   │   │   ├── FeaturesController.cs
│   │   │   ├── ExportController.cs
│   │   │   └── AdminController.cs
│   │   ├── Domain/                 # Entités et modèles
│   │   ├── Infrastructure/         # DbContext et configuration
│   │   └── Program.cs              # Point d'entrée
│   └── frontend/                   # React TypeScript
│       ├── src/
│       │   ├── components/
│       │   │   └── SelectionPanel.tsx
│       │   ├── map/
│       │   │   └── ModernMapView.tsx
│       │   ├── api/
│       │   │   └── client.ts
│       │   ├── types/
│       │   │   └── api.ts
│       │   └── styles/
│       │       └── modern.css
│       └── package.json
├── CLAUDE.md                       # Documentation technique
└── README.md
```

## 🌐 API Endpoints

### Features
- `GET /api/features/{layerId}` - Récupérer les features d'une couche
- `GET /api/features/{layerId}/stats` - Statistiques d'une couche
- `POST /api/features` - Créer une nouvelle feature

### Export
- `GET /api/export/{layerId}/geojson` - Exporter une couche en GeoJSON
- `GET /api/export/{layerId}/csv` - Exporter une couche en CSV

### Administration
- `POST /api/admin/clean-database` - Nettoyer la base de données
- `POST /api/admin/load-demo-data` - Charger les données de démonstration

## 💡 Guide d'Utilisation

### Sélection Spatiale
1. Activer le mode sélection avec l'icône de curseur
2. Dessiner un rectangle sur la carte
3. Les éléments sélectionnés apparaissent dans le panel latéral
4. Exporter la sélection via les boutons dédiés

### Export des Données
- **Export de couche complète**: Utiliser les boutons dans "Export des données"
- **Export de sélection**: Utiliser les boutons dans "Export sélection" après avoir sélectionné des éléments
- **Couche Default**: Exporte automatiquement toutes les couches combinées

### Gestion des Couches
- Créer de nouvelles couches via le bouton "+"
- Charger des données de démonstration avec le bouton dédié
- Basculer entre les couches avec le sélecteur

## 🎨 Interface Utilisateur

### Design Modern
- Interface glassmorphism avec effets de transparence
- Animations fluides et micro-interactions
- Thème sombre/clair adaptatif
- Design responsive pour mobile et desktop

### Composants Principaux
- **ModernMapView**: Composant carte principal avec tous les contrôles
- **SelectionPanel**: Panel flottant pour la gestion de la sélection
- **Sections d'export**: Deux sections distinctes pour l'export complet et la sélection

## 🔧 Configuration

### Ports de Développement
- **Frontend**: Port 3000 (CORS configuré)
- **Backend**: Port 5050
- **SQL Server**: Port 1433 (Docker)

### Variables d'Environnement
Le backend est configuré pour accepter les requêtes CORS depuis localhost:3000

## 📊 Données de Démonstration

Le projet inclut des jeux de données de démonstration pour Paris :
- Monuments parisiens
- Parcs et jardins
- Lignes de métro
- Musées

Charger via le bouton "Charger les données de démo"

## 🚀 Déploiement

### Production
1. Build du frontend : `pnpm run build`
2. Build du backend : `dotnet publish -c Release`
3. Déployer sur votre infrastructure préférée

### Docker (Optionnel)
Des Dockerfiles peuvent être ajoutés pour containeriser l'ensemble de l'application.

## 🤝 Contribution

Ce projet est un POC de démonstration. Pour contribuer :
1. Fork le projet
2. Créer une branche feature
3. Commit les changements
4. Créer une Pull Request

## 📝 Licence

Projet de démonstration - Usage libre pour l'apprentissage et le développement.

---

**POC-SIG** - Démonstration des capacités modernes d'un SIG avec React et .NET 9