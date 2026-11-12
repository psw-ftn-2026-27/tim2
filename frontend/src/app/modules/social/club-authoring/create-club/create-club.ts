import { Component, inject, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubAuthoring } from '../club-authoring';
import { ClubImageList } from '../club-image-list/club-image-list';

@Component({
  imports: [ClubImageList, FormField, RouterLink],
  selector: 'app-create-club',
  styleUrl: './create-club.scss',
  templateUrl: './create-club.html',
})
export class CreateClub {
  private readonly clubAuthoring = inject(ClubAuthoring);
  private readonly router = inject(Router);

  protected readonly form = form(
    signal({ name: '', description: '', images: [] as string[] }),
    (path) => {
      required(path.name, { message: 'Name is required.' });
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
    if (this.form.images().value().length === 0) {
      this.error.set('Add at least one image.');
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubAuthoring.create({
        name: this.form.name().value(),
        description: this.form.description().value(),
        images: this.form.images().value(),
      });
      await this.router.navigate(['/social/clubs/mine']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the club.'));
    } finally {
      this.pending.set(false);
    }
  }
}
