import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const QUIZZES_URL = '/api/games/quizzes';

@Injectable({ providedIn: 'root' })
export class NearbyChallenges {
  private readonly http = inject(HttpClient);

  // No coordinates are sent: the server reads the saved position itself.
  async activateQuiz(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${QUIZZES_URL}/${id}/activate`, null));
  }
}
