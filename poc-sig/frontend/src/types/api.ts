export interface Feature {
  id: number;
  layerId: number;
  propertiesJson?: string;
  properties?: Record<string, any>;
  geometry: any;
  validFromUtc: string;
  validToUtc?: string;
}

export interface Layer {
  id: number;
  name: string;
  srid: number;
  geometryType: string;
  createdUtc: string;
  updatedUtc: string;
  metadataJson: string;
}

export interface FilterParams {
  bbox?: string;
  operation?: 'intersects' | 'within';
  bufferMeters?: number;
  validFrom?: string;
  validTo?: string;
  page?: number;
  pageSize?: number;
}

export interface Stats {
  totalFeatures: number;
  filteredFeatures: number;
  executionTimeMs: number;
}