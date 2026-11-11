import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { UserTransactionDto, UserWalletDto } from '../../api/payment-api-types';
import { WalletFunding } from '../wallet-funding';

const pageSize = 20;

@Component({
  imports: [RouterLink],
  selector: 'app-user-wallet',
  styleUrl: './user-wallet.scss',
  templateUrl: './user-wallet.html',
})
export class UserWallet {
  private readonly walletFunding = inject(WalletFunding);

  readonly userId = input.required<string>();

  protected readonly page = signal(1);

  protected readonly wallet = httpResource<UserWalletDto>(
    () => `/api/payment/wallet/users/${this.userId()}`,
  );

  protected readonly transactions = httpResource<PageResult<UserTransactionDto>>(
    () =>
      `/api/payment/wallet/users/${this.userId()}/transactions?page=${this.page()}&pageSize=${pageSize}`,
  );

  protected readonly hasNextPage = computed(
    () => this.page() * pageSize < (this.transactions.value()?.totalCount ?? 0),
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async cancelDeposit(transactionId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.walletFunding.cancelDeposit(this.userId(), transactionId);
      this.wallet.reload();
      this.transactions.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Storno nije uspeo.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected previousPage(): void {
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.page.update((page) => page + 1);
  }
}
