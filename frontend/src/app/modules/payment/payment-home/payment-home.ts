import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../core/auth/auth';

@Component({
  imports: [RouterLink],
  selector: 'app-payment-home',
  template: `
    <div class="stack">
      <h1>Payment</h1>
      @if (isAdministrator()) {
        <p><a routerLink="/payment/wallets">User wallets</a></p>
      }
      @if (isExplorer()) {
        <p><a routerLink="/payment/wallet">My wallet</a></p>
        <p><a routerLink="/payment/cart">My cart</a></p>
        <p><a routerLink="/payment/purchases">My purchases</a></p>
        <p><a routerLink="/payment/gift-cards">Gift cards</a></p>
        <p><a routerLink="/payment/coupons">Coupons</a></p>
      }
    </div>
  `,
})
export class PaymentHome {
  private readonly auth = inject(Auth);

  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );

  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );
}
