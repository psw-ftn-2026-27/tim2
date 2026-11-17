import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { ImageUpload } from '../../../../shared/image-upload/image-upload';
import { Images } from '../../../../shared/images/images';
import { MapLocation, MapView } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { LocationChallengeDto } from '../../api/games-api-types';
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
  toLocationChallengeFormModel,
} from '../location-challenge-form-schema';

@Component({
  imports: [FormField, ImageUpload, MapView, RouterLink],
  selector: 'app-location-challenge-details',
  styleUrl: './location-challenge-details.scss',
  templateUrl: './location-challenge-details.html',
})
export class LocationChallengeDetails {
  private readonly management = inject(LocationChallengeManagement);
  private readonly images = inject(Images);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly challenge = httpResource<LocationChallengeDto>(
    () => `/api/games/location-challenges/${this.id()}`,
  );

  // The start and every hidden point, named, for the map of the loaded challenge.
  protected readonly places = computed<MapLocation[]>(() => {
    const challenge = this.challenge.value();
    if (challenge === undefined) {
      return [];
    }
    return [
      { ...challenge.startLocation, name: 'Start' },
      ...challenge.hiddenPoints.map((point, index) => ({
        ...point.location,
        name: `Point ${index + 1}`,
      })),
    ];
  });

  protected readonly confirmingAction = signal<'delete' | 'publish' | 'retire' | 'archive' | 'repeat' | null>(
    null,
  );
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly editing = signal(false);
  protected readonly editError = signal<string | null>(null);

  private readonly model = signal<LocationChallengeFormModel>({
    name: '',
    xp: 10,
    startLocation: null,
    hiddenPoints: [emptyPoint()],
  });

  protected readonly editForm = form(this.model, locationChallengeFormSchema);

  // The place a click on the map sets: 'start', the index of a hidden point, or null when
  // nothing is being set.
  protected readonly picking = signal<'start' | number | null>(null);
  protected readonly pickingName = computed(() => {
    const place = this.picking();
    return place === null ? null : placeName(place);
  });
  protected readonly otherPlaces = computed(() => otherPlaces(this.model(), this.picking()));
  protected readonly pickedPlace = computed(() => pickedPlace(this.model(), this.picking()));

  // The upload state of each hidden point, in the same order as the points.
  protected readonly uploads = signal<{ pending: boolean; error: string | null }[]>([]);
  protected readonly uploading = computed(() => this.uploads().some((upload) => upload.pending));

  // Edit is shown only after the challenge has loaded, so the form is filled with that challenge.
  protected startEdit(challenge: LocationChallengeDto): void {
    this.error.set(null);
    this.editError.set(null);
    this.confirmingAction.set(null);
    this.editForm().reset(toLocationChallengeFormModel(challenge));
    this.uploads.set(challenge.hiddenPoints.map(() => ({ pending: false, error: null })));
    this.picking.set(null);
    this.editing.set(true);
  }

  protected cancelEdit(): void {
    this.editing.set(false);
  }

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

  protected async saveEdit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.editError.set(null);
    try {
      await this.management.update(this.id(), toLocationChallengeDto(this.model()));
      this.editing.set(false);
      this.challenge.reload();
    } catch (failure) {
      this.editError.set(serverMessage(failure, 'Could not update the location challenge.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected askToConfirm(action: 'delete' | 'publish' | 'retire' | 'archive' | 'repeat'): void {
    this.error.set(null);
    this.confirmingAction.set(action);
  }

  protected cancelConfirm(): void {
    this.confirmingAction.set(null);
  }

  // After a rejection nothing reloads: the challenge stays as shown and the server message says why.
  protected async confirm(): Promise<void> {
    const action = this.confirmingAction();
    this.pending.set(true);
    this.error.set(null);
    try {
      if (action === 'delete') {
        await this.management.delete(this.id());
        await this.router.navigate(['/games/challenges']);
        return;
      }
      if (action === 'publish') {
        await this.management.publish(this.id());
      } else if (action === 'retire') {
        await this.management.retire(this.id());
      } else if (action === 'archive') {
        await this.management.archive(this.id());
      } else {
        await this.management.repeat(this.id());
      }
      this.challenge.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, `Could not ${action} the location challenge.`));
    } finally {
      this.confirmingAction.set(null);
      this.pending.set(false);
    }
  }

  private setUpload(pointIndex: number, upload: { pending: boolean; error: string | null }): void {
    this.uploads.update((uploads) =>
      uploads.map((current, index) => (index === pointIndex ? upload : current)),
    );
  }
}
