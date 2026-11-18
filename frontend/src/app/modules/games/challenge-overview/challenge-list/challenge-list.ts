import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { ChallengeDto, ChallengeType } from '../../api/games-api-types';
import { ChallengeOverview } from '../challenge-overview';

const pageSize = 20;

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-challenge-list',
  styleUrl: './challenge-list.scss',
  templateUrl: './challenge-list.html',
})
export class ChallengeList {
  private readonly overview = inject(ChallengeOverview);

  private readonly filter = signal({ type: 'All' as ChallengeType | 'All' });

  protected readonly filterForm = form(this.filter);

  protected readonly page = signal(1);

  protected readonly challenges = httpResource<PageResult<ChallengeDto>>(() => {
    const type = this.filter().type;
    const typeParam = type === 'All' ? '' : `&type=${type}`;
    return `/api/games/challenges?page=${this.page()}&pageSize=${pageSize}${typeParam}`;
  });

  protected readonly hasNextPage = computed(
    () => this.page() * pageSize < (this.challenges.value()?.totalCount ?? 0),
  );

  protected readonly confirmingId = signal<string | null>(null);
  protected readonly confirmingAction = signal<'delete' | 'publish' | 'retire' | 'archive' | 'repeat'>(
    'delete',
  );
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected askToConfirm(
    challengeId: string,
    action: 'delete' | 'publish' | 'retire' | 'archive' | 'repeat',
  ): void {
    this.error.set(null);
    this.confirmingAction.set(action);
    this.confirmingId.set(challengeId);
  }

  protected cancelConfirm(): void {
    this.confirmingId.set(null);
  }

  protected async confirm(challenge: ChallengeDto): Promise<void> {
    const challengeId = challenge.id;
    this.pending.set(true);
    this.error.set(null);
    const action = this.confirmingAction();
    try {
      if (challenge.type === 'Quiz') {
        await this.confirmQuiz(challengeId, action);
      } else if (challenge.type === 'LocationChallenge') {
        await this.confirmLocationChallenge(challengeId, action);
      } else if (action === 'delete') {
        await this.overview.delete(challengeId);
      } else if (action === 'publish') {
        await this.overview.publish(challengeId);
      } else if (action === 'retire') {
        await this.overview.retire(challengeId);
      } else if (action === 'archive') {
        await this.overview.archive(challengeId);
      }
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, `Could not ${action} the challenge.`));
      if (challenge.type === 'Quiz' || challenge.type === 'LocationChallenge') {
        this.challenges.reload();
      }
    } finally {
      this.confirmingId.set(null);
      this.pending.set(false);
    }
  }

  private async confirmQuiz(
    quizId: string,
    action: 'delete' | 'publish' | 'retire' | 'archive' | 'repeat',
  ): Promise<void> {
    if (action === 'delete') {
      await this.overview.deleteQuiz(quizId);
    } else if (action === 'publish') {
      await this.overview.publishQuiz(quizId);
    } else if (action === 'retire') {
      await this.overview.retireQuiz(quizId);
    } else if (action === 'archive') {
      await this.overview.archiveQuiz(quizId);
    } else {
      await this.overview.repeatQuiz(quizId);
    }
  }

  private async confirmLocationChallenge(
    challengeId: string,
    action: 'delete' | 'publish' | 'retire' | 'archive' | 'repeat',
  ): Promise<void> {
    if (action === 'delete') {
      await this.overview.deleteLocationChallenge(challengeId);
    } else if (action === 'publish') {
      await this.overview.publishLocationChallenge(challengeId);
    } else if (action === 'retire') {
      await this.overview.retireLocationChallenge(challengeId);
    } else if (action === 'archive') {
      await this.overview.archiveLocationChallenge(challengeId);
    } else {
      await this.overview.repeatLocationChallenge(challengeId);
    }
  }

  protected async repeat(challengeId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.overview.repeat(challengeId);
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not repeat the challenge.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected backToFirstPage(): void {
    this.page.set(1);
  }

  protected previousPage(): void {
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.page.update((page) => page + 1);
  }
}
