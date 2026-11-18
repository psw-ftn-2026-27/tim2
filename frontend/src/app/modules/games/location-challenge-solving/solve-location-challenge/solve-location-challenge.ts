import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { UserPositionDto } from '../../../../shared/api/user-position';
import { MapLocation, MapView } from '../../../../shared/map/map';
import { Positions } from '../../../../shared/positions/positions';
import { serverMessage } from '../../../../shared/util/server-message';
import { LocationChallengeProgressDto } from '../../api/games-api-types';
import { LocationChallengeSolving } from '../location-challenge-solving';

@Component({
  imports: [MapView, RouterLink],
  selector: 'app-solve-location-challenge',
  styleUrl: './solve-location-challenge.scss',
  templateUrl: './solve-location-challenge.html',
})
export class SolveLocationChallenge {
  private readonly positions = inject(Positions);
  private readonly solving = inject(LocationChallengeSolving);

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

  // Only the numbers of the discovered points, because the server sends a location for them alone.
  private readonly discoveredNumbers = computed<number[]>(() => {
    const progress = this.progress.hasValue() ? this.progress.value() : null;
    return (progress?.hiddenPoints ?? [])
      .filter((point) => point.location !== null)
      .map((point) => point.number);
  });

  protected readonly discoveredCount = computed(() => this.discoveredNumbers().length);

  protected readonly checking = signal(false);
  protected readonly error = signal<string | null>(null);

  // The numbers discovered before the last accepted check; null when there is no check result to show.
  private readonly discoveredBefore = signal<number[] | null>(null);

  // The check returns nothing, so the page compares the discovered points before the check with those
  // after the reload. While the progress is still reloading there is nothing to compare yet.
  protected readonly checkResult = computed<string | null>(() => {
    const before = this.discoveredBefore();
    if (before === null || this.checking() || this.progress.isLoading()) {
      return null;
    }
    const found = this.discoveredNumbers().filter((number) => !before.includes(number));
    if (found.length === 0) {
      return 'You are not on a hidden spot';
    }
    if (found.length === 1) {
      return `You found spot ${found[0]}`;
    }
    return `You found spots ${found.join(', ')}`;
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

  protected async check(): Promise<void> {
    // One check at a time, so a click while checking is ignored.
    if (this.checking()) {
      return;
    }
    const before = this.discoveredNumbers();
    this.checking.set(true);
    this.error.set(null);
    this.discoveredBefore.set(null);
    try {
      // A check that finds no point also succeeds; what it found shows up in the reloaded progress.
      await this.solving.check(this.id());
      this.discoveredBefore.set(before);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not check the spot.'));
    } finally {
      // Reloaded after a rejection too, so a retired or solved challenge is shown at once.
      this.progress.reload();
      this.checking.set(false);
    }
  }

  protected formatTime(value: string): string {
    return new Date(value).toLocaleString('en-GB');
  }
}
