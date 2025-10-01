import React, { useState, useEffect, useRef, useMemo } from "react";
import {
  MapContainer,
  TileLayer,
  GeoJSON,
  Rectangle,
  useMapEvents,
} from "react-leaflet";
import { LatLngBounds, LatLng } from "leaflet";
import "leaflet/dist/leaflet.css";
import "../styles/modern.css";
import "../styles/clusters.css";
import { ClusteredMap } from "../components/ClusteredMap";
import {
  Map,
  Layers,
  Download,
  Upload,
  Filter,
  Square,
  Trash2,
  BarChart3,
  Calendar,
  Moon,
  Sun,
  Play,
  Pause,
  RotateCw,
  Plus,
  Settings,
  Database,
  MapPin,
  Globe,
  MousePointer,
} from "lucide-react";
import {
  layersApi,
  featuresApi,
  exportApi,
  downloadFile,
  api,
  clusterApi,
} from "../api/client";
import { SelectionPanel } from "../components/SelectionPanel";
import type { Layer, Feature, FilterParams, Stats } from "../types/api";

export const ModernMapView: React.FC = () => {
  const [layers, setLayers] = useState<Layer[]>([]);
  const [selectedLayer, setSelectedLayer] = useState<Layer | null>(null);
  const [features, setFeatures] = useState<Feature[]>([]);
  const [filters, setFilters] = useState<FilterParams>({
    operation: "intersects",
    pageSize: 2000,  // Load all data, optimize rendering instead
  });
  const [stats, setStats] = useState<Stats | null>(null);
  const [bbox, setBbox] = useState<LatLngBounds | null>(null);
  const [loading, setLoading] = useState(false);
  const [drawing, setDrawing] = useState(false);
  const [darkMode, setDarkMode] = useState(false);
  const [showFilters, setShowFilters] = useState(true);
  const [selectedFeatures, setSelectedFeatures] = useState<Feature[]>([]);
  const [isSelectionMode, setIsSelectionMode] = useState(false);

  useEffect(() => {
    loadLayers();
    // Apply dark mode to body
    if (darkMode) {
      document.body.classList.add("dark-mode");
    } else {
      document.body.classList.remove("dark-mode");
    }
  }, [darkMode]);

  useEffect(() => {
    if (selectedLayer && selectedLayer.id !== 1) {
      // Only load specific layer features if it's not the Default layer
      // For Default layer, we keep all features displayed
      loadFeatures();
      loadStats();
    }
  }, [selectedLayer, filters]);

  const loadLayers = async () => {
    try {
      const layersData = await layersApi.getAll();
      setLayers(layersData);
      if (layersData.length > 0 && !selectedLayer) {
        setSelectedLayer(layersData[0]);
        // Charger toutes les features de toutes les couches au démarrage
        await loadAllFeatures(layersData);
      }
    } catch (error) {
      console.error("Error loading layers:", error);
    }
  };

  const loadAllFeatures = async (layersData: Layer[]) => {
    try {
      setLoading(true);
      console.log("Loading all features from all layers...");
      const allFeatures: Feature[] = [];

      for (const layer of layersData) {
        try {
          const response = await featuresApi.getFeatures(layer.id, {
            pageSize: 2000,  // Load all features
          });
          const layerFeatures = response.features || response.items || [];
          console.log(
            `Loaded ${layerFeatures.length} features from layer "${layer.name}"`
          );
          allFeatures.push(...layerFeatures);
        } catch (error) {
          console.error(
            `Error loading features for layer ${layer.name}:`,
            error
          );
        }
      }

      // Parse properties for GeoJSON format
      const parsedAllFeatures = allFeatures.map((feature: any) => ({
        ...feature,
        properties:
          feature.properties ||
          (feature.propertiesJson ? JSON.parse(feature.propertiesJson) : {}),
      }));

      console.log(`Total features loaded: ${parsedAllFeatures.length}`);
      setFeatures(parsedAllFeatures);
    } catch (error) {
      console.error("Error loading all features:", error);
    } finally {
      setLoading(false);
    }
  };

  const loadFeatures = async () => {
    if (!selectedLayer) return;

    console.log("Loading features with filters:", filters);
    setLoading(true);
    try {
      const response = await featuresApi.getFeatures(selectedLayer.id, filters);
      console.log("API response:", response);

      if (response.features) {
        console.log(
          `Loaded ${response.features.length} features (from response.features)`
        );
        setFeatures(response.features);
      } else if (response.items) {
        console.log(
          `Loaded ${response.items.length} features (from response.items)`
        );
        setFeatures(response.items);
      } else {
        console.log("No features found in response");
        setFeatures([]);
      }
    } catch (error) {
      console.error("Error loading features:", error);
    } finally {
      setLoading(false);
    }
  };

  const loadStats = async () => {
    if (!selectedLayer) return;

    try {
      const statsData = await featuresApi.getStats(selectedLayer.id, filters);

      // Map backend stats to frontend format
      const mappedStats = {
        totalFeatures: features.length,
        filteredFeatures: statsData.count || features.length,
        executionTimeMs: statsData.queryTimeMs || 0,
      };

      setStats(mappedStats);
    } catch (error) {
      console.error("Error loading stats:", error);
    }
  };

  const handleBboxChange = (bounds: LatLngBounds) => {
    setBbox(bounds);
    const bboxString = `${bounds.getWest()},${bounds.getSouth()},${bounds.getEast()},${bounds.getNorth()}`;
    console.log(
      "Setting bbox filter:",
      bboxString,
      "Selection mode:",
      isSelectionMode
    );
    setFilters((prev) => ({ ...prev, bbox: bboxString }));

    // If in selection mode, track selected features
    if (isSelectionMode) {
      console.log("Triggering feature selection for bbox:", bboxString);
      loadSelectedFeatures(bounds);
    }
  };

  const clearBbox = () => {
    setBbox(null);
    setFilters((prev) => ({ ...prev, bbox: undefined }));
    if (isSelectionMode) {
      setSelectedFeatures([]);
    }
  };

  const loadSelectedFeatures = async (bounds: LatLngBounds) => {
    if (!selectedLayer) {
      console.log("No layer selected for feature selection");
      return;
    }

    try {
      const bboxString = `${bounds.getWest()},${bounds.getSouth()},${bounds.getEast()},${bounds.getNorth()}`;

      // Si on est sur la couche Default (ID: 1), chercher dans toutes les couches
      if (selectedLayer.id === 1) {
        console.log(
          `Loading selected features with bbox: ${bboxString} from all layers (Default mode)`
        );

        const allSelectedFeatures: Feature[] = [];

        // Chercher dans toutes les couches sauf Default
        const layersToSearch = layers.filter((layer) => layer.id !== 1);
        for (const layer of layersToSearch) {
          try {
            console.log(`Searching in layer "${layer.name}" (ID: ${layer.id})`);
            const response = await featuresApi.getFeatures(layer.id, {
              bbox: bboxString,
              operation: filters.operation,
              bufferMeters: filters.bufferMeters,
              pageSize: 1000,
            });

            const featuresData = response.features || response.items || [];
            console.log(
              `Found ${featuresData.length} features in layer "${layer.name}"`
            );
            allSelectedFeatures.push(...featuresData);
          } catch (error) {
            console.error(
              `Error loading features from layer ${layer.name}:`,
              error
            );
          }
        }

        // Parse properties for better display (handle both GeoJSON and DB format)
        const parsedFeatures = allSelectedFeatures.map((feature: any) => ({
          ...feature,
          properties:
            feature.properties ||
            (feature.propertiesJson ? JSON.parse(feature.propertiesJson) : {}),
        }));

        console.log(
          `Total selected features from all layers: ${parsedFeatures.length}`
        );
        setSelectedFeatures(parsedFeatures);
      } else {
        // Mode couche spécifique (comportement original)
        console.log(
          `Loading selected features with bbox: ${bboxString} from selected layer "${selectedLayer.name}" (ID: ${selectedLayer.id})`
        );

        const response = await featuresApi.getFeatures(selectedLayer.id, {
          bbox: bboxString,
          operation: filters.operation,
          bufferMeters: filters.bufferMeters,
          pageSize: 1000,
        });

        const featuresData = response.features || response.items || [];
        console.log(
          `Found ${featuresData.length} features in selected layer "${selectedLayer.name}"`
        );

        // Parse properties for better display (handle both GeoJSON and DB format)
        const parsedFeatures = featuresData.map((feature: any) => ({
          ...feature,
          properties:
            feature.properties ||
            (feature.propertiesJson ? JSON.parse(feature.propertiesJson) : {}),
        }));

        console.log(
          `Total selected features from layer "${selectedLayer.name}": ${parsedFeatures.length}`
        );
        setSelectedFeatures(parsedFeatures);
      }
    } catch (error) {
      console.error("Error loading selected features:", error);
    }
  };

  const handleToggleSelectionMode = () => {
    const newMode = !isSelectionMode;
    console.log("Toggling selection mode:", isSelectionMode, "->", newMode);
    setIsSelectionMode(newMode);
    setDrawing(newMode); // Enable drawing when selection mode is on

    if (!newMode) {
      // Clear selection when turning off selection mode
      console.log("Clearing selection data");
      setSelectedFeatures([]);
      setBbox(null);
      setFilters((prev) => ({ ...prev, bbox: undefined }));
    }
  };

  const handleClearSelection = () => {
    setSelectedFeatures([]);
    setBbox(null);
    setFilters((prev) => ({ ...prev, bbox: undefined }));
  };

  const handleImport = async (fileName: string, layerName?: string) => {
    setLoading(true);
    try {
      let targetLayerId = selectedLayer?.id;

      // Si pas de couche, on la crée ou on utilise l'import avec layerName
      if (!targetLayerId) {
        // Import avec création automatique de la couche
        const response = await api.post(
          `/layers/import?layerName=${encodeURIComponent(
            layerName || fileName.replace(".geojson", "")
          )}&fileName=${encodeURIComponent(fileName)}`
        );

        // Recharger les couches après import
        await loadLayers();
        alert(`Import de ${fileName} réussi avec création de la couche !`);
      } else {
        // Import dans la couche existante
        await layersApi.importGeoJson(targetLayerId, fileName);
        await loadFeatures();
        await loadStats();
        alert(`Import de ${fileName} réussi !`);
      }
    } catch (error) {
      console.error("Error importing GeoJSON:", error);
      alert("Erreur lors de l'import");
    } finally {
      setLoading(false);
    }
  };

  const handleLoadDemoData = async () => {
    setLoading(true);
    try {
      const response = await api.post("/admin/load-demo-data");
      const data = response.data;

      if (data.success) {
        // Recharger les couches après import
        await loadLayers();

        // Afficher le résumé
        const successCount = data.results.filter((r: any) => r.success).length;

        // Extraire le nombre de features du message
        const totalFeatures = data.results.reduce((sum: number, r: any) => {
          if (r.success && r.message) {
            const match = r.message.match(/imported (\d+) features/);
            return sum + (match ? parseInt(match[1]) : 0);
          }
          return sum;
        }, 0);

        alert(
          `Données de démonstration chargées avec succès !\n${successCount} couches créées\n${totalFeatures} éléments importés au total`
        );
      } else {
        alert("Erreur lors du chargement des données de démonstration");
      }
    } catch (error) {
      console.error("Error loading demo data:", error);
      alert("Erreur lors du chargement des données de démonstration");
    } finally {
      setLoading(false);
    }
  };

  const handleExportGeoJson = async () => {
    if (!selectedLayer) return;

    try {
      if (selectedLayer.id === 1) {
        // Pour la couche "Default", utiliser l'API features pour récupérer toutes les données
        const allFeatures: any[] = [];
        const layersToExport = layers.filter((layer) => layer.id !== 1);

        for (const layer of layersToExport) {
          try {
            console.log(
              `Exporting features from layer "${layer.name}" (ID: ${layer.id})`
            );
            const response = await featuresApi.getFeatures(layer.id, {
              ...filters,
              pageSize: 1000,
            });

            const layerFeatures = response.features || response.items || [];
            console.log(
              `Found ${layerFeatures.length} features in layer "${layer.name}"`
            );

            // Convertir en format GeoJSON
            layerFeatures.forEach((feature: any) => {
              const geoJsonFeature = {
                type: "Feature",
                id: feature.id,
                geometry: feature.geometry,
                properties: {
                  ...feature.properties,
                  layerId: feature.layerId,
                  layerName: layer.name,
                  validFromUtc: feature.validFromUtc,
                  validToUtc: feature.validToUtc,
                },
              };
              allFeatures.push(geoJsonFeature);
            });
          } catch (error) {
            console.error(`Error exporting layer ${layer.name}:`, error);
          }
        }

        // Créer un GeoJSON combiné
        const combinedGeoJson = {
          type: "FeatureCollection",
          features: allFeatures,
        };

        console.log(`Exporting ${allFeatures.length} total features`);
        const blob = new Blob([JSON.stringify(combinedGeoJson, null, 2)], {
          type: "application/json",
        });
        downloadFile(blob, "toutes-les-couches.geojson");
      } else {
        // Export normal pour une couche spécifique
        const response = await exportApi.exportGeoJson(
          selectedLayer.id,
          filters
        );
        downloadFile(response.blob, response.filename);
      }
    } catch (error) {
      console.error("Error exporting GeoJSON:", error);
    }
  };

  const handleExportCsv = async () => {
    if (!selectedLayer) return;

    try {
      if (selectedLayer.id === 1) {
        // Pour la couche "Default", utiliser l'API features pour récupérer toutes les données
        const allFeatures: any[] = [];
        const layersToExport = layers.filter((layer) => layer.id !== 1);

        for (const layer of layersToExport) {
          try {
            console.log(
              `Exporting CSV from layer "${layer.name}" (ID: ${layer.id})`
            );
            const response = await featuresApi.getFeatures(layer.id, {
              ...filters,
              pageSize: 1000,
            });

            const layerFeatures = response.features || response.items || [];
            console.log(
              `Found ${layerFeatures.length} features for CSV in layer "${layer.name}"`
            );

            // Ajouter les features avec le nom de la couche
            layerFeatures.forEach((feature: any) => {
              allFeatures.push({
                id: feature.id,
                layerName: layer.name,
                layerId: feature.layerId,
                validFromUtc: feature.validFromUtc,
                validToUtc: feature.validToUtc,
                ...feature.properties,
              });
            });
          } catch (error) {
            console.error(
              `Error exporting CSV from layer ${layer.name}:`,
              error
            );
          }
        }

        if (allFeatures.length > 0) {
          // Créer les en-têtes CSV
          const allKeys = new Set<string>();
          allFeatures.forEach((feature) => {
            Object.keys(feature).forEach((key) => allKeys.add(key));
          });
          const headers = Array.from(allKeys);

          // Créer les lignes CSV
          const csvLines = [headers.join(",")];
          allFeatures.forEach((feature) => {
            const row = headers.map((header) => {
              const value = feature[header];
              return value !== null && value !== undefined
                ? `"${String(value).replace(/"/g, '""')}"`
                : "";
            });
            csvLines.push(row.join(","));
          });

          const combinedCsv = csvLines.join("\n");
          console.log(`Exporting ${allFeatures.length} total features to CSV`);
          const blob = new Blob([combinedCsv], { type: "text/csv" });
          downloadFile(blob, "toutes-les-couches.csv");
        } else {
          console.log("No features to export to CSV");
        }
      } else {
        // Export normal pour une couche spécifique
        const response = await exportApi.exportCsv(selectedLayer.id, filters);
        downloadFile(response.blob, response.filename);
      }
    } catch (error) {
      console.error("Error exporting CSV:", error);
    }
  };

  const handleExportSelectedGeoJson = () => {
    if (selectedFeatures.length === 0) {
      alert("Aucun élément sélectionné à exporter");
      return;
    }

    try {
      // Convertir les features sélectionnées en GeoJSON
      const geoJsonFeatures = selectedFeatures.map((feature: any) => ({
        type: "Feature",
        id: feature.id,
        geometry: feature.geometry,
        properties: {
          ...feature.properties,
          layerId: feature.layerId || feature.properties?.layerId,
          validFromUtc: feature.validFromUtc,
          validToUtc: feature.validToUtc,
        },
      }));

      const geoJson = {
        type: "FeatureCollection",
        features: geoJsonFeatures,
      };

      console.log(
        `Exporting ${selectedFeatures.length} selected features to GeoJSON`
      );
      const blob = new Blob([JSON.stringify(geoJson, null, 2)], {
        type: "application/json",
      });
      downloadFile(
        blob,
        `elements-selectionnes-${
          new Date().toISOString().split("T")[0]
        }.geojson`
      );
    } catch (error) {
      console.error("Error exporting selected features to GeoJSON:", error);
      alert("Erreur lors de l'export GeoJSON");
    }
  };

  const handleExportSelectedCsv = () => {
    if (selectedFeatures.length === 0) {
      alert("Aucun élément sélectionné à exporter");
      return;
    }

    try {
      // Préparer les données pour CSV
      const csvData = selectedFeatures.map((feature: any) => ({
        id: feature.id,
        layerId: feature.layerId || feature.properties?.layerId,
        validFromUtc: feature.validFromUtc,
        validToUtc: feature.validToUtc,
        ...feature.properties,
      }));

      // Créer les en-têtes CSV
      const allKeys = new Set<string>();
      csvData.forEach((item) => {
        Object.keys(item).forEach((key) => allKeys.add(key));
      });
      const headers = Array.from(allKeys);

      // Créer les lignes CSV
      const csvLines = [headers.join(",")];
      csvData.forEach((item) => {
        const row = headers.map((header) => {
          const value = item[header];
          return value !== null && value !== undefined
            ? `"${String(value).replace(/"/g, '""')}"`
            : "";
        });
        csvLines.push(row.join(","));
      });

      const csvContent = csvLines.join("\n");
      console.log(
        `Exporting ${selectedFeatures.length} selected features to CSV`
      );
      const blob = new Blob([csvContent], { type: "text/csv" });
      downloadFile(
        blob,
        `elements-selectionnes-${new Date().toISOString().split("T")[0]}.csv`
      );
    } catch (error) {
      console.error("Error exporting selected features to CSV:", error);
      alert("Erreur lors de l'export CSV");
    }
  };

  const BboxDrawer = () => {
    const map = useMapEvents({
      mousedown: (e) => {
        if (drawing && isSelectionMode) {
          // Désactiver le drag de la carte pendant le dessin
          map.dragging.disable();

          const startLatLng = e.latlng;

          const handleMouseMove = (moveEvent: any) => {
            const endLatLng = moveEvent.latlng;
            const bounds = new LatLngBounds(startLatLng, endLatLng);
            setBbox(bounds);
          };

          const handleMouseUp = (upEvent: any) => {
            const endLatLng = upEvent.latlng;
            const bounds = new LatLngBounds(startLatLng, endLatLng);
            handleBboxChange(bounds);
            setDrawing(false);

            // Réactiver le drag de la carte
            map.dragging.enable();

            map.off("mousemove", handleMouseMove);
            map.off("mouseup", handleMouseUp);
          };

          map.on("mousemove", handleMouseMove);
          map.on("mouseup", handleMouseUp);
        }
      },
    });

    // Réactiver le drag si on désactive le mode dessin
    React.useEffect(() => {
      if (!drawing && map) {
        map.dragging.enable();
      }
    }, [drawing, map]);

    return null;
  };

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString("fr-FR");
  };

  const getFeatureColor = (feature: Feature): string => {
    const properties = JSON.parse(feature.propertiesJson || "{}");
    return properties.color || "#667eea";
  };

  return (
    <div style={{ height: "100vh", display: "flex", position: "relative" }}>
      {/* Theme Toggle */}
      <div className="theme-toggle">
        <div
          className={`toggle-switch ${darkMode ? "active" : ""}`}
          onClick={() => setDarkMode(!darkMode)}
          title={darkMode ? "Mode clair" : "Mode sombre"}
        >
          {darkMode ? <Moon size={16} /> : <Sun size={16} />}
        </div>
      </div>

      {/* Sidebar */}
      <div className="sidebar" style={{ width: "380px", padding: "1.5rem" }}>
        {/* Header */}
        <div className="header">
          <div style={{ display: "flex", alignItems: "center", gap: "0.5rem" }}>
            <Globe size={32} />
            <h1>POC SIG</h1>
          </div>
          <div className="subtitle">Système d'Information Géographique</div>
        </div>

        {/* Stats Grid */}
        {stats && (
          <div className="stats-grid fade-in">
            <div className="stat-card">
              <div className="stat-value">{stats.totalFeatures}</div>
              <div className="stat-label">Total</div>
            </div>
            <div className="stat-card">
              <div className="stat-value">{stats.filteredFeatures}</div>
              <div className="stat-label">Filtrées</div>
            </div>
          </div>
        )}

        {/* Layer Selection */}
        <div className="card fade-in">
          <div className="card-title">
            <Layers size={18} />
            Gestion des couches
          </div>
          <div className="form-group">
            <select
              className="form-control"
              value={selectedLayer?.id || ""}
              onChange={async (e) => {
                const layer = layers.find(
                  (l) => l.id === parseInt(e.target.value)
                );
                setSelectedLayer(layer || null);

                // Si on sélectionne la couche Default (ID: 1), recharger toutes les features
                if (layer && layer.id === 1) {
                  console.log(
                    "Switching to Default layer, loading all features"
                  );
                  await loadAllFeatures(layers);
                }
              }}
            >
              {layers.length === 0 && <option>Aucune couche disponible</option>}
              {layers.map((layer) => (
                <option key={layer.id} value={layer.id}>
                  {layer.name}
                </option>
              ))}
            </select>
          </div>
          <button
            className="btn btn-primary"
            onClick={async () => {
              const name = prompt("Nom de la nouvelle couche :");
              if (name) {
                try {
                  const response = await api.post("/layers", {
                    name,
                    srid: 4326,
                    geometryType: "Geometry",
                    metadataJson: "{}",
                  });
                  await loadLayers();
                  alert(`Couche "${name}" créée avec succès !`);
                } catch (error) {
                  alert("Erreur lors de la création de la couche");
                }
              }
            }}
            style={{ width: "100%", marginTop: "0.75rem" }}
          >
            <Plus size={16} />
            Créer une nouvelle couche
          </button>

          <button
            className="btn btn-success"
            onClick={handleLoadDemoData}
            disabled={loading}
            style={{ width: "100%", marginTop: "0.5rem" }}
          >
            <Database size={16} />
            {loading ? "Chargement..." : "Charger les données de démo"}
          </button>
        </div>

        {/* Filters */}
        <div className="card fade-in">
          <div
            className="card-title"
            style={{ cursor: "pointer" }}
            onClick={() => setShowFilters(!showFilters)}
          >
            <Filter size={18} />
            Filtres spatiaux et temporels
            <span style={{ marginLeft: "auto" }}>
              {showFilters ? "−" : "+"}
            </span>
          </div>

          {showFilters && (
            <>
              <div className="form-group">
                <label className="form-label">Opération spatiale</label>
                <select
                  className="form-control"
                  value={filters.operation || "intersects"}
                  onChange={(e) =>
                    setFilters((prev) => ({
                      ...prev,
                      operation: e.target.value as "intersects" | "within",
                    }))
                  }
                >
                  <option value="intersects">Croise</option>
                  <option value="within">Contenu dans</option>
                </select>
              </div>

              <div className="form-group">
                <label className="form-label">Buffer (mètres)</label>
                <input
                  type="number"
                  className="form-control"
                  value={filters.bufferMeters || ""}
                  onChange={(e) =>
                    setFilters((prev) => ({
                      ...prev,
                      bufferMeters: e.target.value
                        ? parseInt(e.target.value)
                        : undefined,
                    }))
                  }
                  placeholder="Ex: 100"
                />
              </div>
            </>
          )}
        </div>

        {/* Actions */}
        <div className="card fade-in">
          <div className="card-title">
            <Settings size={18} />
            Export des données
          </div>
          <div
            style={{
              display: "grid",
              gridTemplateColumns: "1fr 1fr",
              gap: "0.75rem",
            }}
          >
            <button
              className="btn btn-dark"
              onClick={handleExportGeoJson}
              disabled={loading || !selectedLayer}
              title="Exporter la couche sélectionnée en GeoJSON"
            >
              <Download size={16} />
              GeoJSON
            </button>

            <button
              className="btn btn-dark"
              onClick={handleExportCsv}
              disabled={loading || !selectedLayer}
              title="Exporter la couche sélectionnée en CSV"
            >
              <Download size={16} />
              CSV
            </button>
          </div>
        </div>

        {/* Export des éléments sélectionnés */}
        <div className="card fade-in">
          <div className="card-title">
            <MousePointer size={18} />
            Export sélection ({selectedFeatures.length})
          </div>
          <div
            style={{
              display: "grid",
              gridTemplateColumns: "1fr 1fr",
              gap: "0.75rem",
            }}
          >
            <button
              className="btn btn-success"
              onClick={handleExportSelectedGeoJson}
              disabled={selectedFeatures.length === 0}
              title={
                selectedFeatures.length === 0
                  ? "Aucun élément sélectionné"
                  : `Exporter ${selectedFeatures.length} éléments sélectionnés en GeoJSON`
              }
            >
              <Download size={16} />
              GeoJSON
            </button>

            <button
              className="btn btn-success"
              onClick={handleExportSelectedCsv}
              disabled={selectedFeatures.length === 0}
              title={
                selectedFeatures.length === 0
                  ? "Aucun élément sélectionné"
                  : `Exporter ${selectedFeatures.length} éléments sélectionnés en CSV`
              }
            >
              <Download size={16} />
              CSV
            </button>
          </div>
        </div>

        {/* Loading Indicator */}
        {loading && (
          <div className="card fade-in" style={{ textAlign: "center" }}>
            <div className="spinner"></div>
            <div className="stat-label">Chargement...</div>
          </div>
        )}
      </div>

      {/* Map */}
      <div style={{ flex: 1, position: "relative" }}>
        <MapContainer
          center={[48.6, 6.2]}
          zoom={7}
          style={{ height: "100%", width: "100%" }}
        >
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
            url={
              darkMode
                ? "https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png"
                : "https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png"
            }
          />

          <BboxDrawer />

          {/* Bbox Rectangle */}
          {bbox && (
            <Rectangle
              bounds={bbox}
              pathOptions={{
                color: "#667eea",
                weight: 3,
                fillOpacity: 0.2,
                dashArray: "10, 5",
              }}
            />
          )}

          {/* Optimized Feature Rendering */}
          {/* Render clustered points for performance */}
          {features && features.length > 0 && (
            <ClusteredMap
              features={features}
              onFeatureClick={(feature) => {
                console.log("Feature clicked:", feature);
              }}
            />
          )}

          {/* Render non-point geometries separately */}
          {features &&
            features.filter(f => f.geometry?.type !== 'Point').map((feature, index) => {
              try {
                const properties = feature.properties || {};

                return (
                  <GeoJSON
                    key={`${feature.id}-${index}`}
                    data={feature}
                    style={{
                      color: properties.color || "#667eea",
                      weight: 3,
                      fillOpacity: 0.6,
                    }}
                    onEachFeature={(geoJsonFeature, layer) => {
                      const props = geoJsonFeature.properties || {};
                      layer.bindPopup(`
                      <div style="
                        padding: 10px;
                        font-family: 'Inter', sans-serif;
                        min-width: 200px;
                      ">
                        <h4 style="
                          margin: 0 0 10px 0;
                          color: #667eea;
                          font-size: 16px;
                          font-weight: 600;
                        ">
                          ${props.name || "Feature " + props.id}
                        </h4>
                        <div style="
                          display: flex;
                          flex-direction: column;
                          gap: 5px;
                          font-size: 14px;
                          color: #636e72;
                        ">
                          ${
                            props.layer
                              ? `<div><strong>Catégorie:</strong> ${props.layer}</div>`
                              : selectedLayer?.name
                              ? `<div><strong>Couche:</strong> ${selectedLayer.name}</div>`
                              : ""
                          }
                          ${
                            props.type
                              ? `<div><strong>Type:</strong> ${props.type.replace(/_/g, ' ')}</div>`
                              : ""
                          }
                          ${
                            props.commune
                              ? `<div><strong>Commune:</strong> ${props.commune}</div>`
                              : ""
                          }
                          ${
                            props.departement
                              ? `<div><strong>Département:</strong> ${props.departement}</div>`
                              : ""
                          }
                          ${
                            props.surface || props.surface_ha
                              ? `<div><strong>Surface:</strong> ${props.surface || props.surface_ha} ha</div>`
                              : ""
                          }
                          ${
                            props.debit_moyen_m3s
                              ? `<div><strong>Débit moyen:</strong> ${props.debit_moyen_m3s} m³/s</div>`
                              : ""
                          }
                          ${
                            props.qualite_eau
                              ? `<div><strong>Qualité:</strong> ${props.qualite_eau}</div>`
                              : ""
                          }
                          ${
                            props.profondeur
                              ? `<div><strong>Profondeur:</strong> ${props.profondeur} m</div>`
                              : ""
                          }
                          ${
                            props.date_debut
                              ? `<div><strong>Depuis le:</strong> ${formatDate(props.date_debut)}</div>`
                              : props.validFromUtc
                              ? `<div><strong>Depuis le:</strong> ${formatDate(props.validFromUtc)}</div>`
                              : ""
                          }
                        </div>
                        <div style="
                          margin-top: 10px;
                          width: 100%;
                          height: 4px;
                          background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                          border-radius: 2px;
                        "></div>
                      </div>
                    `);
                    }}
                  />
                );
              } catch (error) {
                console.error("Error rendering feature:", feature, error);
                return null;
              }
            })}
        </MapContainer>
      </div>

      {/* Selection Panel */}
      <SelectionPanel
        isSelectionMode={isSelectionMode}
        onToggleSelectionMode={handleToggleSelectionMode}
        selectedFeatures={selectedFeatures}
        onClearSelection={handleClearSelection}
        layers={layers}
      />
    </div>
  );
};
