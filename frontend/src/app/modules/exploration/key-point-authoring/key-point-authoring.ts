import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { KeyPointDataDto, MoveKeyPointDto } from '../api/exploration-api-types';

const BASE_URL = '/api/exploration/tours';

@Injectable({ providedIn: 'root' })
export class KeyPointAuthoring {
  private readonly http = inject(HttpClient);

  async add(tourId: string, dto: KeyPointDataDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${tourId}/key-points`, dto));
  }

  async change(tourId: string, keyPointId: string, dto: KeyPointDataDto): Promise<void> {
    await firstValueFrom(
      this.http.put<void>(`${BASE_URL}/${tourId}/key-points/${keyPointId}`, dto),
    );
  }

  async remove(tourId: string, keyPointId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${tourId}/key-points/${keyPointId}`));
  }

  async move(tourId: string, keyPointId: string, dto: MoveKeyPointDto): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/${tourId}/key-points/${keyPointId}/move`, dto),
    );
  }
}
