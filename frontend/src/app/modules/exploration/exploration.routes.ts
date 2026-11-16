import { Routes } from '@angular/router';
import { TourKeyPoints } from './key-point-authoring/tour-key-points/tour-key-points';
import { CreateTour } from './tour-authoring/create-tour/create-tour';
import { MyTours } from './tour-authoring/my-tours/my-tours';
import { MapSearch } from './tour-browsing/map-search/map-search';
import { TourList } from './tour-browsing/tour-list/tour-list';
import { Preferences } from './tour-preference-setting/preferences/preferences';

export const explorationRoutes: Routes = [
  { path: '', component: TourList },
  { path: 'mine', component: MyTours },
  { path: 'mine/:id/key-points', component: TourKeyPoints },
  { path: 'create', component: CreateTour },
  { path: 'preferences', component: Preferences },
  { path: 'map-search', component: MapSearch },
];
