import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { UserPositionDto } from '../../../../shared/api/user-position';
import { MapLocation, MapView } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { ActiveTourDto } from '../../api/exploration-api-types';
import { TourExecution } from '../tour-execution';

@Component({
  imports: [MapView, RouterLink],
  selector: 'app-active-tour',
  styleUrl: './active-tour.scss',
  templateUrl: './active-tour.html',
})
export class ActiveTour {
  private readonly tourExecution = inject(TourExecution);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  // The server answers 204 while the tour is not active for the tourist, which arrives here as null.
  protected readonly activeTour = httpResource<ActiveTourDto | null>(
    () => `/api/exploration/tours/${this.id()}/active`,
  );

  // The server answers 204 while the user has no position, which arrives here as null.
  protected readonly position = httpResource<UserPositionDto | null>(() => '/api/positions/mine');

  protected readonly youAreHere = computed<MapLocation | null>(() => {
    const position = this.position.hasValue() ? this.position.value() : null;
    if (position === null) {
      return null;
    }
    return { latitude: position.latitude, longitude: position.longitude, name: 'You are here' };
  });

  // The path is drawn through the locations in order: from the tourist to the next key point.
  protected readonly pathLocations = computed<MapLocation[]>(() => {
    const activeTour = this.activeTour.hasValue() ? this.activeTour.value() : null;
    if (activeTour === null) {
      return [];
    }
    const keyPoint = activeTour.nextKeyPoint;
    const next: MapLocation = {
      latitude: keyPoint.latitude,
      longitude: keyPoint.longitude,
      name: keyPoint.name,
    };
    const youAreHere = this.youAreHere();
    return youAreHere === null ? [next] : [youAreHere, next];
  });

  protected readonly leaveError = signal<string | null>(null);

  protected async leave(): Promise<void> {
    this.leaveError.set(null);
    try {
      await this.tourExecution.abandon(this.id());
      await this.router.navigate(['/exploration/tours', this.id()]);
    } catch (failure) {
      this.leaveError.set(serverMessage(failure, 'Could not leave the tour.'));
    }
  }
}
