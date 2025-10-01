import axios from 'axios';
import { fixObjectEncoding } from '../utils/encoding';

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

interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

interface Stats {
  totalFeatures: number;
  filteredFeatures: number;
  executionTimeMs: number;
}

interface ExportResponse {
  blob: Blob;
  filename: string;
}

const BASE_URL = 'http://localhost:5050/api';

const api = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const layersApi = {
  getAll: async (): Promise<Layer[]> => {
    const response = await api.get<Layer[]>('/layers');
    // Fix encoding issues in layer names and metadata
    const fixedData = fixObjectEncoding(response.data);
    return fixedData;
  },

  getById: async (id: number): Promise<Layer> => {
    const response = await api.get<Layer>(`/layers/${id}`);
    // Fix encoding issues in layer data
    const fixedData = fixObjectEncoding(response.data);
    return fixedData;
  },

  importGeoJson: async (layerId: number, fileName?: string): Promise<void> => {
    let url = `/layers/import?layerId=${layerId}`;
    if (fileName) {
      url += `&fileName=${encodeURIComponent(fileName)}`;
    }
    await api.post(url);
  },
};

export const clusterApi = {
  getClustersByLayer: async (
    layerId: number,
    zoom: number,
    bbox?: string
  ): Promise<any> => {
    const params = new URLSearchParams();
    if (zoom !== undefined) params.append('zoom', zoom.toString());
    if (bbox) params.append('bbox', bbox);
    params.append('clusterRadius', '50');

    const response = await api.get(`/cluster/${layerId}?${params.toString()}`);
    // Fix encoding issues in cluster data
    const fixedData = fixObjectEncoding(response.data);
    return fixedData;
  }
};

export const featuresApi = {
  getFeatures: async (layerId: number, filters: FilterParams = {}): Promise<PaginatedResponse<Feature>> => {
    const params = new URLSearchParams();

    if (filters.bbox) params.append('bbox', filters.bbox);
    if (filters.operation) params.append('operation', filters.operation);
    if (filters.bufferMeters) params.append('bufferMeters', filters.bufferMeters.toString());
    if (filters.validFrom) params.append('validFrom', filters.validFrom);
    if (filters.validTo) params.append('validTo', filters.validTo);
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());

    const response = await api.get<PaginatedResponse<Feature>>(`/features/${layerId}?${params.toString()}`);

    // Fix encoding issues in the response data
    const fixedData = fixObjectEncoding(response.data);
    return fixedData;
  },

  getStats: async (layerId: number, filters: FilterParams = {}): Promise<Stats> => {
    const params = new URLSearchParams();

    if (filters.bbox) params.append('bbox', filters.bbox);
    if (filters.operation) params.append('operation', filters.operation);
    if (filters.bufferMeters) params.append('bufferMeters', filters.bufferMeters.toString());
    if (filters.validFrom) params.append('validFrom', filters.validFrom);
    if (filters.validTo) params.append('validTo', filters.validTo);

    const response = await api.get<Stats>(`/features/${layerId}/stats?${params.toString()}`);
    return response.data;
  },
};

export const exportApi = {
  exportGeoJson: async (layerId: number, filters: FilterParams = {}): Promise<ExportResponse> => {
    const params = new URLSearchParams();

    if (filters.bbox) params.append('bbox', filters.bbox);
    if (filters.operation) params.append('operation', filters.operation);
    if (filters.bufferMeters) params.append('bufferMeters', filters.bufferMeters.toString());
    if (filters.validFrom) params.append('validFrom', filters.validFrom);
    if (filters.validTo) params.append('validTo', filters.validTo);

    const response = await api.get(`/export/${layerId}/geojson?${params.toString()}`, {
      responseType: 'blob',
    });

    const filename = response.headers['content-disposition']?.split('filename=')?.[1]?.replace(/"/g, '') || 'export.geojson';

    return {
      blob: response.data,
      filename,
    };
  },

  exportCsv: async (layerId: number, filters: FilterParams = {}): Promise<ExportResponse> => {
    const params = new URLSearchParams();

    if (filters.bbox) params.append('bbox', filters.bbox);
    if (filters.operation) params.append('operation', filters.operation);
    if (filters.bufferMeters) params.append('bufferMeters', filters.bufferMeters.toString());
    if (filters.validFrom) params.append('validFrom', filters.validFrom);
    if (filters.validTo) params.append('validTo', filters.validTo);

    const response = await api.get(`/export/${layerId}/csv?${params.toString()}`, {
      responseType: 'blob',
    });

    const filename = response.headers['content-disposition']?.split('filename=')?.[1]?.replace(/"/g, '') || 'export.csv';

    return {
      blob: response.data,
      filename,
    };
  },
};

export const downloadFile = (blob: Blob, filename: string): void => {
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  window.URL.revokeObjectURL(url);
};

export { api };