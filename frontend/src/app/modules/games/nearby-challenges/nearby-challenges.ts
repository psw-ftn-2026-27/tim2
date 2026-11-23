import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const QUIZZES_URL = '/api/games/quizzes';
const LOCATION_CHALLENGES_URL = '/api/games/location-challenges';
const CHALLENGES_URL = '/api/games/challenges';

@Injectable({ providedIn: 'root' })
export class NearbyChallenges {
  private readonly http = inject(HttpClient);

  // No coordinates are sent: the server reads the saved position itself.
  async activateQuiz(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${QUIZZES_URL}/${id}/activate`, null));
  }

  // No coordinates are sent: the server reads the saved position itself.
  async activateLocationChallenge(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${LOCATION_CHALLENGES_URL}/${id}/activate`, null));
  }

  // Joins the active run of the social challenge, or starts one; no coordinates here either.
  async activateSocialChallenge(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${CHALLENGES_URL}/${id}/activate`, null));
  }
}
