import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/social/follow-requests';

@Injectable({ providedIn: 'root' })
export class FollowRequestSending {
  private readonly http = inject(HttpClient);

  async withdraw(id: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}`));
  }
}
