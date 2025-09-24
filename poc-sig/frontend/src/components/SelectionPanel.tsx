import React, { useState } from 'react';
import { MousePointer, Layers3, ChevronDown, ChevronUp, Target, Sparkles, X } from 'lucide-react';
import type { Feature, Layer } from '../types/api';

interface SelectionPanelProps {
  isSelectionMode: boolean;
  onToggleSelectionMode: () => void;
  selectedFeatures: Feature[];
  onClearSelection: () => void;
  layers: Layer[];
}

export const SelectionPanel: React.FC<SelectionPanelProps> = ({
  isSelectionMode,
  onToggleSelectionMode,
  selectedFeatures,
  onClearSelection,
  layers
}) => {
  const [isExpanded, setIsExpanded] = useState(true);

  return (
    <div className="selection-panel">
      <style jsx>{`
        .selection-panel {
          position: fixed;
          top: 24px;
          right: 24px;
          z-index: 1000;
          width: 420px;
          max-height: calc(100vh - 48px);
          background: linear-gradient(135deg,
            rgba(255, 255, 255, 0.95) 0%,
            rgba(248, 250, 252, 0.95) 100%);
          backdrop-filter: blur(20px);
          border-radius: 24px;
          border: 1px solid rgba(255, 255, 255, 0.3);
          box-shadow:
            0 32px 64px rgba(0, 0, 0, 0.12),
            0 0 0 1px rgba(255, 255, 255, 0.05),
            inset 0 1px 0 rgba(255, 255, 255, 0.3);
          overflow: hidden;
          transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
          transform: translateZ(0);
        }

        .selection-panel:hover {
          transform: translateY(-2px) scale(1.01);
          box-shadow:
            0 40px 80px rgba(0, 0, 0, 0.16),
            0 0 0 1px rgba(255, 255, 255, 0.1),
            inset 0 1px 0 rgba(255, 255, 255, 0.4);
        }

        .panel-header {
          padding: 24px;
          background: linear-gradient(135deg,
            rgba(99, 102, 241, 0.05) 0%,
            rgba(168, 85, 247, 0.05) 100%);
          border-bottom: 1px solid rgba(255, 255, 255, 0.1);
          position: relative;
          overflow: hidden;
        }

        .panel-header::before {
          content: '';
          position: absolute;
          top: 0;
          left: 0;
          right: 0;
          height: 1px;
          background: linear-gradient(90deg,
            transparent 0%,
            rgba(99, 102, 241, 0.3) 50%,
            transparent 100%);
        }

        .header-content {
          display: flex;
          items: center;
          justify-content: space-between;
        }

        .header-title {
          display: flex;
          align-items: center;
          gap: 12px;
        }

        .header-icon {
          width: 24px;
          height: 24px;
          color: #6366f1;
          filter: drop-shadow(0 2px 4px rgba(99, 102, 241, 0.2));
        }

        .header-text h3 {
          font-size: 18px;
          font-weight: 700;
          background: linear-gradient(135deg, #374151, #6b7280);
          background-clip: text;
          -webkit-background-clip: text;
          -webkit-text-fill-color: transparent;
          margin: 0;
          line-height: 1.2;
        }

        .header-text .subtitle {
          font-size: 12px;
          color: #8b5cf6;
          font-weight: 500;
          margin-top: 2px;
          text-transform: uppercase;
          letter-spacing: 0.5px;
        }

        .toggle-button {
          background: none;
          border: none;
          color: #6b7280;
          cursor: pointer;
          padding: 8px;
          border-radius: 12px;
          transition: all 0.2s ease;
        }

        .toggle-button:hover {
          background: rgba(99, 102, 241, 0.1);
          color: #6366f1;
          transform: scale(1.1);
        }

        .selection-toggle {
          padding: 20px 24px;
          border-bottom: 1px solid rgba(255, 255, 255, 0.1);
        }

        .toggle-btn {
          width: 100%;
          padding: 16px 24px;
          border: none;
          border-radius: 16px;
          font-weight: 600;
          font-size: 14px;
          cursor: pointer;
          display: flex;
          items-center;
          justify-content: center;
          gap: 12px;
          transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
          position: relative;
          overflow: hidden;
        }

        .toggle-btn::before {
          content: '';
          position: absolute;
          top: 0;
          left: -100%;
          width: 100%;
          height: 100%;
          background: linear-gradient(90deg,
            transparent,
            rgba(255, 255, 255, 0.2),
            transparent);
          transition: left 0.6s ease;
        }

        .toggle-btn:hover::before {
          left: 100%;
        }

        .toggle-btn-active {
          background: linear-gradient(135deg, #6366f1, #8b5cf6);
          color: white;
          box-shadow:
            0 8px 32px rgba(99, 102, 241, 0.4),
            inset 0 1px 0 rgba(255, 255, 255, 0.2);
          transform: translateY(-1px);
        }

        .toggle-btn-active:hover {
          background: linear-gradient(135deg, #5b5fc7, #7c3aed);
          box-shadow:
            0 12px 40px rgba(99, 102, 241, 0.5),
            inset 0 1px 0 rgba(255, 255, 255, 0.3);
          transform: translateY(-2px);
        }

        .toggle-btn-inactive {
          background: rgba(249, 250, 251, 0.8);
          color: #6b7280;
          border: 1px solid rgba(229, 231, 235, 0.6);
        }

        .toggle-btn-inactive:hover {
          background: rgba(243, 244, 246, 0.9);
          color: #4b5563;
          transform: translateY(-1px);
          box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
        }

        .features-section {
          padding: 20px 24px 24px;
          transition: all 0.3s ease;
          max-height: ${isExpanded ? '600px' : '0px'};
          overflow: hidden;
        }

        .features-header {
          display: flex;
          items: center;
          justify-content: space-between;
          margin-bottom: 16px;
        }

        .features-title {
          display: flex;
          items: center;
          gap: 10px;
          font-weight: 600;
          color: #374151;
          font-size: 16px;
        }

        .features-count {
          background: linear-gradient(135deg, #6366f1, #8b5cf6);
          color: white;
          padding: 4px 12px;
          border-radius: 20px;
          font-size: 12px;
          font-weight: 700;
          min-width: 32px;
          text-align: center;
          box-shadow: 0 2px 8px rgba(99, 102, 241, 0.3);
        }

        .clear-button {
          background: rgba(239, 68, 68, 0.1);
          border: 1px solid rgba(239, 68, 68, 0.2);
          color: #dc2626;
          border-radius: 10px;
          padding: 8px 12px;
          font-size: 12px;
          font-weight: 500;
          cursor: pointer;
          transition: all 0.2s ease;
          display: flex;
          items-center;
          gap: 6px;
        }

        .clear-button:hover {
          background: rgba(239, 68, 68, 0.2);
          color: #b91c1c;
          transform: scale(1.05);
        }

        .empty-state {
          text-align: center;
          padding: 32px 16px;
          color: #9ca3af;
        }

        .empty-state-icon {
          width: 48px;
          height: 48px;
          margin: 0 auto 16px;
          opacity: 0.5;
        }

        .empty-message {
          font-size: 14px;
          line-height: 1.5;
          margin: 0;
        }

        .features-list {
          max-height: 400px;
          overflow-y: auto;
          scrollbar-width: thin;
          scrollbar-color: rgba(99, 102, 241, 0.3) transparent;
        }

        .features-list::-webkit-scrollbar {
          width: 4px;
        }

        .features-list::-webkit-scrollbar-track {
          background: transparent;
        }

        .features-list::-webkit-scrollbar-thumb {
          background: rgba(99, 102, 241, 0.3);
          border-radius: 2px;
        }

        .features-list::-webkit-scrollbar-thumb:hover {
          background: rgba(99, 102, 241, 0.5);
        }

        .feature-item {
          background: rgba(255, 255, 255, 0.7);
          border: 1px solid rgba(255, 255, 255, 0.3);
          border-radius: 16px;
          padding: 16px;
          margin-bottom: 12px;
          transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
          position: relative;
          overflow: hidden;
        }

        .feature-item::before {
          content: '';
          position: absolute;
          top: 0;
          left: 0;
          width: 4px;
          height: 100%;
          background: linear-gradient(135deg, #6366f1, #8b5cf6);
          transform: scaleY(0);
          transition: transform 0.3s ease;
        }

        .feature-item:hover::before {
          transform: scaleY(1);
        }

        .feature-item:hover {
          background: rgba(255, 255, 255, 0.9);
          transform: translateX(4px) scale(1.02);
          box-shadow:
            0 8px 32px rgba(99, 102, 241, 0.15),
            0 0 0 1px rgba(255, 255, 255, 0.5);
        }

        .feature-header {
          display: flex;
          justify-content: space-between;
          align-items: flex-start;
          margin-bottom: 8px;
        }

        .feature-name {
          font-weight: 600;
          color: #111827;
          font-size: 14px;
          line-height: 1.3;
          flex: 1;
          margin-right: 8px;
        }

        .feature-id {
          font-size: 11px;
          color: #9ca3af;
          font-weight: 500;
          background: rgba(156, 163, 175, 0.1);
          padding: 2px 6px;
          border-radius: 4px;
          flex-shrink: 0;
        }

        .feature-meta {
          display: flex;
          flex-wrap: wrap;
          gap: 6px;
          margin-bottom: 12px;
        }

        .layer-tag {
          font-weight: 600;
          border-width: 1px;
          border-style: solid;
        }

        .type-tag {
          background: rgba(75, 85, 99, 0.1);
          color: #4b5563;
          border: 1px solid rgba(75, 85, 99, 0.2);
          font-weight: 500;
        }

        .meta-tag {
          background: rgba(99, 102, 241, 0.1);
          color: #6366f1;
          padding: 4px 10px;
          border-radius: 8px;
          font-size: 11px;
          font-weight: 500;
          border: 1px solid rgba(99, 102, 241, 0.2);
        }

        .feature-properties {
          border-top: 1px solid rgba(229, 231, 235, 0.5);
          padding-top: 12px;
          margin-top: 12px;
        }

        .property-item {
          display: flex;
          justify-content: space-between;
          align-items: flex-start;
          padding: 4px 0;
          gap: 12px;
        }

        .property-key {
          font-size: 11px;
          color: #6b7280;
          font-weight: 500;
          text-transform: capitalize;
          flex-shrink: 0;
          min-width: 60px;
        }

        .property-value {
          font-size: 11px;
          color: #374151;
          font-weight: 400;
          text-align: right;
          word-break: break-word;
          flex: 1;
        }

        @keyframes slideIn {
          from {
            opacity: 0;
            transform: translateY(20px);
          }
          to {
            opacity: 1;
            transform: translateY(0);
          }
        }

        .feature-item {
          animation: slideIn 0.4s cubic-bezier(0.4, 0, 0.2, 1) both;
        }

        .feature-item:nth-child(1) { animation-delay: 0ms; }
        .feature-item:nth-child(2) { animation-delay: 50ms; }
        .feature-item:nth-child(3) { animation-delay: 100ms; }
        .feature-item:nth-child(4) { animation-delay: 150ms; }
        .feature-item:nth-child(5) { animation-delay: 200ms; }
      `}</style>

      {/* Header */}
      <div className="panel-header">
        <div className="header-content">
          <div className="header-title">
            <Target className="header-icon" />
            <div className="header-text">
              <h3>Sélection Spatiale</h3>
              <div className="subtitle">GIS Selection Tool</div>
            </div>
          </div>
          <button
            onClick={() => setIsExpanded(!isExpanded)}
            className="toggle-button"
          >
            {isExpanded ? <ChevronUp size={20} /> : <ChevronDown size={20} />}
          </button>
        </div>
      </div>

      {/* Selection Toggle */}
      <div className="selection-toggle">
        <button
          onClick={onToggleSelectionMode}
          className={`toggle-btn ${
            isSelectionMode ? 'toggle-btn-active' : 'toggle-btn-inactive'
          }`}
        >
          {isSelectionMode ? (
            <>
              <Sparkles size={16} />
              Mode actif
            </>
          ) : (
            <>
              <MousePointer size={16} />
              Activer la sélection
            </>
          )}
        </button>
      </div>

      {/* Features Section */}
      <div className="features-section">
        <div className="features-header">
          <div className="features-title">
            <Layers3 size={18} />
            Éléments sélectionnés
            <span className="features-count">
              {selectedFeatures.length}
            </span>
          </div>
          {selectedFeatures.length > 0 && (
            <button onClick={onClearSelection} className="clear-button">
              <X size={12} />
              Effacer
            </button>
          )}
        </div>

        {selectedFeatures.length === 0 ? (
          <div className="empty-state">
            <Target className="empty-state-icon" />
            <p className="empty-message">
              Activez le mode sélection et dessinez une zone sur la carte pour sélectionner des éléments
            </p>
          </div>
        ) : (
          <div className="features-list">
            {selectedFeatures.map((feature, index) => {
              const getLayerName = (layerId: number) => {
                const layer = layers.find(l => l.id === layerId);
                return layer ? layer.name : `Couche ${layerId}`;
              };

              const getDisplayName = () => {
                // Try various property names for display
                if (feature.properties?.name) return feature.properties.name;
                if (feature.properties?.nom) return feature.properties.nom;
                if (feature.properties?.title) return feature.properties.title;
                if (feature.properties?.libelle) return feature.properties.libelle;
                if (feature.properties?.denomination) return feature.properties.denomination;
                if (feature.properties?.ligne) return `Ligne ${feature.properties.ligne}`;
                if (feature.properties?.numero) return `Ligne ${feature.properties.numero}`;

                // For transport features, check for line names or numbers
                if (feature.layerId === 4) {
                  if (feature.properties?.color) {
                    // Extract line info from color or other properties
                    const colorToLine: { [key: string]: string } = {
                      '#FFCD00': 'Ligne 1',
                      '#0055C8': 'Ligne 2',
                      '#837902': 'Ligne 3',
                      '#CF009E': 'Ligne 4',
                      '#FF7E2E': 'Ligne 5',
                      '#6ECA97': 'Ligne 6',
                      '#FA9ABA': 'Ligne 7',
                      '#E19BDF': 'Ligne 8',
                      '#B6BD00': 'Ligne 9',
                      '#C9910D': 'Ligne 10',
                      '#704B1C': 'Ligne 11',
                      '#007852': 'Ligne 12',
                      '#6EC4E8': 'Ligne 13',
                      '#62259D': 'Ligne 14',
                      '#F14C4D': 'RER A',
                      '#4266B2': 'RER B',
                      '#F99D1C': 'RER C',
                      '#009639': 'RER D',
                      '#281181': 'RER E'
                    };
                    if (colorToLine[feature.properties.color]) {
                      return colorToLine[feature.properties.color];
                    }
                  }
                }

                return `Élément #${feature.id}`;
              };

              const getFeatureType = () => {
                // Get type from properties first, then fallback to layer-based logic
                if (feature.properties?.type) {
                  // Clean up type display
                  const type = feature.properties.type;
                  if (type === 'Metro') return 'Métro';
                  if (type === 'RER') return 'RER';
                  if (type === 'monument') return 'Monument';
                  if (type === 'parc' || type === 'jardin') return 'Parc';
                  if (type === 'musee' || type === 'museum') return 'Musée';
                  return type;
                }

                if (feature.properties?.category) return feature.properties.category;
                if (feature.properties?.style) return feature.properties.style;

                // Fallback to layer-based type detection using featureLayerId
                if (featureLayerId === 2) return 'Monument';
                if (featureLayerId === 3) return 'Parc';
                if (featureLayerId === 4) return 'Transport';
                if (featureLayerId === 5) return 'Musée';
                return 'Élément';
              };

              const getLayerColor = (layerId: number) => {
                const layer = layers.find(l => l.id === layerId);
                if (!layer) return '#6366f1';

                // Assign colors based on layer name
                const name = layer.name.toLowerCase();
                if (name.includes('monument')) return '#dc2626'; // Rouge pour monuments
                if (name.includes('parc') || name.includes('jardin')) return '#059669'; // Vert pour parcs
                if (name.includes('métro') || name.includes('metro') || name.includes('ligne')) return '#2563eb'; // Bleu pour métro
                if (name.includes('musée') || name.includes('musee')) return '#7c3aed'; // Violet pour musées
                if (name.includes('default')) return '#6b7280'; // Gris pour default

                // Default color
                return '#6366f1';
              };

              // Get the correct layerId - it could be in feature.layerId or feature.properties.layerId
              const featureLayerId = feature.layerId || feature.properties?.layerId;

              return (
                <div key={feature.id} className="feature-item" style={{'--layer-color': getLayerColor(featureLayerId)} as any}>
                  <div className="feature-header">
                    <div className="feature-name">
                      {getDisplayName()}
                    </div>
                    <div className="feature-id">#{feature.id}</div>
                  </div>

                  <div className="feature-meta">
                    <span className="meta-tag layer-tag" style={{background: `${getLayerColor(featureLayerId)}15`, color: getLayerColor(featureLayerId), borderColor: `${getLayerColor(featureLayerId)}30`}}>
                      {getLayerName(featureLayerId)}
                    </span>
                    <span className="meta-tag type-tag">
                      {getFeatureType()}
                    </span>
                  </div>

                  {feature.properties && Object.keys(feature.properties).length > 0 && (
                    <div className="feature-properties">
                      {Object.entries(feature.properties)
                        .filter(([key, value]) => {
                          // Skip these display-related keys
                          if (['name', 'nom', 'title', 'libelle', 'type', 'category', 'style', 'color', 'ligne', 'numero'].includes(key)) {
                            return false;
                          }
                          // Skip null values
                          if (value === null || value === 'null' || value === undefined) {
                            return false;
                          }
                          return true;
                        })
                        .sort(([keyA], [keyB]) => {
                          // Prioritize important keys
                          const priority: { [key: string]: number } = {
                            'terminus_1': 1, 'terminus_2': 2, 'stations': 3, 'opened': 4,
                            'adresse': 1, 'arrondissement': 2, 'architect': 3,
                            'superficie': 1, 'ouverture': 2, 'metro_proche': 3,
                            'collection': 1, 'horaires': 2, 'prix': 3,
                            'validFromUtc': 5, 'validToUtc': 6, 'layerId': 10
                          };
                          return (priority[keyA] || 999) - (priority[keyB] || 999);
                        })
                        .slice(0, 3)
                        .map(([key, value]) => {
                          // Translate common keys to French
                          const keyTranslations: { [key: string]: string } = {
                            'terminus_1': 'Terminus 1',
                            'terminus_2': 'Terminus 2',
                            'stations': 'Stations',
                            'opened': 'Ouverture',
                            'adresse': 'Adresse',
                            'arrondissement': 'Arrdt',
                            'architect': 'Architecte',
                            'superficie': 'Superficie',
                            'ouverture': 'Ouvert',
                            'metro_proche': 'Métro',
                            'collection': 'Collection',
                            'horaires': 'Horaires',
                            'prix': 'Prix',
                            'validFromUtc': 'Valide depuis',
                            'validToUtc': 'Valide jusqu\'à',
                            'layerId': 'Couche'
                          };

                          const displayKey = keyTranslations[key] || key;
                          let displayValue = String(value);

                          // Format specific values
                          if (key === 'superficie' && !isNaN(Number(value))) {
                            displayValue = `${Number(value).toLocaleString()} m²`;
                          } else if (key === 'stations' && !isNaN(Number(value))) {
                            displayValue = `${value} stations`;
                          } else if (key === 'opened' && value.toString().match(/^\d{4}$/)) {
                            displayValue = value.toString();
                          } else if ((key === 'validFromUtc' || key === 'validToUtc') && value) {
                            // Format dates
                            try {
                              const date = new Date(value.toString());
                              displayValue = date.toLocaleDateString('fr-FR');
                            } catch {
                              displayValue = value.toString();
                            }
                          } else if (displayValue.length > 30) {
                            displayValue = `${displayValue.substring(0, 30)}...`;
                          }

                          return (
                            <div key={key} className="property-item">
                              <span className="property-key">{displayKey}</span>
                              <span className="property-value">{displayValue}</span>
                            </div>
                          );
                        })}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
};