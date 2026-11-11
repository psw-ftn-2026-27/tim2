import { Component, inject, signal } from '@angular/core';
import { FormField, form, max, maxLength, min, minLength, required } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeManagement } from '../social-challenge-management';

@Component({
  imports: [FormField],
  selector: 'app-create-social-challenge',
  styleUrl: './create-social-challenge.scss',
  templateUrl: './create-social-challenge.html',
})
export class CreateSocialChallenge {
  private readonly management = inject(SocialChallengeManagement);
  private readonly router = inject(Router);

  protected readonly form = form(
    signal({
      name: '',
      start: '',
      durationMinutes: 60,
      requiredParticipants: 2,
      rangeInMeters: 100,
      xp: 10,
      completionMessage: '',
    }),
    (path) => {
      required(path.name, { message: 'Name is required.' });
      minLength(path.name, 3, { message: 'Name must have at least 3 characters.' });
      maxLength(path.name, 60, { message: 'Name can have at most 60 characters.' });
      required(path.start, { message: 'Start time is required.' });
      required(path.durationMinutes, { message: 'Duration is required.' });
      min(path.durationMinutes, 15, { message: 'Duration must be at least 15 minutes.' });
      max(path.durationMinutes, 1439, { message: 'Duration must be shorter than 24 hours.' });
      required(path.requiredParticipants, { message: 'Number of people is required.' });
      min(path.requiredParticipants, 2, { message: 'At least 2 people are required.' });
      max(path.requiredParticipants, 100, { message: 'At most 100 people can be required.' });
      required(path.rangeInMeters, { message: 'Range is required.' });
      min(path.rangeInMeters, 10, { message: 'Range must be at least 10 meters.' });
      max(path.rangeInMeters, 1000, { message: 'Range can be at most 1000 meters.' });
      required(path.xp, { message: 'XP is required.' });
      min(path.xp, 1, { message: 'XP must be at least 1.' });
      max(path.xp, 1000, { message: 'XP can be at most 1000.' });
      required(path.completionMessage, { message: 'Completion message is required.' });
      maxLength(path.completionMessage, 200, {
        message: 'Completion message can have at most 200 characters.',
      });
    },
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    const minutes = this.form.durationMinutes().value();
    const hours = String(Math.floor(minutes / 60)).padStart(2, '0');
    const restMinutes = String(minutes % 60).padStart(2, '0');
    try {
      await this.management.create({
        name: this.form.name().value(),
        start: `${this.form.start().value()}:00`,
        duration: `${hours}:${restMinutes}:00`,
        requiredParticipants: this.form.requiredParticipants().value(),
        rangeInMeters: this.form.rangeInMeters().value(),
        xp: this.form.xp().value(),
        completionMessage: this.form.completionMessage().value(),
      });
      await this.router.navigate(['/games/challenges']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the challenge.'));
    } finally {
      this.pending.set(false);
    }
  }
}
