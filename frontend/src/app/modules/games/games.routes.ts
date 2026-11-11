import { Routes } from '@angular/router';
import { GamesHome } from './games-home/games-home';
import { CreateSocialChallenge } from './social-challenge-management/create-social-challenge/create-social-challenge';
import { SocialChallengeList } from './social-challenge-management/social-challenge-list/social-challenge-list';

export const gamesRoutes: Routes = [
  { path: '', component: GamesHome },
  { path: 'challenges', component: SocialChallengeList },
  { path: 'challenges/create', component: CreateSocialChallenge },
];
