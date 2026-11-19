import { httpResource } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { UserPositionDto } from '../../../../shared/api/user-position';
import { MapLocation, MapView } from '../../../../shared/map/map';
import { Positions } from '../../../../shared/positions/positions';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourProgressDto } from '../../api/exploration-api-types';
import { TourExecution } from '../tour-execution';

@Component({
  imports: [MapView, RouterLink],
  selector: 'app-active-tour',
  styleUrl: './active-tour.scss',
  templateUrl: './active-tour.html',
})
export class ActiveTour {
  private readonly tourExecution = inject(TourExecution);
  private readonly positions = inject(Positions);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly id = input.required<string>();

  // The server answers 204 while the tourist has no session for the tour, which arrives here as null.
  protected readonly progress = httpResource<TourProgressDto | null>(
    () => `/api/exploration/tours/${this.id()}/progress`,
  );

  protected readonly active = computed(
    () => this.progress.hasValue() && this.progress.value()?.status === 'Active',
  );

  // The server answers 204 while the user has no position, which arrives here as null.
  protected readonly position = httpResource<UserPositionDto | null>(() => '/api/positions/mine');

  // The pin shows the position the server keeps, so it moves only after the server confirms.
  protected readonly youAreHere = computed<MapLocation | null>(() => {
    const position = this.position.hasValue() ? this.position.value() : null;
    if (position === null) {
      return null;
    }
    return { latitude: position.latitude, longitude: position.longitude, name: 'You are here' };
  });

  // The path is drawn through the locations in order: from the tourist to the next key point.
  protected readonly pathLocations = computed<MapLocation[]>(() => {
    const progress = this.progress.hasValue() ? this.progress.value() : null;
    const keyPoint = progress === null ? null : progress.nextKeyPoint;
    if (keyPoint === null) {
      return [];
    }
    const next: MapLocation = {
      latitude: keyPoint.latitude,
      longitude: keyPoint.longitude,
      name: keyPoint.name,
    };
    const youAreHere = this.youAreHere();
    return youAreHere === null ? [next] : [youAreHere, next];
  });

  protected readonly saving = signal(false);
  protected readonly saved = signal(false);
  protected readonly failed = signal(false);

  protected readonly checking = signal(false);
  protected readonly checkError = signal<string | null>(null);

  protected readonly leaveError = signal<string | null>(null);

  constructor() {
    const timerId = setInterval(() => this.check(), 10_000);
    // Without this the timer keeps sending checks after the user has left the page.
    this.destroyRef.onDestroy(() => clearInterval(timerId));
  }

  protected async save(location: MapLocation): Promise<void> {
    if (this.saving()) {
      return;
    }
    this.saving.set(true);
    this.saved.set(false);
    this.failed.set(false);
    try {
      await this.positions.save({ latitude: location.latitude, longitude: location.longitude });
      this.position.reload();
      this.saved.set(true);
    } catch {
      this.failed.set(true);
    } finally {
      this.saving.set(false);
    }
  }

  protected async check(): Promise<void> {
    if (this.checking() || !this.active()) {
      return;
    }
    this.checking.set(true);
    try {
      await this.tourExecution.checkArrival(this.id());
      this.checkError.set(null);
    } catch (failure) {
      this.checkError.set(serverMessage(failure, 'Could not check the arrival.'));
    } finally {
      this.progress.reload();
      this.checking.set(false);
    }
  }

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
