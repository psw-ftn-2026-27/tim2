import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, max, min, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { CouponDto, CouponProductDto } from '../../api/payment-api-types';
import { CouponManagement } from '../coupon-management';

const EMPTY_COUPON = { productId: '', percent: 10, expiresOn: '' };

// A coupon must expire after today, so the date picker starts from tomorrow (yyyy-MM-dd).
function tomorrow(): string {
  const date = new Date();
  date.setDate(date.getDate() + 1);
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-coupons',
  styleUrl: './coupons.scss',
  templateUrl: './coupons.html',
})
export class Coupons {
  private readonly couponManagement = inject(CouponManagement);

  protected readonly coupons = httpResource<CouponDto[]>(() => '/api/payment/coupons');
  protected readonly products = httpResource<CouponProductDto[]>(
    () => '/api/payment/coupons/products',
  );

  protected readonly form = form(signal({ ...EMPTY_COUPON }), (path) => {
    required(path.percent, { message: 'Percent is required.' });
    min(path.percent, 1, { message: 'Percent must be at least 1.' });
    max(path.percent, 100, { message: 'Percent can be at most 100.' });
    required(path.expiresOn, { message: 'Expiry date is required.' });
  });

  protected readonly minDate = tomorrow();

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected formatDate(date: string): string {
    const [year, month, day] = date.split('-').map(Number);
    return new Date(year, month - 1, day).toLocaleDateString('en-GB');
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    const productId = this.form.productId().value();
    try {
      await this.couponManagement.create({
        productId: productId === '' ? null : productId,
        percent: this.form.percent().value(),
        expiresOn: this.form.expiresOn().value(),
      });
      this.form().reset({ ...EMPTY_COUPON });
      this.coupons.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the coupon.'));
    } finally {
      this.pending.set(false);
    }
  }
}
