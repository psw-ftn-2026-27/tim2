import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
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
  protected readonly purchased = signal(false);
  protected readonly purchaseError = signal<string | null>(null);

  protected readonly isEmpty = computed(() => (this.cart.value()?.items.length ?? 0) === 0);

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

  protected async purchase(): Promise<void> {
    this.pending.set(true);
    this.purchased.set(false);
    this.purchaseError.set(null);
    try {
      await this.shopping.purchase();
      this.purchased.set(true);
      this.cart.reload();
    } catch (failure) {
      this.purchaseError.set(serverMessage(failure, 'Could not complete the purchase.'));
    } finally {
      this.pending.set(false);
    }
  }
}
