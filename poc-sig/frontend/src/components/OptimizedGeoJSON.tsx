import React, { useMemo } from 'react';
import { GeoJSON, CircleMarker, useMap } from 'react-leaflet';
import MarkerClusterGroup from 'react-leaflet-markercluster';
import L from 'leaflet';
import 'react-leaflet-markercluster/dist/styles.min.css';

interface OptimizedGeoJSONProps {
  features: any[];
  onFeatureClick?: (feature: any) => void;
  selectedFeatures?: any[];
}

// Memoized GeoJSON component to prevent unnecessary re-renders
const OptimizedGeoJSON: React.FC<OptimizedGeoJSONProps> = React.memo(
  ({ features, onFeatureClick, selectedFeatures = [] }) => {
    const map = useMap();
    const zoom = map.getZoom();

    // Group features by geometry type for optimized rendering
    const { points, lines, polygons } = useMemo(() => {
      const grouped = {
        points: [] as any[],
        lines: [] as any[],
        polygons: [] as any[]
      };

      features.forEach(feature => {
        if (feature.geometry) {
          const geomType = feature.geometry.type;
          if (geomType === 'Point') {
            grouped.points.push(feature);
          } else if (geomType === 'LineString' || geomType === 'MultiLineString') {
            grouped.lines.push(feature);
          } else if (geomType === 'Polygon' || geomType === 'MultiPolygon') {
            grouped.polygons.push(feature);
          }
        }
      });

      return grouped;
    }, [features]);

    // Style function for features
    const getFeatureStyle = (feature: any) => {
      const isSelected = selectedFeatures.some(
        (f) => f.properties?.id === feature.properties?.id
      );

      const baseStyle = {
        color: feature.properties?.color || '#3388ff',
        weight: isSelected ? 3 : 2,
        opacity: isSelected ? 1 : 0.7,
        fillOpacity: isSelected ? 0.5 : 0.3,
      };

      if (feature.geometry?.type === 'LineString') {
        return {
          ...baseStyle,
          fillOpacity: 0,
        };
      }

      return baseStyle;
    };

    // Render points with clustering for better performance
    const renderPoints = useMemo(() => {
      if (points.length === 0) return null;

      // Use clustering only when there are many points
      if (points.length > 50 && zoom < 10) {
        return (
          <MarkerClusterGroup
            chunkedLoading
            maxClusterRadius={50}
            spiderfyOnMaxZoom
            showCoverageOnHover={false}
            iconCreateFunction={(cluster: any) => {
              const count = cluster.getChildCount();
              const size = count < 10 ? 'small' : count < 100 ? 'medium' : 'large';
              return L.divIcon({
                html: `<div><span>${count}</span></div>`,
                className: `marker-cluster marker-cluster-${size}`,
                iconSize: L.point(40, 40),
              });
            }}
          >
            {points.map((feature, index) => {
              const coords = feature.geometry.coordinates;
              return (
                <CircleMarker
                  key={`point-${index}`}
                  center={[coords[1], coords[0]]}
                  radius={5}
                  pathOptions={getFeatureStyle(feature)}
                  eventHandlers={{
                    click: () => onFeatureClick?.(feature),
                  }}
                />
              );
            })}
          </MarkerClusterGroup>
        );
      }

      // Render points without clustering when zoomed in
      return (
        <>
          {points.map((feature, index) => {
            const coords = feature.geometry.coordinates;
            return (
              <CircleMarker
                key={`point-${index}`}
                center={[coords[1], coords[0]]}
                radius={5}
                pathOptions={getFeatureStyle(feature)}
                eventHandlers={{
                  click: () => onFeatureClick?.(feature),
                }}
              />
            );
          })}
        </>
      );
    }, [points, zoom, selectedFeatures, onFeatureClick]);

    // Render lines and polygons (less performance intensive)
    const renderVectors = useMemo(() => {
      const vectors = [...lines, ...polygons];
      if (vectors.length === 0) return null;

      return (
        <>
          {vectors.map((feature, index) => (
            <GeoJSON
              key={`vector-${index}`}
              data={feature}
              style={getFeatureStyle(feature)}
              eventHandlers={{
                click: () => onFeatureClick?.(feature),
              }}
            />
          ))}
        </>
      );
    }, [lines, polygons, selectedFeatures, onFeatureClick]);

    return (
      <>
        {renderVectors}
        {renderPoints}
      </>
    );
  },
  // Custom comparison function to prevent unnecessary re-renders
  (prevProps, nextProps) => {
    return (
      prevProps.features.length === nextProps.features.length &&
      prevProps.selectedFeatures?.length === nextProps.selectedFeatures?.length
    );
  }
);

OptimizedGeoJSON.displayName = 'OptimizedGeoJSON';

export default OptimizedGeoJSON;