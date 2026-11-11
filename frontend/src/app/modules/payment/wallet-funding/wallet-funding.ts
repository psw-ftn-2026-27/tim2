import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { DepositDto } from '../api/payment-api-types';

const BASE_URL = '/api/payment/wallet/users';

@Injectable({ providedIn: 'root' })
export class WalletFunding {
  private readonly http = inject(HttpClient);

  async deposit(userId: string, dto: DepositDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${userId}/deposits`, dto));
  }
}
