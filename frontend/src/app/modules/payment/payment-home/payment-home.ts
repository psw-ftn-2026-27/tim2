import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  imports: [RouterLink],
  selector: 'app-payment-home',
  template: `
    <div class="stack">
      <h1>Payment</h1>
      <p><a routerLink="/payment/wallet">Moj novčanik</a></p>
    </div>
  `,
})
export class PaymentHome {}
