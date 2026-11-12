import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { UserWalletDto } from '../../api/payment-api-types';
import { DepositForm } from '../deposit-form/deposit-form';
import { WalletFunding } from '../wallet-funding';

const pageSize = 20;

@Component({
  imports: [DepositForm, RouterLink],
  selector: 'app-user-wallets',
  styleUrl: './user-wallets.scss',
  templateUrl: './user-wallets.html',
})
export class UserWallets {
  private readonly walletFunding = inject(WalletFunding);

  protected readonly page = signal(1);

  protected readonly wallets = httpResource<PageResult<UserWalletDto>>(
    () => `/api/payment/wallet/users?page=${this.page()}&pageSize=${pageSize}`,
  );

  protected readonly hasNextPage = computed(
    () => this.page() * pageSize < (this.wallets.value()?.totalCount ?? 0),
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async deposit(userId: string, amount: number): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.walletFunding.deposit(userId, { amount });
      // While the list reloads it is replaced by the loading text, so every amount field
      // is created again, empty.
      this.wallets.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Uplata nije uspela.'));
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
