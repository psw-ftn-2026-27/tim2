import { Component, computed, input, output, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';

@Component({
  imports: [FormField],
  selector: 'app-image-list',
  styleUrl: './image-list.scss',
  templateUrl: './image-list.html',
})
export class ImageList {
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
