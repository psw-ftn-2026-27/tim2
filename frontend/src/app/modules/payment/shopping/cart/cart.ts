import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { CartDto } from '../../api/payment-api-types';
import { Shopping } from '../shopping';

@Component({
  imports: [RouterLink],
  selector: 'app-cart',
  styleUrl: './cart.scss',
  templateUrl: './cart.html',
})
export class Cart {
  private readonly shopping = inject(Shopping);

  protected readonly cart = httpResource<CartDto>(() => '/api/payment/shopping/cart');

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async removeFromCart(tourId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.shopping.removeFromCart(tourId);
      this.cart.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not remove the tour from the cart.'));
    } finally {
      this.pending.set(false);
    }
  }
}
