import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import {
  FollowedUserDto,
  FollowerDto,
  ReceivedFollowRequestDto,
  SentFollowRequestDto,
} from '../../api/social-api-types';
import { FollowerManaging } from '../follower-managing';

const pageSize = 20;

type ConnectionTab = 'followers' | 'following' | 'requests';

@Component({
  imports: [RouterLink],
  selector: 'app-follower-list',
  styleUrl: './follower-list.scss',
  templateUrl: './follower-list.html',
})
export class FollowerList {
  private readonly followerManaging = inject(FollowerManaging);

  protected readonly activeTab = signal<ConnectionTab>('followers');
  protected readonly followersPage = signal(1);
  protected readonly followingPage = signal(1);

  protected readonly followers = httpResource<PageResult<FollowerDto>>(
    () => `/api/social/follow-requests/followers?page=${this.followersPage()}&pageSize=${pageSize}`,
  );

  protected readonly following = httpResource<PageResult<FollowedUserDto>>(
    () => `/api/social/follow-requests/following?page=${this.followingPage()}&pageSize=${pageSize}`,
  );

  protected readonly receivedRequests = httpResource<ReceivedFollowRequestDto[]>(
    () => '/api/social/follow-requests/received',
  );

  protected readonly sentRequests = httpResource<SentFollowRequestDto[]>(
    () => '/api/social/follow-requests/sent',
  );

  protected readonly followersCount = computed(() => this.followers.value()?.totalCount ?? 0);
  protected readonly followingCount = computed(() => this.following.value()?.totalCount ?? 0);
  protected readonly receivedCount = computed(() => this.receivedRequests.value()?.length ?? 0);

  protected readonly hasNextFollowersPage = computed(
    () => this.followersPage() * pageSize < this.followersCount(),
  );

  protected readonly hasNextFollowingPage = computed(
    () => this.followingPage() * pageSize < this.followingCount(),
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected formatDate(value: string): string {
    return new Date(value).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  }

  protected selectTab(tab: ConnectionTab): void {
    this.activeTab.set(tab);
  }

  protected previousFollowersPage(): void {
    this.followersPage.update((page) => page - 1);
  }

  protected nextFollowersPage(): void {
    this.followersPage.update((page) => page + 1);
  }

  protected previousFollowingPage(): void {
    this.followingPage.update((page) => page - 1);
  }

  protected nextFollowingPage(): void {
    this.followingPage.update((page) => page + 1);
  }

  protected async followBack(userId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followerManaging.sendFollowRequest({ receiverId: userId });
      this.reloadAll();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not send the follow request.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async cancelRequest(requestId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followerManaging.withdrawFollowRequest(requestId);
      this.reloadAll();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not cancel the follow request.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async accept(requestId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followerManaging.acceptFollowRequest(requestId);
      this.reloadAll();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not accept the follow request.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async decline(requestId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followerManaging.rejectFollowRequest(requestId);
      this.reloadAll();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not decline the follow request.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async removeFollower(followRequestId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followerManaging.removeFollower(followRequestId);
      // The removed row was the last one on a page after the first: show the previous page.
      if (this.followers.value()?.items.length === 1 && this.followersPage() > 1) {
        this.previousFollowersPage();
      }
      this.reloadAll();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not remove the follower.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async unfollow(followRequestId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followerManaging.unfollow(followRequestId);
      // The unfollowed row was the last one on a page after the first: show the previous page.
      if (this.following.value()?.items.length === 1 && this.followingPage() > 1) {
        this.previousFollowingPage();
      }
      this.reloadAll();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not unfollow the user.'));
    } finally {
      this.pending.set(false);
    }
  }

  private reloadAll(): void {
    this.followers.reload();
    this.following.reload();
    this.receivedRequests.reload();
    this.sentRequests.reload();
  }
}
