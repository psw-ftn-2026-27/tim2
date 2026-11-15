import {
  SchemaPathTree,
  applyEach,
  max,
  maxLength,
  min,
  minLength,
  required,
} from '@angular/forms/signals';
import { CreateQuizDto, QuizDto } from '../api/games-api-types';

// The client-only model of the quiz form, used by the create page and the details page.
export interface QuizFormModel {
  name: string;
  instructions: string;
  xp: number;
  questions: QuizFormQuestion[];
}

export interface QuizFormQuestion {
  text: string;
  answers: QuizFormAnswer[];
  // The index of the correct answer as text ('0', '1', ...), because a radio input's value
  // is always text. An empty string means that no answer is marked as correct yet.
  correct: string;
}

export interface QuizFormAnswer {
  text: string;
}

export function quizFormSchema(path: SchemaPathTree<QuizFormModel>): void {
  required(path.name, { message: 'Name is required.' });
  minLength(path.name, 3, { message: 'Name must have at least 3 characters.' });
  maxLength(path.name, 60, { message: 'Name can have at most 60 characters.' });
  required(path.instructions, { message: 'Instructions are required.' });
  maxLength(path.instructions, 1000, {
    message: 'Instructions can have at most 1000 characters.',
  });
  required(path.xp, { message: 'XP is required.' });
  min(path.xp, 1, { message: 'XP must be at least 1.' });
  max(path.xp, 1000, { message: 'XP can be at most 1000.' });

  applyEach(path.questions, (question) => {
    required(question.text, { message: 'Question text is required.' });
    maxLength(question.text, 200, { message: 'Question text can have at most 200 characters.' });
    required(question.correct, { message: 'Select the correct answer.' });

    applyEach(question.answers, (answer) => {
      required(answer.text, { message: 'Answer text is required.' });
      maxLength(answer.text, 100, { message: 'Answer text can have at most 100 characters.' });
    });
  });
}

export function emptyQuestion(): QuizFormQuestion {
  return { text: '', answers: [{ text: '' }, { text: '' }], correct: '' };
}

export function addQuestion(quiz: QuizFormModel): QuizFormModel {
  return { ...quiz, questions: [...quiz.questions, emptyQuestion()] };
}

export function removeQuestion(quiz: QuizFormModel, questionIndex: number): QuizFormModel {
  return { ...quiz, questions: quiz.questions.filter((_, index) => index !== questionIndex) };
}

export function addAnswer(quiz: QuizFormModel, questionIndex: number): QuizFormModel {
  return changeQuestion(quiz, questionIndex, (question) => ({
    ...question,
    answers: [...question.answers, { text: '' }],
  }));
}

// Removing an answer keeps `correct` pointing at the same answer: an answer before the
// correct one shifts the index down by one, removing the correct answer itself clears the
// selection, and an answer after it leaves the index unchanged.
export function removeAnswer(
  quiz: QuizFormModel,
  questionIndex: number,
  answerIndex: number,
): QuizFormModel {
  return changeQuestion(quiz, questionIndex, (question) => {
    let correct = question.correct;
    if (correct !== '') {
      const correctIndex = Number(correct);
      if (answerIndex < correctIndex) {
        correct = String(correctIndex - 1);
      } else if (answerIndex === correctIndex) {
        correct = '';
      }
    }
    return {
      ...question,
      answers: question.answers.filter((_, index) => index !== answerIndex),
      correct,
    };
  });
}

// CreateQuizDto and UpdateQuizDto have the same fields, so the result is sent by both commands.
export function toQuizDto(quiz: QuizFormModel): CreateQuizDto {
  return {
    name: quiz.name,
    instructions: quiz.instructions,
    xp: quiz.xp,
    questions: quiz.questions.map((question) => ({
      text: question.text,
      options: question.answers.map((answer, index) => ({
        text: answer.text,
        isCorrect: String(index) === question.correct,
      })),
    })),
  };
}

export function toQuizFormModel(quiz: QuizDto): QuizFormModel {
  return {
    name: quiz.name,
    instructions: quiz.instructions,
    xp: quiz.xp,
    questions: quiz.questions.map((question) => {
      const correctIndex = question.options.findIndex((option) => option.isCorrect);
      return {
        text: question.text,
        answers: question.options.map((option) => ({ text: option.text })),
        correct: correctIndex === -1 ? '' : String(correctIndex),
      };
    }),
  };
}

function changeQuestion(
  quiz: QuizFormModel,
  questionIndex: number,
  change: (question: QuizFormQuestion) => QuizFormQuestion,
): QuizFormModel {
  return {
    ...quiz,
    questions: quiz.questions.map((question, index) =>
      index === questionIndex ? change(question) : question,
    ),
  };
}
