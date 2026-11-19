import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/exploration/tours';

@Injectable({ providedIn: 'root' })
export class TourExecution {
  private readonly http = inject(HttpClient);

  async abandon(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/abandon`, {}));
  }

  async checkArrival(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/check-arrival`, {}));
  }
}
