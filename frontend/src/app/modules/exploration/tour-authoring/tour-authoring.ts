import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  CreateTourDto,
  RecommendedGroupSizeDto,
  TourDto,
  TourPriceDto,
  TransportTimeDto,
} from '../api/exploration-api-types';

const BASE_URL = '/api/exploration/tours';

@Injectable({ providedIn: 'root' })
export class TourAuthoring {
  private readonly http = inject(HttpClient);

  async create(dto: CreateTourDto): Promise<TourDto> {
    return firstValueFrom(this.http.post<TourDto>(BASE_URL, dto));
  }

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, {}));
  }

  async addTransportTime(id: string, dto: TransportTimeDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/transport-times`, dto));
  }

  async changeRecommendedGroupSize(id: string, dto: RecommendedGroupSizeDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}/recommended-group-size`, dto));
  }

  async changePrice(id: string, dto: TourPriceDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}/price`, dto));
  }
}
