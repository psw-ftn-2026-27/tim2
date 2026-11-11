import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, min, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDto, TransportMode } from '../../api/exploration-api-types';
import { TourAuthoring } from '../tour-authoring';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-my-tours',
  styleUrl: './my-tours.scss',
  templateUrl: './my-tours.html',
})
export class MyTours {
  private readonly tourAuthoring = inject(TourAuthoring);

  protected readonly tours = httpResource<TourDto[]>(() => '/api/exploration/tours/mine');

  protected readonly selectedTourId = signal<string | null>(null);
  protected readonly groupSizeTourId = signal<string | null>(null);
  protected readonly priceTourId = signal<string | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  private readonly model = signal({ transport: 'Walking' as TransportMode, minutes: 30 });

  protected readonly form = form(this.model, (path) => {
    required(path.minutes, { message: 'Minutes are required.' });
    min(path.minutes, 1, { message: 'Minutes must be at least 1.' });
  });

  private readonly groupSizeModel = signal({
    min: null as number | null,
    max: null as number | null,
  });

  protected readonly groupSizeForm = form(this.groupSizeModel, (path) => {
    required(path.min, { message: 'Minimum is required.' });
    min(path.min, 1, { message: 'Minimum must be at least 1.' });
    required(path.max, { message: 'Maximum is required.' });
    min(path.max, 1, { message: 'Maximum must be at least 1.' });
  });

  private readonly priceModel = signal({ price: 0 });

  protected readonly priceForm = form(this.priceModel, (path) => {
    required(path.price, { message: 'Price is required.' });
    min(path.price, 0, { message: 'Price cannot be negative.' });
  });

  protected select(tourId: string): void {
    this.error.set(null);
    this.groupSizeTourId.set(null);
    this.priceTourId.set(null);
    this.selectedTourId.set(tourId);
    this.form().reset({ transport: 'Walking', minutes: 30 });
  }

  protected async publish(tourId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.publish(tourId);
      this.tours.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not publish the tour.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async addTransportTime(event: Event): Promise<void> {
    event.preventDefault();
    const tourId = this.selectedTourId();
    if (tourId === null) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.addTransportTime(tourId, {
        transport: this.form.transport().value(),
        minutes: this.form.minutes().value(),
      });
      this.tours.reload();
      this.selectedTourId.set(null);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not add the transport time.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected selectGroupSize(tour: TourDto): void {
    this.error.set(null);
    this.selectedTourId.set(null);
    this.priceTourId.set(null);
    this.groupSizeTourId.set(tour.id);
    this.groupSizeForm().reset({
      min: tour.recommendedGroupSize?.min ?? null,
      max: tour.recommendedGroupSize?.max ?? null,
    });
  }

  protected async changeGroupSize(event: Event): Promise<void> {
    event.preventDefault();
    const tourId = this.groupSizeTourId();
    const minPeople = this.groupSizeForm.min().value();
    const maxPeople = this.groupSizeForm.max().value();
    if (tourId === null || minPeople === null || maxPeople === null) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.changeRecommendedGroupSize(tourId, {
        min: minPeople,
        max: maxPeople,
      });
      this.tours.reload();
      this.groupSizeTourId.set(null);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not change the recommended group size.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected selectPrice(tour: TourDto): void {
    this.error.set(null);
    this.selectedTourId.set(null);
    this.groupSizeTourId.set(null);
    this.priceTourId.set(tour.id);
    this.priceForm().reset({ price: tour.price });
  }

  protected async changePrice(event: Event): Promise<void> {
    event.preventDefault();
    const tourId = this.priceTourId();
    if (tourId === null) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.changePrice(tourId, { price: this.priceForm.price().value() });
      this.tours.reload();
      this.priceTourId.set(null);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not change the price.'));
    } finally {
      this.pending.set(false);
    }
  }
}
