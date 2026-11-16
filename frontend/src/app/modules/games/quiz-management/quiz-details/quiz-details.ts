import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { MapLocation, MapView } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuizDto } from '../../api/games-api-types';
import { QuizManagement } from '../quiz-management';
import {
  QuizFormModel,
  addAnswer,
  addQuestion,
  emptyQuestion,
  pickLocation,
  quizFormSchema,
  removeAnswer,
  removeLocation,
  removeQuestion,
  toQuizDto,
  toQuizFormModel,
} from '../quiz-form-schema';

@Component({
  imports: [FormField, MapView, RouterLink],
  selector: 'app-quiz-details',
  styleUrl: './quiz-details.scss',
  templateUrl: './quiz-details.html',
})
export class QuizDetails {
  private readonly management = inject(QuizManagement);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly quiz = httpResource<QuizDto>(() => `/api/games/quizzes/${this.id()}`);

  // The read-only map gets the saved location as its only pin.
  protected readonly savedLocation = computed<MapLocation[]>(() => {
    const location = this.quiz.value()?.location;
    return location ? [{ latitude: location.latitude, longitude: location.longitude }] : [];
  });

  protected readonly confirmingAction = signal<'delete' | 'publish' | 'retire' | 'archive' | 'repeat' | null>(
    null,
  );
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly editing = signal(false);
  protected readonly editError = signal<string | null>(null);

  private readonly model = signal<QuizFormModel>({
    name: '',
    instructions: '',
    xp: 10,
    questions: [emptyQuestion()],
    location: null,
    rangeInMeters: null,
  });

  protected readonly editForm = form(this.model, quizFormSchema);

  // Edit is shown only after the quiz has loaded, so the form is filled with that quiz.
  protected startEdit(quiz: QuizDto): void {
    this.error.set(null);
    this.editError.set(null);
    this.confirmingAction.set(null);
    this.editForm().reset(toQuizFormModel(quiz));
    this.editing.set(true);
  }

  protected cancelEdit(): void {
    this.editing.set(false);
  }

  protected pickLocation(location: MapLocation): void {
    this.model.update((quiz) => pickLocation(quiz, location));
  }

  protected removeLocation(): void {
    this.model.update((quiz) => removeLocation(quiz));
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

  protected askToConfirm(action: 'delete' | 'publish' | 'retire' | 'archive' | 'repeat'): void {
    this.error.set(null);
    this.confirmingAction.set(action);
  }

  protected cancelConfirm(): void {
    this.confirmingAction.set(null);
  }

  protected async confirm(): Promise<void> {
    const action = this.confirmingAction();
    this.pending.set(true);
    this.error.set(null);
    try {
      if (action === 'delete') {
        await this.management.delete(this.id());
        await this.router.navigate(['/games/challenges']);
        return;
      }
      if (action === 'publish') {
        await this.management.publish(this.id());
      } else if (action === 'retire') {
        await this.management.retire(this.id());
      } else if (action === 'archive') {
        await this.management.archive(this.id());
      } else {
        await this.management.repeat(this.id());
      }
      this.quiz.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, `Could not ${action} the quiz.`));
      this.quiz.reload();
    } finally {
      this.confirmingAction.set(null);
      this.pending.set(false);
    }
  }
}
