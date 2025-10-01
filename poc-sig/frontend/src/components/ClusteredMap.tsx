import React, { useMemo } from 'react';
import MarkerClusterGroup from 'react-leaflet-cluster';
import { Marker, Popup } from 'react-leaflet';
import L from 'leaflet';
import type { Feature } from '../types/api';

// Custom cluster icon
const createClusterCustomIcon = (cluster: any) => {
  const count = cluster.getChildCount();
  let size = 'small';
  let className = 'marker-cluster-small';

  if (count > 100) {
    size = 'large';
    className = 'marker-cluster-large';
  } else if (count > 50) {
    size = 'medium';
    className = 'marker-cluster-medium';
  }

  return L.divIcon({
    html: `<span>${count}</span>`,
    className: `marker-cluster ${className}`,
    iconSize: L.point(40, 40, true),
  });
};

interface ClusteredMapProps {
  features: Feature[];
  onFeatureClick?: (feature: Feature) => void;
}

export const ClusteredMap: React.FC<ClusteredMapProps> = ({
  features,
  onFeatureClick
}) => {
  // Convert features to markers only for Point geometries
  const markers = useMemo(() => {
    return features
      .filter(f => f.geometry?.type === 'Point')
      .map(feature => {
        const coords = feature.geometry.coordinates as [number, number];
        return {
          position: [coords[1], coords[0]] as [number, number],
          feature: feature,
        };
      });
  }, [features]);

  // Create custom icon for individual markers
  const createCustomIcon = (color: string = '#4169E1') => {
    return L.divIcon({
      html: `
        <div style="
          width: 12px;
          height: 12px;
          border-radius: 50%;
          background: ${color};
          border: 2px solid white;
          box-shadow: 0 2px 4px rgba(0,0,0,0.3);
        "></div>
      `,
      iconSize: [16, 16],
      iconAnchor: [8, 8],
      className: '',
    });
  };

  if (markers.length === 0) return null;

  return (
    <MarkerClusterGroup
      chunkedLoading
      maxClusterRadius={60}
      spiderfyOnMaxZoom={true}
      showCoverageOnHover={false}
      iconCreateFunction={createClusterCustomIcon}
      removeOutsideVisibleBounds={true}
      animate={true}
      animateAddingMarkers={false}
      disableClusteringAtZoom={15}
    >
      {markers.map((marker, index) => {
        const properties = marker.feature.properties || {};
        const color = properties.color || '#4169E1';

        return (
          <Marker
            key={marker.feature.id || index}
            position={marker.position}
            icon={createCustomIcon(color)}
            eventHandlers={{
              click: () => onFeatureClick?.(marker.feature),
            }}
          >
            <Popup>
              <div className="popup-content">
                <h4>{properties.name || 'Point'}</h4>
                {properties.type && <p><strong>Type:</strong> {properties.type}</p>}
                {properties.category && <p><strong>Catégorie:</strong> {properties.category}</p>}
                {properties.commune && <p><strong>Commune:</strong> {properties.commune}</p>}
                {properties.departement && <p><strong>Département:</strong> {properties.departement}</p>}
                {properties.code_bss && <p><strong>Code BSS:</strong> {properties.code_bss}</p>}
                {properties.profondeur && <p><strong>Profondeur:</strong> {properties.profondeur}m</p>}
                {properties.altitude_sol && <p><strong>Altitude:</strong> {properties.altitude_sol}m</p>}
              </div>
            </Popup>
          </Marker>
        );
      })}
    </MarkerClusterGroup>
  );
};