import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { ImageUpload } from '../../../../shared/image-upload/image-upload';
import { Images } from '../../../../shared/images/images';
import { MapLocation, MapView } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { LocationChallengeManagement } from '../location-challenge-management';
import {
  LocationChallengeFormModel,
  addPoint,
  emptyPoint,
  locationChallengeFormSchema,
  otherPlaces,
  pickedPlace,
  placeName,
  removePoint,
  setPointImage,
  setPointLocation,
  setStartLocation,
  toLocationChallengeDto,
} from '../location-challenge-form-schema';

@Component({
  imports: [FormField, ImageUpload, MapView, RouterLink],
  selector: 'app-create-location-challenge',
  styleUrl: './create-location-challenge.scss',
  templateUrl: './create-location-challenge.html',
})
export class CreateLocationChallenge {
  private readonly management = inject(LocationChallengeManagement);
  private readonly images = inject(Images);
  private readonly router = inject(Router);

  private readonly model = signal<LocationChallengeFormModel>({
    name: '',
    xp: 10,
    startLocation: null,
    hiddenPoints: [emptyPoint()],
  });

  protected readonly form = form(this.model, locationChallengeFormSchema);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // The place a click on the map sets: 'start', the index of a hidden point, or null when
  // nothing is being set. A new challenge has nothing placed yet, so the first click sets the start.
  protected readonly picking = signal<'start' | number | null>('start');
  protected readonly pickingName = computed(() => {
    const place = this.picking();
    return place === null ? null : placeName(place);
  });
  protected readonly otherPlaces = computed(() => otherPlaces(this.model(), this.picking()));
  protected readonly pickedPlace = computed(() => pickedPlace(this.model(), this.picking()));

  // The upload state of each hidden point, in the same order as the points.
  protected readonly uploads = signal([{ pending: false, error: null as string | null }]);
  protected readonly uploading = computed(() => this.uploads().some((upload) => upload.pending));

  protected pick(place: 'start' | number): void {
    this.picking.set(place);
  }

  protected cancelPicking(): void {
    this.picking.set(null);
  }

  // One click places exactly one place, then nothing is being set, so a later click cannot
  // move a place by accident.
  protected placePicked(location: MapLocation): void {
    const place = this.picking();
    if (place === null) {
      return;
    }
    if (place === 'start') {
      this.model.update((challenge) => setStartLocation(challenge, location));
    } else {
      this.model.update((challenge) => setPointLocation(challenge, place, location));
    }
    this.picking.set(null);
  }

  protected addPoint(): void {
    this.model.update((challenge) => addPoint(challenge));
    this.uploads.update((uploads) => [...uploads, { pending: false, error: null }]);
    // A new point has no place yet, so it is the one the next click sets.
    this.picking.set(this.model().hiddenPoints.length - 1);
  }

  // The place being picked stays the same place: a point after the removed one moves up by
  // one, and removing the point being picked stops picking.
  protected removePoint(pointIndex: number): void {
    this.model.update((challenge) => removePoint(challenge, pointIndex));
    this.uploads.update((uploads) => uploads.filter((_, index) => index !== pointIndex));
    const place = this.picking();
    if (place === pointIndex) {
      this.picking.set(null);
    } else if (typeof place === 'number' && place > pointIndex) {
      this.picking.set(place - 1);
    }
  }

  // Removing a point is disabled while an upload runs, so pointIndex still names the same point.
  protected async upload(pointIndex: number, file: File): Promise<void> {
    this.setUpload(pointIndex, { pending: true, error: null });
    try {
      const uploaded = await this.images.upload(file);
      this.model.update((challenge) => setPointImage(challenge, pointIndex, uploaded.url));
      this.setUpload(pointIndex, { pending: false, error: null });
    } catch (failure) {
      this.setUpload(pointIndex, {
        pending: false,
        error: serverMessage(failure, 'Could not upload the image.'),
      });
    }
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      const id = await this.management.create(toLocationChallengeDto(this.model()));
      await this.router.navigate(['/games/challenges/location', id]);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the location challenge.'));
    } finally {
      this.pending.set(false);
    }
  }

  private setUpload(pointIndex: number, upload: { pending: boolean; error: string | null }): void {
    this.uploads.update((uploads) =>
      uploads.map((current, index) => (index === pointIndex ? upload : current)),
    );
  }
}
