import { httpResource } from '@angular/common/http';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormField, form, min, validate } from '@angular/forms/signals';
import { PageResult } from '../../../../shared/api/page-result';
import { MapLocation, MapView } from '../../../../shared/map/map';
import {
  LocatedTourDto,
  TourDifficulty,
  TourLocationSearchDto,
  TransportMode,
} from '../../api/exploration-api-types';
import { TourBrowsing } from '../tour-browsing';

const pageSize = 10;

const emptyCriteria = {
  latitude: null as number | null,
  longitude: null as number | null,
  radiusKm: null as number | null,
  name: '',
  easy: false,
  moderate: false,
  hard: false,
  tags: '',
  walking: false,
  bicycle: false,
  car: false,
  minPrice: null as number | null,
  maxPrice: null as number | null,
  minLengthKm: null as number | null,
  maxLengthKm: null as number | null,
};

const wholePriceError = { kind: 'wholeNumber', message: 'Price must be a whole number.' };

const positiveRadiusError = {
  kind: 'positiveRadius',
  message: 'Radius must be a positive number of kilometres.',
};

@Component({
  imports: [FormField, MapView],
  selector: 'app-map-search',
  styleUrl: './map-search.scss',
  templateUrl: './map-search.html',
})
export class MapSearch {
  private readonly tourBrowsing = inject(TourBrowsing);

  private readonly criteria = signal({ ...emptyCriteria });

  protected readonly form = form(this.criteria, (path) => {
    validate(path.radiusKm, ({ value }) => {
      const radius = value();
      return radius !== null && radius > 0 ? undefined : positiveRadiusError;
    });
    min(path.minPrice, 0, { message: 'Lowest price cannot be negative.' });
    min(path.maxPrice, 0, { message: 'Highest price cannot be negative.' });
    validate(path.minPrice, ({ value }) =>
      value() === null || Number.isInteger(value()) ? undefined : wholePriceError,
    );
    validate(path.maxPrice, ({ value }) =>
      value() === null || Number.isInteger(value()) ? undefined : wholePriceError,
    );
    min(path.minLengthKm, 0, { message: 'Shortest length cannot be negative.' });
    min(path.maxLengthKm, 0, { message: 'Longest length cannot be negative.' });
  });

  protected readonly selectedLocation = computed<MapLocation | null>(() => {
    const { latitude, longitude } = this.criteria();
    return latitude !== null && longitude !== null ? { latitude, longitude } : null;
  });

  protected readonly canSearch = computed(
    () => this.selectedLocation() !== null && this.form.radiusKm().valid(),
  );

  // The search runs only on "Search": the resource reads these criteria, never the live form.
  private readonly submitted = signal<TourLocationSearchDto | null>(null);

  protected readonly hasSearched = computed(() => this.submitted() !== null);

  protected readonly page = signal(1);

  protected readonly tours = httpResource<PageResult<LocatedTourDto>>(() => {
    const search = this.submitted();
    if (search === null) {
      return undefined;
    }

    const params = new URLSearchParams();
    params.append('latitude', String(search.latitude));
    params.append('longitude', String(search.longitude));
    params.append('radiusKm', String(search.radiusKm));
    params.append('page', String(this.page()));
    params.append('pageSize', String(pageSize));

    if (search.name !== null) {
      params.append('name', search.name);
    }
    for (const difficulty of search.difficulties ?? []) {
      params.append('difficulties', difficulty);
    }
    for (const tag of search.tags ?? []) {
      params.append('tags', tag);
    }
    for (const transportMode of search.transportModes ?? []) {
      params.append('transportModes', transportMode);
    }
    if (search.minPrice !== null) {
      params.append('minPrice', String(search.minPrice));
    }
    if (search.maxPrice !== null) {
      params.append('maxPrice', String(search.maxPrice));
    }
    if (search.minLengthKm !== null) {
      params.append('minLengthKm', String(search.minLengthKm));
    }
    if (search.maxLengthKm !== null) {
      params.append('maxLengthKm', String(search.maxLengthKm));
    }

    return `/api/exploration/tours/published/by-location?${params.toString()}`;
  });

