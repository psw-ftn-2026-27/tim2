import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuizDto } from '../../api/games-api-types';
import { QuizManagement } from '../quiz-management';

@Component({
  imports: [RouterLink],
  selector: 'app-quiz-details',
  styleUrl: './quiz-details.scss',
  templateUrl: './quiz-details.html',
})
export class QuizDetails {
  private readonly management = inject(QuizManagement);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly quiz = httpResource<QuizDto>(() => `/api/games/quizzes/${this.id()}`);

  protected readonly confirmingDelete = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected askToConfirmDelete(): void {
    this.error.set(null);
    this.confirmingDelete.set(true);
  }

  protected cancelDelete(): void {
    this.confirmingDelete.set(false);
  }

  protected async confirmDelete(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.management.delete(this.id());
      await this.router.navigate(['/games/challenges']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the quiz.'));
    } finally {
      this.confirmingDelete.set(false);
      this.pending.set(false);
    }
  }
}
