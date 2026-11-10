import { Component, inject, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { BlogAuthoring } from '../blog-authoring';
import { ImageList } from '../image-list/image-list';

@Component({
  imports: [FormField, ImageList],
  selector: 'app-create-blog',
  styleUrl: './create-blog.scss',
  templateUrl: './create-blog.html',
})
export class CreateBlog {
  private readonly blogAuthoring = inject(BlogAuthoring);
  private readonly router = inject(Router);

  protected readonly form = form(
    signal({ title: '', description: '', images: [] as string[] }),
    (path) => {
      required(path.title, { message: 'Title is required.' });
      required(path.description, { message: 'Description is required.' });
    },
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected addImage(address: string): void {
    this.form.images().value.update((images) => [...images, address]);
  }

  protected removeImage(index: number): void {
    this.form.images().value.update((images) => images.filter((_, i) => i !== index));
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.blogAuthoring.create({
        title: this.form.title().value(),
        description: this.form.description().value(),
        images: this.form.images().value(),
      });
      await this.router.navigate(['/social/mine']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the blog.'));
    } finally {
      this.pending.set(false);
    }
  }
}
