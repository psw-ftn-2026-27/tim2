import { Component, inject, signal } from '@angular/core';
import { ImageUpload } from '../../../shared/image-upload/image-upload';
import { serverMessage } from '../../../shared/util/server-message';
import { Images } from '../images';

@Component({
  imports: [ImageUpload],
  selector: 'app-image-upload-demo',
  styleUrl: './image-upload-demo.scss',
  templateUrl: './image-upload-demo.html',
})
export class ImageUploadDemo {
  private readonly images = inject(Images);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly uploadedUrl = signal<string | null>(null);

  protected async upload(file: File): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      const uploaded = await this.images.upload(file);
      this.uploadedUrl.set(uploaded.url);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Otpremanje nije uspelo. Pokušajte ponovo.'));
    } finally {
      this.pending.set(false);
    }
  }
}
