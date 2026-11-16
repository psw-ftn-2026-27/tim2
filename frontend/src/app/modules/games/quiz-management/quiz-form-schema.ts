import {
  SchemaPathTree,
  applyEach,
  max,
  maxLength,
  min,
  minLength,
  required,
  validate,
} from '@angular/forms/signals';
import { MapLocation } from '../../../shared/map/map';
import { CreateQuizDto, QuizDto } from '../api/games-api-types';

// A pin placed while the range is empty gets this range, so a picked location is valid at once.
const DEFAULT_RANGE_IN_METERS = 50;

// P3: the same message for a pin without a range and for a range without a pin.
const LOCATION_AND_RANGE_TOGETHER =
  'Location and range must be set together. Click the map to set the location, or clear the range.';

// The client-only model of the quiz form, used by the create page and the details page.
export interface QuizFormModel {
  name: string;
  instructions: string;
  xp: number;
  questions: QuizFormQuestion[];
  // The pin on the map and the range are kept apart, so the form can show that one is missing.
  location: MapLocation | null;
  rangeInMeters: number | null;
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
  min(path.rangeInMeters, 10, { message: 'Range must be at least 10 m.' });
  max(path.rangeInMeters, 1000, { message: 'Range can be at most 1000 m.' });
  required(path.rangeInMeters, {
    when: ({ valueOf }) => valueOf(path.location) !== null,
    message: LOCATION_AND_RANGE_TOGETHER,
  });
  validate(path.rangeInMeters, ({ value, valueOf }) =>
    value() !== null && valueOf(path.location) === null
      ? { kind: 'locationAndRange', message: LOCATION_AND_RANGE_TOGETHER }
      : undefined,
  );

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

// A click on the map places or moves the pin; an existing range stays as it is.
export function pickLocation(quiz: QuizFormModel, location: MapLocation): QuizFormModel {
  return {
    ...quiz,
    location,
    rangeInMeters: quiz.rangeInMeters ?? DEFAULT_RANGE_IN_METERS,
  };
}

export function removeLocation(quiz: QuizFormModel): QuizFormModel {
  return { ...quiz, location: null, rangeInMeters: null };
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
    location:
      quiz.location === null || quiz.rangeInMeters === null
        ? null
        : {
            latitude: quiz.location.latitude,
            longitude: quiz.location.longitude,
            rangeInMeters: quiz.rangeInMeters,
          },
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
    location:
      quiz.location === null
        ? null
        : { latitude: quiz.location.latitude, longitude: quiz.location.longitude },
    rangeInMeters: quiz.location?.rangeInMeters ?? null,
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
