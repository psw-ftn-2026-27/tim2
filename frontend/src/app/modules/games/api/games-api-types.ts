export interface CreateSocialChallengeDto {
  name: string;
  start: string;
  duration: string;
  requiredParticipants: number;
  rangeInMeters: number;
  xp: number;
  completionMessage: string;
}

export interface UpdateSocialChallengeDto {
  name: string;
  start: string;
  duration: string;
  requiredParticipants: number;
  rangeInMeters: number;
  xp: number;
  completionMessage: string;
}

export type ChallengeType = 'SocialChallenge' | 'Quiz';

export interface ChallengeDto {
  id: string;
  type: ChallengeType;
  name: string;
  xp: number;
  status: string;
  start: string | null;
  duration: string | null;
  end: string | null;
  requiredParticipants: number | null;
  rangeInMeters: number | null;
  completionMessage: string | null;
}

export type QuizStatus = 'Draft' | 'Published' | 'Retired' | 'Archived';

export interface AnswerOptionDto {
  text: string;
  isCorrect: boolean;
}

export interface QuestionDto {
  text: string;
  options: AnswerOptionDto[];
}

export interface QuizLocationDto {
  latitude: number;
  longitude: number;
  rangeInMeters: number;
}

export interface CreateQuizDto {
  name: string;
  instructions: string;
  xp: number;
  questions: QuestionDto[];
  location: QuizLocationDto | null;
}

export interface UpdateQuizDto {
  name: string;
  instructions: string;
  xp: number;
  questions: QuestionDto[];
  location: QuizLocationDto | null;
}

export interface QuizDto {
  id: string;
  name: string;
  instructions: string;
  xp: number;
  status: QuizStatus;
  questions: QuestionDto[];
  location: QuizLocationDto | null;
}
