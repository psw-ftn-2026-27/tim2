import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, max, maxLength, min, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { WalletBalanceDto } from '../../api/payment-api-types';
import { GiftCardGifting } from '../gift-card-gifting';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-buy-gift-card',
  styleUrl: './buy-gift-card.scss',
  templateUrl: './buy-gift-card.html',
})
export class BuyGiftCard {
  private readonly giftCardGifting = inject(GiftCardGifting);
  private readonly router = inject(Router);

  protected readonly balance = httpResource<WalletBalanceDto>(() => '/api/payment/wallet/balance');

  protected readonly form = form(
    signal({ recipientEmail: '', amount: 10, message: '' }),
    (path) => {
      required(path.recipientEmail, { message: 'Recipient email is required.' });
      required(path.amount, { message: 'Amount is required.' });
      min(path.amount, 10, { message: 'Amount must be at least 10 EC.' });
      max(path.amount, 1000, { message: 'Amount can be at most 1000 EC.' });
      maxLength(path.message, 200, { message: 'Message can have at most 200 characters.' });
    },
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    const message = this.form.message().value();
    try {
      await this.giftCardGifting.purchase({
        recipientEmail: this.form.recipientEmail().value(),
        amount: this.form.amount().value(),
        message: message === '' ? null : message,
      });
      await this.router.navigate(['/payment/gift-cards']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not buy the gift card.'));
    } finally {
      this.pending.set(false);
    }
  }
}
