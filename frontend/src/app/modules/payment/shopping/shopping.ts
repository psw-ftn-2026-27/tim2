import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/payment/shopping/cart/items';
const PURCHASE_URL = '/api/payment/shopping/cart/purchase';

@Injectable({ providedIn: 'root' })
export class Shopping {
  private readonly http = inject(HttpClient);

  async addToCart(tourId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${tourId}`, null));
  }

  async removeFromCart(tourId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${tourId}`));
  }

  async purchase(): Promise<void> {
    await firstValueFrom(this.http.post<void>(PURCHASE_URL, null));
  }
}
