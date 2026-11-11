export type TourDifficulty = 'Easy' | 'Moderate' | 'Hard';

export type TourStatus = 'Draft' | 'Published';

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
}

export type DifficultySort = 'None' | 'Ascending' | 'Descending';

export interface TourSearchDto {
  name: string | null;
  difficulty: TourDifficulty | null;
  tags: string[] | null;
  transportModes: TransportMode[] | null;
  difficultySort: DifficultySort;
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
