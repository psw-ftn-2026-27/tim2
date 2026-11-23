import { Component, input } from '@angular/core';
import { GiftCardDto } from '../../api/payment-api-types';

@Component({
  selector: 'app-gift-card-list',
  styleUrl: './gift-card-list.scss',
  templateUrl: './gift-card-list.html',
})
export class GiftCardList {
  readonly cards = input.required<GiftCardDto[]>();
  readonly otherPartyLabel = input.required<string>();
  readonly emptyMessage = input.required<string>();

  protected formatDate(value: string): string {
    return new Date(value).toLocaleDateString('en-GB');
  }
}
