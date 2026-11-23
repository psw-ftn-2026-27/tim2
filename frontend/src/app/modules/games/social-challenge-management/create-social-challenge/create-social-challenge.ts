import { Component, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { MapLocation } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuizLocationFields } from '../../quiz-management/quiz-location-fields/quiz-location-fields';
import {
  emptySocialChallenge,
  socialChallengeFormSchema,
  toSocialChallengeDto,
} from '../social-challenge-form-schema';
import { SocialChallengeManagement } from '../social-challenge-management';

@Component({
  imports: [FormField, QuizLocationFields],
  selector: 'app-create-social-challenge',
  styleUrl: './create-social-challenge.scss',
  templateUrl: './create-social-challenge.html',
})
export class CreateSocialChallenge {
  private readonly management = inject(SocialChallengeManagement);
  private readonly router = inject(Router);

  protected readonly form = form(signal(emptySocialChallenge()), socialChallengeFormSchema);

  // The place picked on the map; null while the challenge has no location.
  protected readonly location = signal<MapLocation | null>(null);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.management.create(toSocialChallengeDto(this.form().value(), this.location()));
      await this.router.navigate(['/games/challenges']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the challenge.'));
    } finally {
      this.pending.set(false);
    }
  }
}
