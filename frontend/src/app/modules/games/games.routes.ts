import { Routes } from '@angular/router';
import { GamesHome } from './games-home/games-home';
import { CreateQuiz } from './quiz-management/create-quiz/create-quiz';
import { QuizDetails } from './quiz-management/quiz-details/quiz-details';
import { CreateSocialChallenge } from './social-challenge-management/create-social-challenge/create-social-challenge';
import { ChallengeList } from './challenge-overview/challenge-list/challenge-list';
import { CreateLocationChallenge } from './location-challenge-management/create-location-challenge/create-location-challenge';
import { LocationChallengeDetails } from './location-challenge-management/location-challenge-details/location-challenge-details';

export const gamesRoutes: Routes = [
  { path: '', component: GamesHome },
  { path: 'challenges', component: ChallengeList },
  { path: 'challenges/create', component: CreateSocialChallenge },
  { path: 'challenges/quiz/create', component: CreateQuiz },
  { path: 'challenges/quiz/:id', component: QuizDetails },
  { path: 'challenges/location/create', component: CreateLocationChallenge },
  { path: 'challenges/location/:id', component: LocationChallengeDetails },
];
