import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AnswerQuizDto } from '../api/games-api-types';

const QUIZZES_URL = '/api/games/quizzes';

@Injectable({ providedIn: 'root' })
export class QuizSolving {
  private readonly http = inject(HttpClient);

  async answer(id: string, dto: AnswerQuizDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${QUIZZES_URL}/${id}/answers`, dto));
  }
}
