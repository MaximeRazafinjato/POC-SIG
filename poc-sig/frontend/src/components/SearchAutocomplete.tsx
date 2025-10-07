import React, { useState, useEffect, useRef, KeyboardEvent } from 'react';
import { MapPin, Layers, Map, Globe, Search, X } from 'lucide-react';
import { searchApi } from '../api/client';
import type { SearchResult } from '../types/search';
import '../styles/search.css';

interface SearchAutocompleteProps {
  onSelect: (result: SearchResult) => void;
  placeholder?: string;
  className?: string;
}

export const SearchAutocomplete: React.FC<SearchAutocompleteProps> = ({
  onSelect,
  placeholder = 'Rechercher une commune, EPCI, département...',
  className = '',
}) => {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<SearchResult[]>([]);
  const [loading, setLoading] = useState(false);
  const [selectedIndex, setSelectedIndex] = useState(-1);
  const [showResults, setShowResults] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);
  const resultsRef = useRef<HTMLDivElement>(null);

  // Debounced search
  useEffect(() => {
    if (query.length < 2) {
      setResults([]);
      setShowResults(false);
      return;
    }

    setLoading(true);
    const timer = setTimeout(async () => {
      try {
        const data = await searchApi.autocomplete(query, 10);
        setResults(data);
        setShowResults(true);
        setSelectedIndex(-1);
      } catch (error) {
        console.error('Search error:', error);
        setResults([]);
      } finally {
        setLoading(false);
      }
    }, 300);

    return () => clearTimeout(timer);
  }, [query]);

  // Close results when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        resultsRef.current &&
        !resultsRef.current.contains(event.target as Node) &&
        inputRef.current &&
        !inputRef.current.contains(event.target as Node)
      ) {
        setShowResults(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (!showResults || results.length === 0) return;

    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault();
        setSelectedIndex((prev) => Math.min(prev + 1, results.length - 1));
        break;
      case 'ArrowUp':
        e.preventDefault();
        setSelectedIndex((prev) => Math.max(prev - 1, -1));
        break;
      case 'Enter':
        e.preventDefault();
        if (selectedIndex >= 0 && selectedIndex < results.length) {
          handleSelect(results[selectedIndex]);
        }
        break;
      case 'Escape':
        e.preventDefault();
        setShowResults(false);
        setSelectedIndex(-1);
        break;
    }
  };

  const handleSelect = (result: SearchResult) => {
    onSelect(result);
    setQuery(result.label);
    setShowResults(false);
    setSelectedIndex(-1);
    inputRef.current?.blur();
  };

  const handleClear = () => {
    setQuery('');
    setResults([]);
    setShowResults(false);
    setSelectedIndex(-1);
    inputRef.current?.focus();
  };

  const getIcon = (type: string) => {
    switch (type) {
      case 'commune':
        return <MapPin size={16} />;
      case 'epci':
        return <Layers size={16} />;
      case 'departement':
        return <Map size={16} />;
      case 'coordinate':
        return <Globe size={16} />;
      default:
        return <MapPin size={16} />;
    }
  };

  const getTypeLabel = (type: string) => {
    switch (type) {
      case 'commune':
        return 'Commune';
      case 'epci':
        return 'EPCI';
      case 'departement':
        return 'Département';
      case 'coordinate':
        return 'Coordonnées';
      default:
        return '';
    }
  };

  return (
    <div className={`search-autocomplete-container ${className}`}>
      <div className="search-input-wrapper">
        <Search className="search-icon" size={18} />
        <input
          ref={inputRef}
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onKeyDown={handleKeyDown}
          onFocus={() => {
            if (results.length > 0) setShowResults(true);
          }}
          placeholder={placeholder}
          className="search-input"
          autoComplete="off"
        />
        {query && (
          <button
            onClick={handleClear}
            className="search-clear-btn"
            title="Effacer"
          >
            <X size={16} />
          </button>
        )}
        {loading && <div className="search-loading-spinner" />}
      </div>

      {showResults && results.length > 0 && (
        <div ref={resultsRef} className="search-results-dropdown">
          {results.map((result, index) => (
            <div
              key={result.id}
              className={`search-result-item ${
                index === selectedIndex ? 'selected' : ''
              }`}
              onClick={() => handleSelect(result)}
              onMouseEnter={() => setSelectedIndex(index)}
            >
              <div className="result-icon">{getIcon(result.type)}</div>
              <div className="result-content">
                <div className="result-header">
                  <span className="result-label">{result.label}</span>
                  <span className="result-type-badge">{getTypeLabel(result.type)}</span>
                </div>
                {result.secondaryLabel && (
                  <div className="result-secondary">{result.secondaryLabel}</div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {showResults && !loading && query.length >= 2 && results.length === 0 && (
        <div ref={resultsRef} className="search-results-dropdown">
          <div className="search-no-results">
            <Search size={32} />
            <p>Aucun résultat trouvé</p>
            <span>Essayez une autre recherche</span>
          </div>
        </div>
      )}
    </div>
  );
};
