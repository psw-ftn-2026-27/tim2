import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MapLocation, MapView } from '../../../../shared/map/map';
import { TourDetailsDto } from '../../api/exploration-api-types';

@Component({
  imports: [MapView, RouterLink],
  selector: 'app-tour-details',
  styleUrl: './tour-details.scss',
  templateUrl: './tour-details.html',
})
export class TourDetails {
  readonly id = input.required<string>();

  protected readonly tour = httpResource<TourDetailsDto>(
    () => `/api/exploration/tours/${this.id()}`,
  );

  // The server answers 404 for a tour that does not exist, is a draft, or is archived and not bought.
  protected readonly notFound = computed(() => {
    const error = this.tour.error();
    return error instanceof HttpErrorResponse && error.status === 404;
  });

  // The buyer and the author see every key point with the path; everyone else sees only the public ones.
  protected readonly fullView = computed(
    () => this.tour.hasValue() && (this.tour.value().isPurchased || this.tour.value().isOwnTour),
  );

  protected readonly keyPointLocations = computed<MapLocation[]>(() => {
    if (!this.tour.hasValue()) {
      return [];
    }
    return this.tour.value().keyPoints.map((keyPoint) => ({
      latitude: keyPoint.latitude,
      longitude: keyPoint.longitude,
      name: keyPoint.name,
    }));
  });
}
