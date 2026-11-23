import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuizProgressDto } from '../../api/games-api-types';
import { QuizSolving } from '../quiz-solving';

@Component({
  imports: [RouterLink],
  selector: 'app-solve-quiz',
  styleUrl: './solve-quiz.scss',
  templateUrl: './solve-quiz.html',
})
export class SolveQuiz {
  private readonly solving = inject(QuizSolving);

  readonly id = input.required<string>();

  // The server always answers with the first unanswered question, so leaving and coming back continues the quiz.
  protected readonly progress = httpResource<QuizProgressDto>(
    () => `/api/games/quizzes/${this.id()}/progress`,
  );

  // True after an answer is accepted: the page shows whether it was correct before the next question.
  protected readonly answered = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // Answer buttons are shown only for the current question, so the question index comes from it.
  protected async answer(questionIndex: number, optionIndex: number): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.solving.answer(this.id(), { questionIndex, optionIndex });
      this.answered.set(true);
    } catch (failure) {
      // Shown above the question; when the quiz turns out to be solved already, the result replaces it.
      this.error.set(serverMessage(failure, 'Could not save the answer.'));
    } finally {
      this.progress.reload();
      this.pending.set(false);
    }
  }

  protected nextQuestion(): void {
    this.answered.set(false);
  }
}
