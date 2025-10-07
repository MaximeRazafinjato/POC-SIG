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

## Plan d'Implémentation Complète POC-SIG

### Phase 1: Architecture et Infrastructure (2-3 semaines)

**Backend - Clean Architecture avec CQRS**
- Restructurer le backend selon l'architecture Clean avec CQRS obligatoire
- Packages requis: MediatR, FluentValidation, Ardalis.Result
- Structure: Application/Common/Base + Application/[Feature]/Management/
- Mappings manuels (pas d'AutoMapper)
- Result pattern pour toutes les réponses

**Nouvelle Structure des Entités Territoriales**
- Commune, EPCI, Département, Région, PNR, BassinVersant
- Tables de référence avec codes INSEE, populations, hiérarchies
- Support spatial avancé avec géométries précises et métadonnées

### Phase 2: Navigation Cartographique Multimodale (3-4 semaines)

**Recherche Intelligente**
- Auto-complétion avancée : communes, codes INSEE, EPCI, coordonnées
- Gestion des homonymies avec désambiguïsation
- Index de recherche optimisé avec Elasticsearch/Lucene.NET
- Temps de réponse < 3 secondes garanti

**Zoom Multi-Échelles et Grille Territoriale**
- Échelles logiques alignées sur les niveaux de décision
- Agrégations dynamiques selon le niveau (commune → EPCI → département)
- Légendes adaptatives avec discrétiseurs configurables
- Conservation des filtres lors des changements d'échelle

**Couches Thématiques Avancées**
- 5 groupes thématiques: Ressource, Usage, Qualité, Pression, Gouvernance
- Légendes dynamiques avec quantiles, intervalles égaux, seuils réglementaires
- Métadonnées complètes : source, producteur, dates, restrictions
- Performance 3 secondes max pour toute mise à jour

### Phase 3: Filtres et Comparaisons (2 semaines)

**Filtres Spatio-Temporels Avancés**
- Granularité temporelle au mois/année selon l'indicateur
- Emprise polygonale libre (pas seulement rectangulaire)
- Synchronisation complète carte/tableaux/exports
- Bouton réinitialisation global

**Comparaisons Inter-Territoires**
- 2 à 5 territoires simultanés de même niveau
- Mode side-by-side cartographique
- Tableau comparatif des indicateurs clés
- Export PDF automatique avec graphiques

### Phase 4: Tableaux de Bord et "Mon Territoire" (3-4 semaines)

**Indicateurs et Métriques**
- Dashboard complet : valeurs instantanées, tendances, seuils
- Badges qualité des données avec traçabilité
- Séries temporelles interactives avec accessibilité
- Génération < 3 secondes garantie

**Fiche "Mon Territoire"**
- Synthèse cartographique avec position hiérarchique
- Tuiles d'indicateurs avec cartes miniatures
- Points d'attention liés aux seuils d'alerte
- Suggestions de comparaison intelligentes

**Système d'Alertes**
- Seuils paramétrables par indicateur
- États visuels et historisation
- Notifications en temps réel
- Corrélation carte/tableau/alerte

### Phase 5: Annuaire et Métadonnées (2-3 semaines)

**Dictionnaire des Indicateurs**
- Fiches standardisées : définition, méthode, unités, bornes
- Traçabilité complète des sources et transformations
- Versioning des méthodes de calcul
- Liens directs depuis cartes/tableaux

**Monitoring des Flux**
- Tableau de bord opérationnel avec statuts en temps réel
- Journalisation détaillée : volume, durée, erreurs
- Mécanisme de relance automatique avec traçabilité
- Alertes < 1 minute pour les échecs

### Phase 6: Performance et Caching (2 semaines)

**Optimisations Backend**
- Tuiles pré-générées pour niveaux supérieurs
- Cache Redis pour requêtes fréquentes
- Clustering/heatmap pour couches denses
- Pagination optimisée avec index spatiaux

**Optimisations Frontend**
- Lazy loading des couches
- Debouncing des requêtes
- Web Workers pour calculs lourds
- Progressive loading avec states intermédiaires

### Phase 7: Accessibilité RGAA 4.1 (2-3 semaines)

**Conformité Complète**
- Audit RGAA 4.1 en Vérification d'Aptitude
- Navigation clavier complète pour cartes/graphiques
- Alternatives textuelles pour tous les éléments visuels
- Contrastes conformes et tailles adaptatives

**Composants Accessibles**
- Lecteurs d'écran compatibles
- Tableaux alternatifs pour graphiques/cartes
- Messages d'erreur compréhensibles
- Focus visible et ordre logique

### Phase 8: Exports et Liens Profonds (1-2 semaines)

**Exports Avancés**
- GeoJSON conforme avec métadonnées
- CSV structuré avec en-têtes explicites
- PDF cartographique haute qualité
- PNG avec légendes intégrées

**Partage et Permaliens**
- URL profonds avec état complet
- QR codes pour partage mobile
- Intégration sociale avec previews
- API REST pour intégrations tierces

### Phase 9: Parcours Démo et Formation (1 semaine)

**Scénarios Types**
- Parcours décideur : 5 minutes max fin-à-fin
- Parcours technicien : emprise → export → vérification
- Documentation interactive avec captures
- Guides contextuels intégrés

### Phase 10: Tests et Déploiement (2 semaines)

**Tests d'Intégration**
- Tests automatisés pour tous les parcours
- Tests de charge avec métriques 3 secondes
- Tests d'accessibilité automatiques
- Tests cross-browser complets

**Monitoring Production**
- Métriques temps réel : performance, erreurs, usage
- Alerting proactif sur seuils
- Logs centralisés avec corrélation
- Backup/Recovery automatisé

### Livrables Techniques
- Architecture Clean complète avec CQRS
- Documentation API OpenAPI/Swagger
- Guide d'administration système
- Manuel utilisateur avec captures
- Procédures de déploiement Docker/Kubernetes

### Métriques de Succès
- Performance : 100% des vues < 3 secondes
- Accessibilité : RGAA 4.1 conforme à 100%
- Fiabilité : 99.9% uptime, < 1% erreur rate
- Usage : parcours démo réussis sans assistance