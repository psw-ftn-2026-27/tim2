import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { ImageUpload } from '../../../../shared/image-upload/image-upload';
import { Images } from '../../../../shared/images/images';
import { MapLocation, MapView } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import {
  KeyPointDto,
  KeyPointMoveDirection,
  TourKeyPointsDto,
} from '../../api/exploration-api-types';
import { KeyPointAuthoring } from '../key-point-authoring';

interface KeyPointFormModel {
  name: string;
  description: string;
  imageUrl: string;
  latitude: number | null;
  longitude: number | null;
  secret: string;
  isVisibleBeforePurchase: boolean;
}

const EMPTY_KEY_POINT: KeyPointFormModel = {
  name: '',
  description: '',
  imageUrl: '',
  latitude: null,
  longitude: null,
  secret: '',
  isVisibleBeforePurchase: false,
};

@Component({
  imports: [FormField, ImageUpload, MapView, RouterLink],
  selector: 'app-tour-key-points',
  styleUrl: './tour-key-points.scss',
  templateUrl: './tour-key-points.html',
})
export class TourKeyPoints {
  private readonly keyPointAuthoring = inject(KeyPointAuthoring);
  private readonly images = inject(Images);

  readonly id = input.required<string>();

  protected readonly tour = httpResource<TourKeyPointsDto>(
    () => `/api/exploration/tours/${this.id()}/key-points`,
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly deletingKeyPointId = signal<string | null>(null);

  // Add and edit share one form: it is open when formOpen is set, and editingKeyPointId
  // tells which point it changes (null means a new point).
  protected readonly formOpen = signal(false);
  protected readonly editingKeyPointId = signal<string | null>(null);

  private readonly model = signal<KeyPointFormModel>({ ...EMPTY_KEY_POINT });

  protected readonly form = form(this.model, (path) => {
    required(path.name, { message: 'Name is required.' });
    required(path.description, { message: 'Description is required.' });
    required(path.imageUrl, { message: 'Upload an image.' });
    required(path.latitude, { message: 'Pick a location on the map.' });
  });

  protected readonly selectedLocation = computed<MapLocation | null>(() => {
    const { latitude, longitude } = this.model();
    return latitude !== null && longitude !== null ? { latitude, longitude } : null;
  });

  protected readonly imagePending = signal(false);
  protected readonly imageError = signal<string | null>(null);

  protected startAdd(): void {
    this.deletingKeyPointId.set(null);
    this.editingKeyPointId.set(null);
    this.imageError.set(null);
    this.form().reset({ ...EMPTY_KEY_POINT });
    this.formOpen.set(true);
  }

  protected startEdit(keyPoint: KeyPointDto): void {
    this.deletingKeyPointId.set(null);
    this.editingKeyPointId.set(keyPoint.id);
    this.imageError.set(null);
    this.form().reset({
      name: keyPoint.name,
      description: keyPoint.description,
      imageUrl: keyPoint.imageUrl,
      latitude: keyPoint.latitude,
      longitude: keyPoint.longitude,
      secret: keyPoint.secret ?? '',
      isVisibleBeforePurchase: keyPoint.isVisibleBeforePurchase,
    });
    this.formOpen.set(true);
  }

  protected cancelForm(): void {
    this.formOpen.set(false);
    this.editingKeyPointId.set(null);
  }

  protected async uploadImage(file: File): Promise<void> {
    this.imagePending.set(true);
    this.imageError.set(null);
    try {
      const uploaded = await this.images.upload(file);
      this.form.imageUrl().value.set(uploaded.url);
    } catch (failure) {
      this.imageError.set(serverMessage(failure, 'Could not upload the image.'));
    } finally {
      this.imagePending.set(false);
    }
  }

  protected pickLocation(location: MapLocation): void {
    this.form.latitude().value.set(location.latitude);
    this.form.longitude().value.set(location.longitude);
  }

  protected async save(event: Event): Promise<void> {
    event.preventDefault();
    const latitude = this.form.latitude().value();
    const longitude = this.form.longitude().value();
    if (latitude === null || longitude === null) {
      return;
    }
    const secret = this.form.secret().value();
    const dto = {
      name: this.form.name().value(),
      description: this.form.description().value(),
      imageUrl: this.form.imageUrl().value(),
      latitude,
      longitude,
      secret: secret === '' ? null : secret,
      isVisibleBeforePurchase: this.form.isVisibleBeforePurchase().value(),
    };
    const keyPointId = this.editingKeyPointId();
    this.pending.set(true);
    this.error.set(null);
    try {
      if (keyPointId === null) {
        await this.keyPointAuthoring.add(this.id(), dto);
      } else {
        await this.keyPointAuthoring.change(this.id(), keyPointId, dto);
      }
      this.formOpen.set(false);
      this.editingKeyPointId.set(null);
      this.tour.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the key point.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected startDelete(keyPointId: string): void {
    this.formOpen.set(false);
    this.editingKeyPointId.set(null);
    this.deletingKeyPointId.set(keyPointId);
  }

  protected cancelDelete(): void {
    this.deletingKeyPointId.set(null);
  }

  protected async confirmDelete(keyPointId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.keyPointAuthoring.remove(this.id(), keyPointId);
      this.deletingKeyPointId.set(null);
      this.tour.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the key point.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async move(keyPointId: string, direction: KeyPointMoveDirection): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.keyPointAuthoring.move(this.id(), keyPointId, { direction });
      this.tour.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not move the key point.'));
    } finally {
      this.pending.set(false);
    }
  }
}
