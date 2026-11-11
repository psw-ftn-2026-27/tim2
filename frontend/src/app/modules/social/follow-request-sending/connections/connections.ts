import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { SentFollowRequestDto } from '../../api/social-api-types';
import { FollowRequestSending } from '../follow-request-sending';

@Component({
  imports: [RouterLink],
  selector: 'app-connections',
  styleUrl: './connections.scss',
  templateUrl: './connections.html',
})
export class Connections {
  private readonly followRequestSending = inject(FollowRequestSending);

  protected readonly requests = httpResource<SentFollowRequestDto[]>(
    () => '/api/social/follow-requests/sent',
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async cancel(requestId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followRequestSending.withdraw(requestId);
      this.requests.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not cancel the follow request.'));
    } finally {
      this.pending.set(false);
    }
  }
}
