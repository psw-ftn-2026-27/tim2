import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDetailsDto } from '../../api/exploration-api-types';
import { TourBrowsing } from '../tour-browsing';

@Component({
  imports: [RouterLink],
  selector: 'app-tour-details',
  styleUrl: './tour-details.scss',
  templateUrl: './tour-details.html',
})
export class TourDetails {
  private readonly tourBrowsing = inject(TourBrowsing);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly tour = httpResource<TourDetailsDto>(
    () => `/api/exploration/tours/${this.id()}`,
  );

  protected readonly startError = signal<string | null>(null);

  // Starting a tour that is already active resumes it, so the same click leads to the same page.
  protected async start(): Promise<void> {
    this.startError.set(null);
    try {
      await this.tourBrowsing.startTour(this.id());
      await this.router.navigate(['/exploration/tours', this.id(), 'run']);
    } catch (failure) {
      this.startError.set(serverMessage(failure, 'Could not start the tour.'));
    }
  }
}
