import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { LocationSearchRecordDto } from '../api/exploration-api-types';

const BASE_URL = '/api/exploration/tour-location-searches';
const TOURS_URL = '/api/exploration/tours';

@Injectable({ providedIn: 'root' })
export class TourBrowsing {
  private readonly http = inject(HttpClient);

  async saveLocationSearch(dto: LocationSearchRecordDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(BASE_URL, dto));
  }

  async startTour(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${TOURS_URL}/${id}/start`, {}));
  }
}
