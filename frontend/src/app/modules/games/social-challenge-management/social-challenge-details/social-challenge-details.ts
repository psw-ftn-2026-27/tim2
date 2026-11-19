import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { MapLocation, MapView } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeDto } from '../../api/games-api-types';
import { QuizLocationFields } from '../../quiz-management/quiz-location-fields/quiz-location-fields';
import {
  emptySocialChallenge,
  socialChallengeFormSchema,
  toSocialChallengeDto,
  toSocialChallengeFormModel,
} from '../social-challenge-form-schema';
import { SocialChallengeManagement } from '../social-challenge-management';

@Component({
  imports: [FormField, MapView, QuizLocationFields, RouterLink],
  selector: 'app-social-challenge-details',
  styleUrl: './social-challenge-details.scss',
  templateUrl: './social-challenge-details.html',
})
export class SocialChallengeDetails {
  private readonly management = inject(SocialChallengeManagement);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly challenge = httpResource<SocialChallengeDto>(
    () => `/api/games/challenges/${this.id()}`,
  );

  protected readonly savedLocation = computed<MapLocation[]>(() => {
    const location = this.challenge.value()?.location;
    return location ? [{ latitude: location.latitude, longitude: location.longitude }] : [];
  });

  protected readonly confirmingAction = signal<'delete' | 'publish' | 'retire' | 'archive' | 'repeat' | null>(
    null,
  );
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly editing = signal(false);
  protected readonly editError = signal<string | null>(null);

  protected readonly editForm = form(signal(emptySocialChallenge()), socialChallengeFormSchema);

  // The place picked on the map of the edit form; null while the challenge has no location.
  protected readonly editLocation = signal<MapLocation | null>(null);

  // Edit is shown only after the challenge has loaded, so the form is filled with that challenge.
  protected startEdit(challenge: SocialChallengeDto): void {
    this.error.set(null);
    this.editError.set(null);
    this.confirmingAction.set(null);
    this.editForm().reset(toSocialChallengeFormModel(challenge));
    this.editLocation.set(challenge.location);
    this.editing.set(true);
  }

  protected cancelEdit(): void {
    this.editing.set(false);
  }

  protected async saveEdit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.editError.set(null);
    try {
      await this.management.update(
        this.id(),
        toSocialChallengeDto(this.editForm().value(), this.editLocation()),
      );
      this.editing.set(false);
      this.challenge.reload();
    } catch (failure) {
      this.editError.set(serverMessage(failure, 'Could not update the challenge.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected askToConfirm(action: 'delete' | 'publish' | 'retire' | 'archive' | 'repeat'): void {
    this.error.set(null);
    this.confirmingAction.set(action);
  }

  protected cancelConfirm(): void {
    this.confirmingAction.set(null);
  }

  // After a rejection nothing reloads: the challenge stays as shown and the server message says why.
  protected async confirm(): Promise<void> {
    const action = this.confirmingAction();
    this.pending.set(true);
    this.error.set(null);
    try {
      if (action === 'delete') {
        await this.management.delete(this.id());
        await this.router.navigate(['/games/challenges']);
        return;
      }
      if (action === 'publish') {
        await this.management.publish(this.id());
      } else if (action === 'retire') {
        await this.management.retire(this.id());
      } else if (action === 'archive') {
        await this.management.archive(this.id());
      } else {
        await this.management.repeat(this.id());
      }
      this.challenge.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, `Could not ${action} the challenge.`));
    } finally {
      this.confirmingAction.set(null);
      this.pending.set(false);
    }
  }
}
