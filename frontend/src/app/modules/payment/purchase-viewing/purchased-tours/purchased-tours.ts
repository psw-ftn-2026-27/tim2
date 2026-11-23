import { httpResource } from '@angular/common/http';
import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { PurchasedTourDto } from '../../api/payment-api-types';

const pageSize = 20;

@Component({
  imports: [RouterLink],
  selector: 'app-purchased-tours',
  styleUrl: './purchased-tours.scss',
  templateUrl: './purchased-tours.html',
})
export class PurchasedTours {
  protected readonly page = signal(1);

  protected readonly tours = httpResource<PageResult<PurchasedTourDto>>(
    () => `/api/payment/purchases/tours?page=${this.page()}&pageSize=${pageSize}`,
  );

  protected readonly hasNextPage = computed(
    () => this.page() * pageSize < (this.tours.value()?.totalCount ?? 0),
  );

  protected formatTime(value: string): string {
    return new Date(value).toLocaleString('en-GB');
  }

  protected previousPage(): void {
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.page.update((page) => page + 1);
  }
}
