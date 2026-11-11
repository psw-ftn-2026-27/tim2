import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, min, validate } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { SortDirection, TourDto } from '../../api/exploration-api-types';
import { TourCard } from '../tour-card/tour-card';

const pageSize = 10;

const emptyCriteria = {
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
  difficultySort: 'None' as SortDirection,
  priceSort: 'None' as SortDirection,
};

const wholePriceError = { kind: 'wholeNumber', message: 'Price must be a whole number.' };

@Component({
  imports: [FormField, RouterLink, TourCard],
  selector: 'app-tour-list',
  styleUrl: './tour-list.scss',
  templateUrl: './tour-list.html',
})
export class TourList {
  protected readonly auth = inject(Auth);

  private readonly criteria = signal({ ...emptyCriteria });

  protected readonly form = form(this.criteria, (path) => {
    min(path.minPrice, 0, { message: 'Lowest price cannot be negative.' });
    min(path.maxPrice, 0, { message: 'Highest price cannot be negative.' });
    validate(path.minPrice, ({ value }) =>
      value() === null || Number.isInteger(value()) ? undefined : wholePriceError,
    );
    validate(path.maxPrice, ({ value }) =>
      value() === null || Number.isInteger(value()) ? undefined : wholePriceError,
    );
  });

  protected readonly page = signal(1);

  protected readonly tours = httpResource<PageResult<TourDto>>(() => {
    const criteria = this.criteria();
    const params = new URLSearchParams();
    params.append('page', String(this.page()));
    params.append('pageSize', String(pageSize));

    if (criteria.name.trim() !== '') {
      params.append('name', criteria.name.trim());
    }
    if (criteria.easy) {
      params.append('difficulties', 'Easy');
    }
    if (criteria.moderate) {
      params.append('difficulties', 'Moderate');
    }
    if (criteria.hard) {
      params.append('difficulties', 'Hard');
    }
    const tags = criteria.tags
      .split(',')
      .map((tag) => tag.trim())
      .filter((tag) => tag !== '');
    for (const tag of tags) {
      params.append('tags', tag);
    }
    if (criteria.walking) {
      params.append('transportModes', 'Walking');
    }
    if (criteria.bicycle) {
      params.append('transportModes', 'Bicycle');
    }
    if (criteria.car) {
      params.append('transportModes', 'Car');
    }
    if (criteria.minPrice !== null && this.form.minPrice().valid()) {
      params.append('minPrice', String(criteria.minPrice));
    }
    if (criteria.maxPrice !== null && this.form.maxPrice().valid()) {
      params.append('maxPrice', String(criteria.maxPrice));
    }
    if (criteria.difficultySort !== 'None') {
      params.append('difficultySort', criteria.difficultySort);
    }
    if (criteria.priceSort !== 'None') {
      params.append('priceSort', criteria.priceSort);
    }

    return `/api/exploration/tours/published?${params.toString()}`;
  });

  protected readonly isFirstPage = computed(() => this.page() === 1);

  protected readonly isLastPage = computed(
    () => this.page() * pageSize >= (this.tours.value()?.totalCount ?? 0),
  );

  protected backToFirstPage(): void {
    this.page.set(1);
  }

  protected clearFilters(): void {
    this.criteria.set({ ...emptyCriteria });
    this.page.set(1);
  }

  protected previousPage(): void {
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.page.update((page) => page + 1);
  }
}
