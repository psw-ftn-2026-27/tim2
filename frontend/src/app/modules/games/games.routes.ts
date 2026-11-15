import { Routes } from '@angular/router';
import { GamesHome } from './games-home/games-home';
import { QuizDetails } from './quiz-management/quiz-details/quiz-details';
import { CreateSocialChallenge } from './social-challenge-management/create-social-challenge/create-social-challenge';
import { ChallengeList } from './challenge-overview/challenge-list/challenge-list';

export const gamesRoutes: Routes = [
  { path: '', component: GamesHome },
  { path: 'challenges', component: ChallengeList },
  { path: 'challenges/create', component: CreateSocialChallenge },
  // 'challenges/quiz/create' goes here, before 'challenges/quiz/:id', so that 'create' is not read as an id.
  { path: 'challenges/quiz/:id', component: QuizDetails },
];
