import { Component, input, output, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';

@Component({
  imports: [FormField],
  selector: 'app-deposit-form',
  styleUrl: './deposit-form.scss',
  templateUrl: './deposit-form.html',
})
export class DepositForm {
  readonly disabled = input(false);
  readonly deposit = output<number>();

  protected readonly form = form(signal({ amount: null as number | null }), (path) => {
    required(path.amount);
  });

  protected submit(event: Event): void {
    event.preventDefault();
    const amount = this.form.amount().value();
    if (amount !== null) {
      this.deposit.emit(amount);
    }
  }
}
