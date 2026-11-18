export interface CreateSocialChallengeDto {
  name: string;
  start: string;
  duration: string;
  requiredParticipants: number;
  rangeInMeters: number;
  xp: number;
  completionMessage: string;
  location: LocationDto | null;
}

export interface UpdateSocialChallengeDto {
  name: string;
  start: string;
  duration: string;
  requiredParticipants: number;
  rangeInMeters: number;
  xp: number;
  completionMessage: string;
  location: LocationDto | null;
}

export type SocialChallengeStatus = 'Draft' | 'Published' | 'Retired' | 'Archived';

export interface SocialChallengeDto {
  id: string;
  name: string;
  start: string;
  duration: string;
  end: string;
  requiredParticipants: number;
  rangeInMeters: number;
  xp: number;
  completionMessage: string;
  status: SocialChallengeStatus;
  location: LocationDto | null;
}

export type ChallengeType = 'SocialChallenge' | 'Quiz' | 'LocationChallenge';

export interface ChallengeDto {
  id: string;
  type: ChallengeType;
  name: string;
  xp: number;
  status: string;
  location: LocationDto | null;
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

export type LocationChallengeStatus = 'Draft' | 'Published' | 'Retired' | 'Archived';

export interface LocationDto {
  latitude: number;
  longitude: number;
}

export interface HiddenPointDto {
  imageUrl: string;
  location: LocationDto;
}

export interface CreateLocationChallengeDto {
  name: string;
  xp: number;
  startLocation: LocationDto;
  hiddenPoints: HiddenPointDto[];
}

export interface UpdateLocationChallengeDto {
  name: string;
  xp: number;
  startLocation: LocationDto;
  hiddenPoints: HiddenPointDto[];
}

export interface LocationChallengeDto {
  id: string;
  name: string;
  xp: number;
  status: LocationChallengeStatus;
  startLocation: LocationDto;
  hiddenPoints: HiddenPointDto[];
}

export interface NearbyChallengeDto {
  id: string;
  type: ChallengeType;
  name: string;
  xp: number;
  latitude: number;
  longitude: number;
  distanceInMeters: number;
  metersToRange: number;
  isStarted: boolean;
  isSolved: boolean;
  earnedXp: number | null;
  social: NearbySocialChallengeDto | null;
}

export interface NearbySocialChallengeDto {
  requiredParticipants: number;
  windowStart: string;
  isWindowOpen: boolean;
  earnedXp: number | null;
}

export interface AnswerQuizDto {
  questionIndex: number;
  optionIndex: number;
}

export interface CurrentQuestionDto {
  index: number;
  text: string;
  options: string[];
}

export interface QuizProgressDto {
  quizId: string;
  questionCount: number;
  correctCount: number;
  lastAnswerCorrect: boolean | null;
  currentQuestion: CurrentQuestionDto | null;
  percentage: number | null;
  earnedXp: number | null;
  solvedAt: string | null;
}

export type LocationChallengeProgressState = 'Unavailable' | 'NotActivated' | 'Active' | 'Solved';

export interface HiddenPointProgressDto {
  number: number;
  imageUrl: string;
  location: LocationDto | null;
}

export interface LocationChallengeProgressDto {
  challengeId: string;
  state: LocationChallengeProgressState;
  name: string | null;
  center: LocationDto | null;
  searchRadiusInMeters: number | null;
  hiddenPoints: HiddenPointProgressDto[] | null;
  solvedAt: string | null;
  earnedXp: number | null;
}

export type SocialChallengeProgressState = 'InRun' | 'NotInRun' | 'Expired' | 'Solved';

export interface SocialChallengeProgressDto {
  challengeId: string;
  name: string;
  location: LocationDto | null;
  rangeInMeters: number;
  requiredParticipants: number;
  participantCount: number;
  lastCheckedAt: string | null;
  state: SocialChallengeProgressState;
  completionMessage: string | null;
  earnedXp: number | null;
  solvedAt: string | null;
}
