import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { UserPositionDto } from '../../../../shared/api/user-position';
import { MapLocation, MapView } from '../../../../shared/map/map';
import { Positions } from '../../../../shared/positions/positions';
import { LocationChallengeProgressDto } from '../../api/games-api-types';

@Component({
  imports: [MapView, RouterLink],
  selector: 'app-solve-location-challenge',
  styleUrl: './solve-location-challenge.scss',
  templateUrl: './solve-location-challenge.html',
})
export class SolveLocationChallenge {
  private readonly positions = inject(Positions);

  readonly id = input.required<string>();

  // The server sends the same search area and photos on every visit, wherever the player is.
  protected readonly progress = httpResource<LocationChallengeProgressDto>(
    () => `/api/games/location-challenges/${this.id()}/progress`,
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

  // The start and the discovered points; the server never sends where an undiscovered point is.
  protected readonly pins = computed<MapLocation[]>(() => {
    const progress = this.progress.hasValue() ? this.progress.value() : null;
    if (progress === null || progress.center === null) {
      return [];
    }
    const discovered: MapLocation[] = [];
    for (const point of progress.hiddenPoints ?? []) {
      if (point.location !== null) {
        discovered.push({ ...point.location, name: `Point ${point.number}`, label: `${point.number}` });
      }
    }
    return [{ ...progress.center, name: 'Start', label: 'S' }, ...discovered];
  });

  protected readonly discoveredCount = computed(() => {
    const progress = this.progress.hasValue() ? this.progress.value() : null;
    return (progress?.hiddenPoints ?? []).filter((point) => point.location !== null).length;
  });

  protected readonly saving = signal(false);
  protected readonly saved = signal(false);
  protected readonly failed = signal(false);

  protected async save(location: MapLocation): Promise<void> {
    // One request at a time, so a click while saving is ignored.
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
}
