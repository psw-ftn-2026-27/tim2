import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const LOCATION_CHALLENGES_URL = '/api/games/location-challenges';

@Injectable({ providedIn: 'root' })
export class LocationChallengeSolving {
  private readonly http = inject(HttpClient);

  // No coordinates are sent: the server reads the saved position itself.
  // A check that finds no point also succeeds; the page learns what was found from the progress.
  async check(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${LOCATION_CHALLENGES_URL}/${id}/checks`, null));
  }
}
