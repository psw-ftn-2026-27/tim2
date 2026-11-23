import { httpResource } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { UserPositionDto } from '../../../../shared/api/user-position';
import { MapLocation, MapView } from '../../../../shared/map/map';
import { Positions } from '../../../../shared/positions/positions';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeProgressDto } from '../../api/games-api-types';
import { SocialChallengeSolving } from '../social-challenge-solving';

@Component({
  imports: [MapView, RouterLink],
  selector: 'app-solve-social-challenge',
  styleUrl: './solve-social-challenge.scss',
  templateUrl: './solve-social-challenge.html',
})
export class SolveSocialChallenge {
  private readonly positions = inject(Positions);
  private readonly solving = inject(SocialChallengeSolving);
  private readonly destroyRef = inject(DestroyRef);

  readonly id = input.required<string>();

  // The server shows the run as the last check left it, so the page loads it again after every check.
  protected readonly progress = httpResource<SocialChallengeProgressDto>(
    () => `/api/games/challenges/${this.id()}/progress`,
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

  protected readonly challengePins = computed<MapLocation[]>(() => {
    const progress = this.progress.hasValue() ? this.progress.value() : null;
    if (progress === null || progress.location === null) {
      return [];
    }
    return [
      { latitude: progress.location.latitude, longitude: progress.location.longitude, name: progress.name },
    ];
  });

  protected readonly saving = signal(false);
  protected readonly saved = signal(false);
  protected readonly failed = signal(false);

  protected readonly inRun = computed(
    () => this.progress.hasValue() && this.progress.value().state === 'InRun',
  );

  protected readonly checking = signal(false);
  protected readonly error = signal<string | null>(null);

  constructor() {
    const timerId = setInterval(() => this.check(), 10_000);
    // Without this the timer keeps sending checks after the player has left the page.
    this.destroyRef.onDestroy(() => clearInterval(timerId));
  }

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

  // Called by the timer every 10 s. It sends nothing while a check is running or the player is not in an active run,
  // so a finished run sends no more checks. A rejected check reloads the progress too, so the page shows the new state.
  protected async check(): Promise<void> {
    if (this.checking() || !this.inRun()) {
      return;
    }
    this.checking.set(true);
    try {
      await this.solving.check(this.id());
      this.error.set(null);
    } catch (failure) {
      // Shown above the counter; the timer keeps going and the next successful check clears it.
      this.error.set(serverMessage(failure, 'Could not check the run.'));
    } finally {
      this.progress.reload();
      this.checking.set(false);
    }
  }

  protected formatTime(value: string): string {
    return new Date(value).toLocaleString('en-GB');
  }
}
