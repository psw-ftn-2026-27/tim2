import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CreateSocialChallengeDto, UpdateSocialChallengeDto } from '../api/games-api-types';

const BASE_URL = '/api/games/challenges';

@Injectable({ providedIn: 'root' })
export class SocialChallengeManagement {
  private readonly http = inject(HttpClient);

  async create(dto: CreateSocialChallengeDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(BASE_URL, dto));
  }

  async update(id: string, dto: UpdateSocialChallengeDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}`, dto));
  }

  async delete(id: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}`));
  }

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, null));
  }

  async retire(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/retire`, null));
  }

  async archive(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/archive`, null));
  }

  async repeat(id: string): Promise<string> {
    return firstValueFrom(this.http.post<string>(`${BASE_URL}/${id}/repeat`, null));
  }
}
