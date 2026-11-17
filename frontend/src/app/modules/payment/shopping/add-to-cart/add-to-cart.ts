import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourOfferDto } from '../../api/payment-api-types';
import { Shopping } from '../shopping';

@Component({
  imports: [RouterLink],
  selector: 'app-add-to-cart',
  styleUrl: './add-to-cart.scss',
  templateUrl: './add-to-cart.html',
})
export class AddToCart {
  private readonly shopping = inject(Shopping);
  private readonly router = inject(Router);

  readonly tourId = input.required<string>();

  protected readonly offer = httpResource<TourOfferDto>(
    () => `/api/payment/shopping/tours/${this.tourId()}`,
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async addToCart(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.shopping.addToCart(this.tourId());
      await this.router.navigate(['/payment/cart']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not add the tour to the cart.'));
    } finally {
      this.pending.set(false);
    }
  }
}