  // One pin per found tour, at its first visible key point.
  protected readonly tourLocations = computed<MapLocation[]>(() => {
    if (!this.tours.hasValue()) {
      return [];
    }
    return (this.tours.value()?.items ?? []).map((tour) => ({
      latitude: tour.markerLatitude,
      longitude: tour.markerLongitude,
      name: tour.name,
    }));
  });

  // True from a click on "Search" until its results arrive; paging and clearing never set it.
  private readonly recordPending = signal(false);

  // An effect, because the confirmed search must drive something outside the component
  // tree: a write on the server. The results load in the resource, so only the effect
  // sees the moment they arrive, together with the total count the record needs.
  private readonly recordSearch = effect(() => {
    if (!this.recordPending()) {
      return;
    }
    const status = this.tours.status();
    if (status === 'resolved') {
      this.recordPending.set(false);
      const search = this.submitted();
      const result = this.tours.value();
      if (search !== null && result !== undefined) {
        void this.saveLocationSearch(search, result.totalCount);
      }
    } else if (status === 'error') {
      this.recordPending.set(false);
    }
  });

  protected readonly isFirstPage = computed(() => this.page() === 1);

  protected readonly isLastPage = computed(
    () => this.page() * pageSize >= (this.tours.value()?.totalCount ?? 0),
  );

  protected pickLocation(location: MapLocation): void {
    this.form.latitude().value.set(location.latitude);
    this.form.longitude().value.set(location.longitude);
  }

  protected search(event: Event): void {
    event.preventDefault();
    const criteria = this.criteria();
    if (criteria.latitude === null || criteria.longitude === null || criteria.radiusKm === null) {
      return;
    }

    const difficulties: TourDifficulty[] = [];
    if (criteria.easy) {
      difficulties.push('Easy');
    }
    if (criteria.moderate) {
      difficulties.push('Moderate');
    }
    if (criteria.hard) {
      difficulties.push('Hard');
    }
    const transportModes: TransportMode[] = [];
    if (criteria.walking) {
      transportModes.push('Walking');
    }
    if (criteria.bicycle) {
      transportModes.push('Bicycle');
    }
    if (criteria.car) {
      transportModes.push('Car');
    }
    const tags = criteria.tags
      .split(',')
      .map((tag) => tag.trim())
      .filter((tag) => tag !== '');
    const name = criteria.name.trim();

    this.submitted.set({
      latitude: criteria.latitude,
      longitude: criteria.longitude,
      radiusKm: criteria.radiusKm,
      name: name === '' ? null : name,
      difficulties,
      tags,
      transportModes,
      minPrice: this.form.minPrice().valid() ? criteria.minPrice : null,
      maxPrice: this.form.maxPrice().valid() ? criteria.maxPrice : null,
      minLengthKm: this.form.minLengthKm().valid() ? criteria.minLengthKm : null,
      maxLengthKm: this.form.maxLengthKm().valid() ? criteria.maxLengthKm : null,
    });
    this.page.set(1);
    this.recordPending.set(true);
  }

  // Clearing also clears the results, so the list and the pins never show tours for criteria that are gone.
  protected clearFilters(): void {
    this.recordPending.set(false);
    this.form().reset({ ...emptyCriteria });
    this.submitted.set(null);
    this.page.set(1);
  }

  protected previousPage(): void {
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.page.update((page) => page + 1);
  }

  private async saveLocationSearch(search: TourLocationSearchDto, resultCount: number): Promise<void> {
    try {
      await this.tourBrowsing.saveLocationSearch({ ...search, resultCount });
    } catch {
      // The record serves later analytics, so a failed save shows nothing and keeps the
      // results on screen; the server's middleware logs the error.
    }
  }
}
