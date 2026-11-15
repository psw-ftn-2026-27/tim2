import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuizDto } from '../../api/games-api-types';
import { QuizManagement } from '../quiz-management';
import {
  QuizFormModel,
  addAnswer,
  addQuestion,
  emptyQuestion,
  quizFormSchema,
  removeAnswer,
  removeQuestion,
  toQuizDto,
  toQuizFormModel,
} from '../quiz-form-schema';

@Component({
  imports: [FormField, RouterLink],
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

  protected readonly editing = signal(false);
  protected readonly editError = signal<string | null>(null);

  private readonly model = signal<QuizFormModel>({
    name: '',
    instructions: '',
    xp: 10,
    questions: [emptyQuestion()],
  });

  protected readonly editForm = form(this.model, quizFormSchema);

  // Edit is shown only after the quiz has loaded, so the form is filled with that quiz.
  protected startEdit(quiz: QuizDto): void {
    this.error.set(null);
    this.editError.set(null);
    this.confirmingDelete.set(false);
    this.editForm().reset(toQuizFormModel(quiz));
    this.editing.set(true);
  }

  protected cancelEdit(): void {
    this.editing.set(false);
  }

  protected addQuestion(): void {
    this.model.update((quiz) => addQuestion(quiz));
  }

  protected removeQuestion(questionIndex: number): void {
    this.model.update((quiz) => removeQuestion(quiz, questionIndex));
  }

  protected addAnswer(questionIndex: number): void {
    this.model.update((quiz) => addAnswer(quiz, questionIndex));
  }

  protected removeAnswer(questionIndex: number, answerIndex: number): void {
    this.model.update((quiz) => removeAnswer(quiz, questionIndex, answerIndex));
  }

  protected async saveEdit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.editError.set(null);
    try {
      await this.management.update(this.id(), toQuizDto(this.model()));
      this.editing.set(false);
      this.quiz.reload();
    } catch (failure) {
      this.editError.set(serverMessage(failure, 'Could not update the quiz.'));
    } finally {
      this.pending.set(false);
    }
  }

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
