import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { PurchaseGiftCardDto } from '../api/payment-api-types';

const BASE_URL = '/api/payment/gift-cards';

@Injectable({ providedIn: 'root' })
export class GiftCardGifting {
  private readonly http = inject(HttpClient);

  async purchase(dto: PurchaseGiftCardDto): Promise<string> {
    return await firstValueFrom(this.http.post<string>(BASE_URL, dto));
  }
}
