import { Component, computed, input, output, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';

@Component({
  imports: [FormField],
  selector: 'app-club-image-list',
  styleUrl: './club-image-list.scss',
  templateUrl: './club-image-list.html',
})
export class ClubImageList {
  readonly images = input.required<string[]>();

  readonly addImage = output<string>();
  readonly removeImage = output<number>();

  protected readonly form = form(signal({ address: '' }));

  protected readonly canAdd = computed(() => this.form.address().value().trim() !== '');

  protected add(): void {
    this.addImage.emit(this.form.address().value());
    this.form().reset({ address: '' });
  }
}
