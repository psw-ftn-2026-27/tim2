import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, max, maxLength, min, minLength, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeDto } from '../../api/games-api-types';
import { SocialChallengeManagement } from '../social-challenge-management';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-social-challenge-list',
  styleUrl: './social-challenge-list.scss',
  templateUrl: './social-challenge-list.html',
})
export class SocialChallengeList {
  private readonly management = inject(SocialChallengeManagement);

  protected readonly challenges = httpResource<PageResult<SocialChallengeDto>>(
    () => '/api/games/challenges?page=1&pageSize=20',
  );

  protected readonly confirmingId = signal<string | null>(null);
  protected readonly confirmingAction = signal<'delete' | 'publish' | 'retire' | 'archive'>(
    'delete',
  );
  protected readonly editingId = signal<string | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editError = signal<string | null>(null);

  protected readonly editForm = form(
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

  protected startEdit(challenge: SocialChallengeDto): void {
    this.error.set(null);
    this.editError.set(null);
    this.confirmingId.set(null);
    const [hours, minutes] = challenge.duration.split(':').map(Number);
    this.editForm().reset({
      name: challenge.name,
      start: challenge.start.slice(0, 5),
      durationMinutes: hours * 60 + minutes,
      requiredParticipants: challenge.requiredParticipants,
      rangeInMeters: challenge.rangeInMeters,
      xp: challenge.xp,
      completionMessage: challenge.completionMessage,
    });
    this.editingId.set(challenge.id);
  }

  protected cancelEdit(): void {
    this.editingId.set(null);
  }

  protected async saveEdit(event: Event, challengeId: string): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.editError.set(null);
    const minutes = this.editForm.durationMinutes().value();
    const hours = String(Math.floor(minutes / 60)).padStart(2, '0');
    const restMinutes = String(minutes % 60).padStart(2, '0');
    try {
      await this.management.update(challengeId, {
        name: this.editForm.name().value(),
        start: `${this.editForm.start().value()}:00`,
        duration: `${hours}:${restMinutes}:00`,
        requiredParticipants: this.editForm.requiredParticipants().value(),
        rangeInMeters: this.editForm.rangeInMeters().value(),
        xp: this.editForm.xp().value(),
        completionMessage: this.editForm.completionMessage().value(),
      });
      this.editingId.set(null);
      this.challenges.reload();
    } catch (failure) {
      this.editError.set(serverMessage(failure, 'Could not update the challenge.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected askToConfirm(
    challengeId: string,
    action: 'delete' | 'publish' | 'retire' | 'archive',
  ): void {
    this.error.set(null);
    this.editingId.set(null);
    this.confirmingAction.set(action);
    this.confirmingId.set(challengeId);
  }

  protected cancelConfirm(): void {
    this.confirmingId.set(null);
  }

  protected async confirm(challengeId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    const action = this.confirmingAction();
    try {
      if (action === 'delete') {
        await this.management.delete(challengeId);
      } else if (action === 'publish') {
        await this.management.publish(challengeId);
      } else if (action === 'retire') {
        await this.management.retire(challengeId);
      } else {
        await this.management.archive(challengeId);
      }
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, `Could not ${action} the challenge.`));
    } finally {
      this.confirmingId.set(null);
      this.pending.set(false);
    }
  }

  protected async repeat(challengeId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.management.repeat(challengeId);
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not repeat the challenge.'));
    } finally {
      this.pending.set(false);
    }
  }
}
