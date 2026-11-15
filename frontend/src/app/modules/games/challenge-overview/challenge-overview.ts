import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { UpdateSocialChallengeDto } from '../api/games-api-types';

const SOCIAL_CHALLENGES_URL = '/api/games/challenges';
const QUIZZES_URL = '/api/games/quizzes';

@Injectable({ providedIn: 'root' })
export class ChallengeOverview {
  private readonly http = inject(HttpClient);

  async update(id: string, dto: UpdateSocialChallengeDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${SOCIAL_CHALLENGES_URL}/${id}`, dto));
  }

  async delete(id: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${SOCIAL_CHALLENGES_URL}/${id}`));
  }

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${SOCIAL_CHALLENGES_URL}/${id}/publish`, null));
  }

  async retire(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${SOCIAL_CHALLENGES_URL}/${id}/retire`, null));
  }

  async archive(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${SOCIAL_CHALLENGES_URL}/${id}/archive`, null));
  }

  async repeat(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${SOCIAL_CHALLENGES_URL}/${id}/repeat`, null));
  }

  async deleteQuiz(id: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${QUIZZES_URL}/${id}`));
  }
}
