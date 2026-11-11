import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { BlogDto } from '../../api/social-api-types';
import { BlogAuthoring } from '../blog-authoring';
import { ImageList } from '../image-list/image-list';

@Component({
  imports: [FormField, ImageList, RouterLink],
  selector: 'app-my-blogs',
  styleUrl: './my-blogs.scss',
  templateUrl: './my-blogs.html',
})
export class MyBlogs {
  private readonly blogAuthoring = inject(BlogAuthoring);

  protected readonly blogs = httpResource<BlogDto[]>(() => '/api/social/blogs/mine');

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly editingBlogId = signal<string | null>(null);
  protected readonly editError = signal<string | null>(null);

  protected readonly editForm = form(
    signal({ title: '', description: '', images: [] as string[] }),
    (path) => {
      required(path.title, { message: 'Title is required.' });
      required(path.description, { message: 'Description is required.' });
    },
  );

  protected async publish(blogId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.blogAuthoring.publish(blogId);
      this.blogs.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not publish the blog.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected formatDate(value: string): string {
    return new Date(value).toLocaleString('sr-RS');
  }

  protected startEdit(blog: BlogDto): void {
    this.editingBlogId.set(blog.id);
    this.editError.set(null);
    this.editForm().reset({
      title: blog.title,
      description: blog.description,
      images: [...blog.images],
    });
  }

  protected cancelEdit(): void {
    this.editingBlogId.set(null);
    this.editError.set(null);
  }

  protected addImage(address: string): void {
    this.editForm.images().value.update((images) => [...images, address]);
  }

  protected removeImage(index: number): void {
    this.editForm.images().value.update((images) => images.filter((_, i) => i !== index));
  }

  protected async saveEdit(event: Event, blogId: string): Promise<void> {
    event.preventDefault();
    if (this.editForm.images().value().length === 0) {
      this.editError.set('Add at least one image.');
      return;
    }
    this.pending.set(true);
    this.editError.set(null);
    try {
      await this.blogAuthoring.edit(blogId, {
        title: this.editForm.title().value(),
        description: this.editForm.description().value(),
        images: this.editForm.images().value(),
      });
      this.editingBlogId.set(null);
      this.blogs.reload();
    } catch (failure) {
      this.editError.set(serverMessage(failure, 'Could not save the blog.'));
    } finally {
      this.pending.set(false);
    }
  }
}
