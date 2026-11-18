import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MapLocation, MapView } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDetailsDto } from '../../api/exploration-api-types';
import { TourBrowsing } from '../tour-browsing';

@Component({
  imports: [MapView, RouterLink],
  selector: 'app-tour-details',
  styleUrl: './tour-details.scss',
  templateUrl: './tour-details.html',
})
export class TourDetails {
  private readonly tourBrowsing = inject(TourBrowsing);
  private readonly router = inject(Router);

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

  protected readonly startError = signal<string | null>(null);

  // Starting a tour that is already active resumes it, so the same click leads to the same page.
  protected async start(): Promise<void> {
    this.startError.set(null);
    try {
      await this.tourBrowsing.startTour(this.id());
      await this.router.navigate(['/exploration/tours', this.id(), 'run']);
    } catch (failure) {
      this.startError.set(serverMessage(failure, 'Could not start the tour.'));
    }
  }
}
