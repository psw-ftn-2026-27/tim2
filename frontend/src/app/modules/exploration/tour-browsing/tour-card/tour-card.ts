import { Component, computed, input } from '@angular/core';
import { TourDto } from '../../api/exploration-api-types';

@Component({
  selector: 'app-tour-card',
  styleUrl: './tour-card.scss',
  templateUrl: './tour-card.html',
})
export class TourCard {
  readonly tour = input.required<TourDto>();

  protected readonly excerpt = computed(() => {
    const description = this.tour().description;
    return description.length > 150 ? `${description.slice(0, 150)}…` : description;
  });
}
