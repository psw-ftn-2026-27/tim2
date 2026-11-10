import { httpResource } from '@angular/common/http';
import { Component, computed, signal } from '@angular/core';
import { PageResult } from '../../../../shared/api/page-result';
import { TransactionDto, WalletBalanceDto } from '../../api/payment-api-types';

const pageSize = 20;

@Component({
  selector: 'app-wallet',
  styleUrl: './wallet.scss',
  templateUrl: './wallet.html',
})
export class Wallet {
  protected readonly page = signal(1);

  protected readonly balance = httpResource<WalletBalanceDto>(() => '/api/payment/wallet/balance');

  protected readonly transactions = httpResource<PageResult<TransactionDto>>(
    () => `/api/payment/wallet/transactions?page=${this.page()}&pageSize=${pageSize}`,
  );

  protected readonly hasNextPage = computed(
    () => this.page() * pageSize < (this.transactions.value()?.totalCount ?? 0),
  );

  protected previousPage(): void {
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.page.update((page) => page + 1);
  }
}
