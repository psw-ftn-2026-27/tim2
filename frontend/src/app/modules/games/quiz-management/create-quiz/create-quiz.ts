import { Component, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { MapLocation } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
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
} from '../quiz-form-schema';
import { QuizLocationFields } from '../quiz-location-fields/quiz-location-fields';

@Component({
  imports: [FormField, QuizLocationFields, RouterLink],
  selector: 'app-create-quiz',
  styleUrl: './create-quiz.scss',
  templateUrl: './create-quiz.html',
})
export class CreateQuiz {
  private readonly management = inject(QuizManagement);
  private readonly router = inject(Router);

  private readonly model = signal<QuizFormModel>({
    name: '',
    instructions: '',
    xp: 10,
    questions: [emptyQuestion()],
    location: null,
    rangeInMeters: null,
  });

  protected readonly form = form(this.model, quizFormSchema);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

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

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      const id = await this.management.create(toQuizDto(this.model()));
      await this.router.navigate(['/games/challenges/quiz', id]);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the quiz.'));
    } finally {
      this.pending.set(false);
    }
  }
}
