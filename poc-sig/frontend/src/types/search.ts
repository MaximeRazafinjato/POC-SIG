export interface SearchResult {
  id: string;
  type: 'commune' | 'epci' | 'departement' | 'coordinate';
  label: string;
  secondaryLabel?: string;
  latitude?: number;
  longitude?: number;
  boundingBox?: string;
  score: number;
}

export interface SearchResultResponse {
  value: SearchResult[];
  isSuccess: boolean;
  status: string;
}
