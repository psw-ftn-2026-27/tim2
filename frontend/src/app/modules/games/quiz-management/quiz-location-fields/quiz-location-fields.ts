import { Component, input, output } from '@angular/core';
import { MapLocation, MapView } from '../../../../shared/map/map';

// The pin part of the quiz location, shared by the create page and the details page.
// The range field stays in the pages, because it is bound to their form.
@Component({
  imports: [MapView],
  selector: 'app-quiz-location-fields',
  styleUrl: './quiz-location-fields.scss',
  templateUrl: './quiz-location-fields.html',
})
export class QuizLocationFields {
  readonly location = input<MapLocation | null>(null);

  readonly locationPicked = output<MapLocation>();
  readonly locationRemoved = output<void>();
}
