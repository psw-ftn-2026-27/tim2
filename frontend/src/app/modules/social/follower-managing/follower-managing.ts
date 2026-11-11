import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { SendFollowRequestDto } from '../api/social-api-types';

const FOLLOW_REQUESTS_URL = '/api/social/follow-requests';

@Injectable({ providedIn: 'root' })
export class FollowerManaging {
  private readonly http = inject(HttpClient);

  async sendFollowRequest(dto: SendFollowRequestDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(FOLLOW_REQUESTS_URL, dto));
  }

  async withdrawFollowRequest(requestId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${FOLLOW_REQUESTS_URL}/${requestId}`));
  }

  async acceptFollowRequest(requestId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${FOLLOW_REQUESTS_URL}/${requestId}/accept`, null));
  }

  async rejectFollowRequest(requestId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${FOLLOW_REQUESTS_URL}/${requestId}/reject`, null));
  }
}
