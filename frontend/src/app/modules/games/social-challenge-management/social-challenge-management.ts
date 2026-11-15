import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CreateSocialChallengeDto } from '../api/games-api-types';

const BASE_URL = '/api/games/challenges';

@Injectable({ providedIn: 'root' })
export class SocialChallengeManagement {
  private readonly http = inject(HttpClient);

  async create(dto: CreateSocialChallengeDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(BASE_URL, dto));
  }
}
