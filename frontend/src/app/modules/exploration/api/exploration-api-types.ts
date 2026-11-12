export type TourDifficulty = 'Easy' | 'Moderate' | 'Hard';

export type TourStatus = 'Draft' | 'Published' | 'Archived';

export type TransportMode = 'Walking' | 'Bicycle' | 'Car';

export interface TransportTimeDto {
  transport: TransportMode;
  minutes: number;
}

export interface RecommendedGroupSizeDto {
  min: number;
  max: number;
}

export interface TourDto {
  id: string;
  authorId: string;
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  status: TourStatus;
  publishedAt: string | null;
  transportTimes: TransportTimeDto[];
  recommendedGroupSize: RecommendedGroupSizeDto | null;
  price: number;
  archivedAt: string | null;
}

export type SortDirection = 'None' | 'Ascending' | 'Descending';

export interface TourSearchDto {
  name: string | null;
  difficulties: TourDifficulty[] | null;
  tags: string[] | null;
  transportModes: TransportMode[] | null;
  minPrice: number | null;
  maxPrice: number | null;
  difficultySort: SortDirection;
  priceSort: SortDirection;
}

export interface CreateTourDto {
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  recommendedGroupSize: RecommendedGroupSizeDto | null;
  price: number;
}

export interface TourPriceDto {
  price: number;
}

export interface TourPreferenceDto {
  difficulties: TourDifficulty[];
  transportModes: TransportMode[];
  tags: string[];
  applicableInSearch: boolean;
}
