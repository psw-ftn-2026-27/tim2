import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { UserPositionDto } from '../../../../shared/api/user-position';
import { MapLocation, MapView } from '../../../../shared/map/map';
import { Positions } from '../../../../shared/positions/positions';
import { serverMessage } from '../../../../shared/util/server-message';
import { NearbyChallengeDto } from '../../api/games-api-types';
import { NearbyChallenges } from '../nearby-challenges';

@Component({
  imports: [MapView, RouterLink],
  selector: 'app-nearby-challenge-list',
  styleUrl: './nearby-challenge-list.scss',
  templateUrl: './nearby-challenge-list.html',
})
export class NearbyChallengeList {
  private readonly positions = inject(Positions);
  private readonly nearby = inject(NearbyChallenges);
  private readonly router = inject(Router);

  // The server answers 204 while the user has no position, which arrives here as null.
  protected readonly position = httpResource<UserPositionDto | null>(() => '/api/positions/mine');

  // The server sorts the list by distance from the saved position, and sends an empty list without one.
  protected readonly challenges = httpResource<NearbyChallengeDto[]>(() => '/api/games/nearby-challenges');

  // The pin shows the position the server keeps, so it moves only after the server confirms.
  protected readonly youAreHere = computed<MapLocation | null>(() => {
    const position = this.position.hasValue() ? this.position.value() : null;
    if (position === null) {
      return null;
    }
    return { latitude: position.latitude, longitude: position.longitude, name: 'You are here' };
  });

  protected readonly challengePins = computed<MapLocation[]>(() =>
    (this.challenges.hasValue() ? this.challenges.value() : []).map((challenge) => ({
      latitude: challenge.latitude,
      longitude: challenge.longitude,
      name: challenge.name,
    })),
  );

  protected readonly saving = signal(false);
  protected readonly saved = signal(false);
  protected readonly failed = signal(false);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

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
      // The distances depend on the position, so the list is loaded again.
      this.challenges.reload();
      this.saved.set(true);
    } catch {
      this.failed.set(true);
    } finally {
      this.saving.set(false);
    }
  }

  protected async activate(quizId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.nearby.activateQuiz(quizId);
      await this.router.navigate(['/games/play/quiz', quizId]);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not activate the quiz.'));
      this.challenges.reload();
    } finally {
      this.pending.set(false);
    }
  }

  protected async activateSocialChallenge(challengeId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.nearby.activateSocialChallenge(challengeId);
      await this.router.navigate(['/games/play/social', challengeId]);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not activate the social challenge.'));
      this.challenges.reload();
    } finally {
      this.pending.set(false);
    }
  }

  // The server sends the window start as "HH:mm:ss" local time at the challenge's place; the row shows hours and minutes.
  protected windowStartTime(windowStart: string): string {
    return windowStart.slice(0, 5);
  }
}
