import { httpResource } from '@angular/common/http';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { GiftCardOverviewDto, WalletBalanceDto } from '../../api/payment-api-types';
import { GiftCardList } from '../gift-card-list/gift-card-list';

@Component({
  imports: [GiftCardList, RouterLink],
  selector: 'app-gift-cards',
  styleUrl: './gift-cards.scss',
  templateUrl: './gift-cards.html',
})
export class GiftCards {
  protected readonly balance = httpResource<WalletBalanceDto>(() => '/api/payment/wallet/balance');

  protected readonly overview = httpResource<GiftCardOverviewDto>(() => '/api/payment/gift-cards');
}
