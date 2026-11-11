import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { UserTransactionDto, UserWalletDto } from '../../api/payment-api-types';
import { WalletFunding } from '../wallet-funding';

const pageSize = 20;
const cancellationWindowMs = 24 * 60 * 60 * 1000;

@Component({
  imports: [RouterLink],
  selector: 'app-user-wallet',
  styleUrl: './user-wallet.scss',
  templateUrl: './user-wallet.html',
})
export class UserWallet {
  private readonly walletFunding = inject(WalletFunding);
  private readonly auth = inject(Auth);

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

  protected formatTime(value: string): string {
    return new Date(value).toLocaleString('sr-RS');
  }

  protected isOwnDeposit(transaction: UserTransactionDto): boolean {
    return transaction.adminId === this.auth.user()?.id;
  }

  protected isWithinCancellationWindow(transaction: UserTransactionDto): boolean {
    return Date.now() - new Date(transaction.createdAt).getTime() <= cancellationWindowMs;
  }

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
