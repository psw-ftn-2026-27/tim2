import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const CHALLENGES_URL = '/api/games/challenges';

@Injectable({ providedIn: 'root' })
export class SocialChallengeSolving {
  private readonly http = inject(HttpClient);

  // No coordinates are sent: the server reads the saved position itself. Nothing comes back, so the page reloads its progress.
  async check(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${CHALLENGES_URL}/${id}/checks`, null));
  }
}
