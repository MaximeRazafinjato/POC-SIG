import React, { useState, useEffect, useRef } from 'react';
import { MapContainer, TileLayer, GeoJSON, Rectangle, useMapEvents } from 'react-leaflet';
import { LatLngBounds, LatLng } from 'leaflet';
import 'leaflet/dist/leaflet.css';
import { layersApi, featuresApi, exportApi, downloadFile } from '../api/client';

interface Layer {
  id: number;
  name: string;
  srid: number;
  geometryType: string;
  createdUtc: string;
  updatedUtc: string;
  metadataJson: string;
}

interface Feature {
  id: number;
  layerId: number;
  propertiesJson: string;
  geometry: any;
  validFromUtc: string;
  validToUtc?: string;
}

interface FilterParams {
  bbox?: string;
  operation?: 'intersects' | 'within';
  bufferMeters?: number;
  validFrom?: string;
  validTo?: string;
  page?: number;
  pageSize?: number;
}

interface Stats {
  totalFeatures: number;
  filteredFeatures: number;
  executionTimeMs: number;
}

interface MapViewProps {}

export const MapView: React.FC<MapViewProps> = () => {
  const [layers, setLayers] = useState<Layer[]>([]);
  const [selectedLayer, setSelectedLayer] = useState<Layer | null>(null);
  const [features, setFeatures] = useState<Feature[]>([]);
  const [filters, setFilters] = useState<FilterParams>({
    operation: 'intersects',
    pageSize: 500,
  });
  const [stats, setStats] = useState<Stats | null>(null);
  const [bbox, setBbox] = useState<LatLngBounds | null>(null);
  const [loading, setLoading] = useState(false);
  const [drawing, setDrawing] = useState(false);

  useEffect(() => {
    loadLayers();
  }, []);

  useEffect(() => {
    if (selectedLayer) {
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
      }
    } catch (error) {
      console.error('Error loading layers:', error);
    }
  };

  const loadFeatures = async () => {
    if (!selectedLayer) return;

    setLoading(true);
    try {
      const response = await featuresApi.getFeatures(selectedLayer.id, filters);
      console.log('Features response:', response);
      // Le backend retourne features, pas items
      if (response.features) {
        setFeatures(response.features);
      } else if (response.items) {
        setFeatures(response.items);
      } else {
        setFeatures([]);
      }
    } catch (error) {
      console.error('Error loading features:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadStats = async () => {
    if (!selectedLayer) return;

    try {
      const statsData = await featuresApi.getStats(selectedLayer.id, filters);
      setStats(statsData);
    } catch (error) {
      console.error('Error loading stats:', error);
    }
  };

  const handleBboxChange = (bounds: LatLngBounds) => {
    setBbox(bounds);
    const bboxString = `${bounds.getWest()},${bounds.getSouth()},${bounds.getEast()},${bounds.getNorth()}`;
    setFilters(prev => ({ ...prev, bbox: bboxString }));
  };

  const clearBbox = () => {
    setBbox(null);
    setFilters(prev => ({ ...prev, bbox: undefined }));
  };

  const handleImport = async (fileName: string = 'sample.geojson') => {
    if (!selectedLayer) return;

    setLoading(true);
    try {
      await layersApi.importGeoJson(selectedLayer.id, fileName);
      await loadFeatures();
      await loadStats();
      alert(`Import de ${fileName} réussi !`);
    } catch (error) {
      console.error('Error importing GeoJSON:', error);
      alert('Erreur lors de l\'import');
    } finally {
      setLoading(false);
    }
  };

  const handleExportGeoJson = async () => {
    if (!selectedLayer) return;

    try {
      const response = await exportApi.exportGeoJson(selectedLayer.id, filters);
      downloadFile(response.blob, response.filename);
    } catch (error) {
      console.error('Error exporting GeoJSON:', error);
    }
  };

  const handleExportCsv = async () => {
    if (!selectedLayer) return;

    try {
      const response = await exportApi.exportCsv(selectedLayer.id, filters);
      downloadFile(response.blob, response.filename);
    } catch (error) {
      console.error('Error exporting CSV:', error);
    }
  };

  const BboxDrawer = () => {
    const map = useMapEvents({
      mousedown: (e) => {
        if (drawing) {
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
            map.off('mousemove', handleMouseMove);
            map.off('mouseup', handleMouseUp);
          };

          map.on('mousemove', handleMouseMove);
          map.on('mouseup', handleMouseUp);
        }
      },
    });

    return null;
  };

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('fr-FR');
  };

  const getFeatureColor = (feature: Feature): string => {
    const properties = JSON.parse(feature.propertiesJson || '{}');
    return properties.color || '#3388ff';
  };

  return (
    <div style={{ height: '100vh', display: 'flex' }}>
      {/* Sidebar */}
      <div style={{ width: '300px', padding: '20px', background: '#f5f5f5', overflow: 'auto' }}>
        <h2>POC SIG</h2>

        {/* Layer Selection */}
        <div style={{ marginBottom: '20px' }}>
          <label>Couche :</label>
          <select
            value={selectedLayer?.id || ''}
            onChange={(e) => {
              const layer = layers.find(l => l.id === parseInt(e.target.value));
              setSelectedLayer(layer || null);
            }}
            style={{ width: '100%', marginTop: '5px' }}
          >
            {layers.map(layer => (
              <option key={layer.id} value={layer.id}>{layer.name}</option>
            ))}
          </select>
        </div>

        {/* Filters */}
        <div style={{ marginBottom: '20px' }}>
          <h3>Filtres</h3>

          <div style={{ marginBottom: '10px' }}>
            <label>Opération spatiale :</label>
            <select
              value={filters.operation || 'intersects'}
              onChange={(e) => setFilters(prev => ({ ...prev, operation: e.target.value as 'intersects' | 'within' }))}
              style={{ width: '100%', marginTop: '5px' }}
            >
              <option value="intersects">Intersects</option>
              <option value="within">Within</option>
            </select>
          </div>

          <div style={{ marginBottom: '10px' }}>
            <label>Buffer (mètres) :</label>
            <input
              type="number"
              value={filters.bufferMeters || ''}
              onChange={(e) => setFilters(prev => ({ ...prev, bufferMeters: e.target.value ? parseInt(e.target.value) : undefined }))}
              style={{ width: '100%', marginTop: '5px' }}
            />
          </div>

          <div style={{ marginBottom: '10px' }}>
            <label>Date début :</label>
            <input
              type="date"
              value={filters.validFrom || ''}
              onChange={(e) => setFilters(prev => ({ ...prev, validFrom: e.target.value || undefined }))}
              style={{ width: '100%', marginTop: '5px' }}
            />
          </div>

          <div style={{ marginBottom: '10px' }}>
            <label>Date fin :</label>
            <input
              type="date"
              value={filters.validTo || ''}
              onChange={(e) => setFilters(prev => ({ ...prev, validTo: e.target.value || undefined }))}
              style={{ width: '100%', marginTop: '5px' }}
            />
          </div>
        </div>

        {/* Drawing Tools */}
        <div style={{ marginBottom: '20px' }}>
          <h3>Outils</h3>
          <button
            onClick={() => setDrawing(!drawing)}
            style={{
              width: '100%',
              marginBottom: '10px',
              backgroundColor: drawing ? '#ff6b6b' : '#4CAF50',
              color: 'white',
              border: 'none',
              padding: '10px',
              cursor: 'pointer'
            }}
          >
            {drawing ? 'Arrêter dessin' : 'Dessiner bbox'}
          </button>

          <button
            onClick={clearBbox}
            disabled={!bbox}
            style={{
              width: '100%',
              marginBottom: '10px',
              backgroundColor: bbox ? '#ff9800' : '#ccc',
              color: 'white',
              border: 'none',
              padding: '10px',
              cursor: bbox ? 'pointer' : 'not-allowed'
            }}
          >
            Effacer bbox
          </button>
        </div>

        {/* Actions */}
        <div style={{ marginBottom: '20px' }}>
          <h3>Actions</h3>
          <button
            onClick={() => handleImport('sample.geojson')}
            disabled={loading || !selectedLayer}
            style={{
              width: '100%',
              marginBottom: '10px',
              backgroundColor: '#2196F3',
              color: 'white',
              border: 'none',
              padding: '10px',
              cursor: loading ? 'not-allowed' : 'pointer'
            }}
          >
            Importer sample.geojson
          </button>

          <button
            onClick={() => handleImport('demo_complete.geojson')}
            disabled={loading || !selectedLayer}
            style={{
              width: '100%',
              marginBottom: '10px',
              backgroundColor: '#4CAF50',
              color: 'white',
              border: 'none',
              padding: '10px',
              cursor: loading ? 'not-allowed' : 'pointer'
            }}
          >
            Importer démo complète (30 features)
          </button>

          <button
            onClick={() => handleImport('monuments.geojson')}
            disabled={loading || !selectedLayer}
            style={{
              width: '100%',
              marginBottom: '10px',
              backgroundColor: '#FFD700',
              color: 'black',
              border: 'none',
              padding: '10px',
              cursor: loading ? 'not-allowed' : 'pointer'
            }}
          >
            Importer Monuments Paris (Tour Eiffel, etc.)
          </button>

          <button
            onClick={handleExportGeoJson}
            disabled={loading || !selectedLayer}
            style={{
              width: '100%',
              marginBottom: '10px',
              backgroundColor: '#9C27B0',
              color: 'white',
              border: 'none',
              padding: '10px',
              cursor: loading ? 'not-allowed' : 'pointer'
            }}
          >
            Exporter GeoJSON
          </button>

          <button
            onClick={handleExportCsv}
            disabled={loading || !selectedLayer}
            style={{
              width: '100%',
              marginBottom: '10px',
              backgroundColor: '#607D8B',
              color: 'white',
              border: 'none',
              padding: '10px',
              cursor: loading ? 'not-allowed' : 'pointer'
            }}
          >
            Exporter CSV
          </button>
        </div>

        {/* Stats */}
        {stats && (
          <div style={{ marginBottom: '20px' }}>
            <h3>Statistiques</h3>
            <p>Total : {stats.totalFeatures}</p>
            <p>Filtrées : {stats.filteredFeatures}</p>
            <p>Temps : {stats.executionTimeMs}ms</p>
          </div>
        )}

        {/* Current bbox */}
        {bbox && (
          <div style={{ marginBottom: '20px' }}>
            <h3>Bbox actuelle</h3>
            <p style={{ fontSize: '12px', wordBreak: 'break-all' }}>
              {bbox.getWest().toFixed(6)}, {bbox.getSouth().toFixed(6)}, {bbox.getEast().toFixed(6)}, {bbox.getNorth().toFixed(6)}
            </p>
          </div>
        )}

        {loading && (
          <div style={{ textAlign: 'center', color: '#666' }}>
            Chargement...
          </div>
        )}
      </div>

      {/* Map */}
      <div style={{ flex: 1 }}>
        <MapContainer
          center={[48.8566, 2.3522]}
          zoom={13}
          style={{ height: '100%', width: '100%' }}
        >
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />

          <BboxDrawer />

          {/* Bbox Rectangle */}
          {bbox && (
            <Rectangle
              bounds={bbox}
              pathOptions={{
                color: '#ff0000',
                weight: 2,
                fillOpacity: 0.1,
                dashArray: '10, 10'
              }}
            />
          )}

          {/* Features */}
          {features && features.map((feature, index) => {
            try {
              // Le backend renvoie déjà du GeoJSON valide
              // Les propriétés sont dans feature.properties
              const properties = feature.properties || {};

              return (
                <GeoJSON
                  key={`${feature.id}-${index}`}
                  data={feature} // Utiliser directement la feature GeoJSON
                  style={{
                    color: properties.color || '#3388ff',
                    weight: 2,
                    fillOpacity: 0.5,
                  }}
                  onEachFeature={(geoJsonFeature, layer) => {
                    const props = geoJsonFeature.properties || {};
                    layer.bindPopup(`
                      <div>
                        <h4>${props.name || 'Feature ' + props.id}</h4>
                        <p><strong>Couche:</strong> ${selectedLayer?.name || 'Paris Landmarks'}</p>
                        <p><strong>Type:</strong> ${props.type || 'N/A'}</p>
                        ${props.surface ? `<p><strong>Surface:</strong> ${props.surface} ha</p>` : ''}
                        ${props.validFromUtc ? `<p><strong>Valide du:</strong> ${formatDate(props.validFromUtc)}</p>` : ''}
                        ${props.validToUtc ? `<p><strong>Valide jusqu'à:</strong> ${formatDate(props.validToUtc)}</p>` : ''}
                      </div>
                    `);
                  }}
                />
              );
            } catch (error) {
              console.error('Error rendering feature:', feature, error);
              return null;
            }
          })}
        </MapContainer>
      </div>
    </div>
  );
};