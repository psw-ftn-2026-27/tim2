import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubDto } from '../../api/social-api-types';
import { ClubAuthoring } from '../club-authoring';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-my-clubs',
  styleUrl: './my-clubs.scss',
  templateUrl: './my-clubs.html',
})
export class MyClubs {
  private readonly clubAuthoring = inject(ClubAuthoring);

  protected readonly clubs = httpResource<ClubDto[]>(() => '/api/social/clubs/mine');

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly deletingClubId = signal<string | null>(null);

  protected readonly editingClubId = signal<string | null>(null);
  protected readonly editError = signal<string | null>(null);

  protected readonly editForm = form(signal({ name: '', description: '', images: '' }), (path) => {
    required(path.name, { message: 'Name is required.' });
    required(path.description, { message: 'Description is required.' });
  });

  protected startDelete(clubId: string): void {
    this.deletingClubId.set(clubId);
  }

  protected cancelDelete(): void {
    this.deletingClubId.set(null);
  }

  protected async confirmDelete(clubId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubAuthoring.delete(clubId);
      this.deletingClubId.set(null);
      this.clubs.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the club.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected startEdit(club: ClubDto): void {
    this.editingClubId.set(club.id);
    this.editError.set(null);
    this.editForm().reset({
      name: club.name,
      description: club.description,
      images: club.images.join('\n'),
    });
  }

  protected cancelEdit(): void {
    this.editingClubId.set(null);
    this.editError.set(null);
  }

  protected async saveEdit(event: Event, clubId: string): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.editError.set(null);
    try {
      await this.clubAuthoring.update(clubId, {
        name: this.editForm.name().value(),
        description: this.editForm.description().value(),
        images: this.toImageList(this.editForm.images().value()),
      });
      this.editingClubId.set(null);
      this.clubs.reload();
    } catch (failure) {
      this.editError.set(serverMessage(failure, 'Could not save the club.'));
    } finally {
      this.pending.set(false);
    }
  }

  private toImageList(text: string): string[] {
    return text
      .split('\n')
      .map((line) => line.trim())
      .filter((line) => line !== '');
  }
}
