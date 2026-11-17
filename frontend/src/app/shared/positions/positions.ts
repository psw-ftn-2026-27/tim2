import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { UserPositionDto } from '../api/user-position';

@Injectable({ providedIn: 'root' })
export class Positions {
  private readonly http = inject(HttpClient);

  async save(position: UserPositionDto): Promise<void> {
    await firstValueFrom(this.http.put<void>('/api/positions/mine', position));
  }
}
