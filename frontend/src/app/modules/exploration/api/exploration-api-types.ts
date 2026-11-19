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
  lengthKm: number | null;
}

export type SortDirection = 'None' | 'Ascending' | 'Descending';

export interface TourSearchDto {
  name: string | null;
  difficulties: TourDifficulty[] | null;
  tags: string[] | null;
  transportModes: TransportMode[] | null;
  minPrice: number | null;
  maxPrice: number | null;
  minLengthKm: number | null;
  maxLengthKm: number | null;
  difficultySort: SortDirection;
  priceSort: SortDirection;
}

export interface TourLocationSearchDto {
  latitude: number;
  longitude: number;
  radiusKm: number;
  name: string | null;
  difficulties: TourDifficulty[] | null;
  tags: string[] | null;
  transportModes: TransportMode[] | null;
  minPrice: number | null;
  maxPrice: number | null;
  minLengthKm: number | null;
  maxLengthKm: number | null;
}

export interface LocationSearchRecordDto {
  latitude: number;
  longitude: number;
  radiusKm: number;
  name: string | null;
  difficulties: TourDifficulty[] | null;
  tags: string[] | null;
  transportModes: TransportMode[] | null;
  minPrice: number | null;
  maxPrice: number | null;
  minLengthKm: number | null;
  maxLengthKm: number | null;
  resultCount: number;
}

export interface LocatedTourDto {
  id: string;
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  price: number;
  markerLatitude: number;
  markerLongitude: number;
  lengthKm: number | null;
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

export interface KeyPointDto {
  id: string;
  order: number;
  name: string;
  description: string;
  imageUrl: string;
  latitude: number;
  longitude: number;
  secret: string | null;
  isVisibleBeforePurchase: boolean;
}

export interface TourKeyPointsDto {
  name: string;
  status: TourStatus;
  keyPoints: KeyPointDto[];
  lengthKm: number | null;
}

export interface KeyPointDataDto {
  name: string;
  description: string;
  imageUrl: string;
  latitude: number;
  longitude: number;
  secret: string | null;
  isVisibleBeforePurchase: boolean;
}

export type KeyPointMoveDirection = 'Up' | 'Down';

export interface MoveKeyPointDto {
  direction: KeyPointMoveDirection;
}

export interface TourLengthDto {
  lengthKm: number;
}

export interface TourDetailsKeyPointDto {
  order: number;
  name: string;
  description: string;
  imageUrl: string;
  latitude: number;
  longitude: number;
}

export interface TourDetailsDto {
  id: string;
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  price: number;
  lengthKm: number | null;
  recommendedGroupSize: RecommendedGroupSizeDto | null;
  transportTimes: TransportTimeDto[];
  keyPoints: TourDetailsKeyPointDto[];
  isPurchased: boolean;
  isOwnTour: boolean;
}

export interface ActiveTourKeyPointDto {
  name: string;
  latitude: number;
  longitude: number;
}

export interface ActiveTourDto {
  tourName: string;
  nextKeyPoint: ActiveTourKeyPointDto;
}
