import { Component, input, output, signal } from '@angular/core';

const MAX_SIZE = 15 * 1024 * 1024;
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

@Component({
  selector: 'app-image-upload',
  styleUrl: './image-upload.scss',
  templateUrl: './image-upload.html',
})
export class ImageUpload {
  readonly pending = input(false);
  readonly error = input<string | null>(null);
  readonly uploadedUrl = input<string | null>(null);

  readonly upload = output<File>();

  protected readonly file = signal<File | null>(null);
  protected readonly previewUrl = signal<string | null>(null);
  protected readonly problem = signal<string | null>(null);

  protected choose(event: Event): void {
    const target = event.target as HTMLInputElement;
    const chosen = target.files?.[0] ?? null;
    target.value = '';
    if (chosen === null) {
      return;
    }

    this.revokePreview();
    this.file.set(null);
    this.problem.set(null);

    if (!ALLOWED_TYPES.includes(chosen.type)) {
      this.problem.set('Dozvoljeni formati su JPEG, PNG i WebP.');
      return;
    }
    if (chosen.size > MAX_SIZE) {
      this.problem.set('Slika je veća od 15MB.');
      return;
    }

    this.file.set(chosen);
    this.previewUrl.set(URL.createObjectURL(chosen));
  }

  protected submit(): void {
    const chosen = this.file();
    if (chosen !== null) {
      this.upload.emit(chosen);
    }
  }

  // The browser keeps a loaded image on screen, so the address can be released.
  protected revokePreview(): void {
    const url = this.previewUrl();
    if (url !== null) {
      URL.revokeObjectURL(url);
    }
  }
}
