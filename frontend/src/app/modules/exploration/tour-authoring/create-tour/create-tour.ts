import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, min, required } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDifficulty } from '../../api/exploration-api-types';
import { TourAuthoring } from '../tour-authoring';

@Component({
  imports: [FormField],
  selector: 'app-create-tour',
  styleUrl: './create-tour.scss',
  templateUrl: './create-tour.html',
})
export class CreateTour {
  private readonly tourAuthoring = inject(TourAuthoring);
  private readonly router = inject(Router);

  private readonly model = signal({
    name: '',
    description: '',
    difficulty: 'Easy' as TourDifficulty,
    tags: '',
    minPeople: null as number | null,
    maxPeople: null as number | null,
  });

  protected readonly form = form(this.model, (path) => {
    required(path.name, { message: 'Name is required.' });
    required(path.description, { message: 'Description is required.' });
    min(path.minPeople, 1, { message: 'Minimum must be at least 1.' });
    min(path.maxPeople, 1, { message: 'Maximum must be at least 1.' });
  });

  protected readonly onlyOneGroupSizeFilled = computed(
    () => (this.model().minPeople === null) !== (this.model().maxPeople === null),
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    const minPeople = this.form.minPeople().value();
    const maxPeople = this.form.maxPeople().value();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.create({
        name: this.form.name().value(),
        description: this.form.description().value(),
        difficulty: this.form.difficulty().value(),
        tags: this.form
          .tags()
          .value()
          .split(',')
          .map((tag) => tag.trim())
          .filter((tag) => tag !== ''),
        recommendedGroupSize:
          minPeople !== null && maxPeople !== null ? { min: minPeople, max: maxPeople } : null,
      });
      await this.router.navigate(['/exploration/mine']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the tour.'));
    } finally {
      this.pending.set(false);
    }
  }
}
