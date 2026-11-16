import { httpResource } from '@angular/common/http';
import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../auth/auth';

// The host's own view of the agreed featured-tours address, only the fields this page uses.
export interface FeaturedTour {
  name: string;
  description: string;
  price: number;
}

const excerptLength = 120;

@Component({
  imports: [RouterLink],
  selector: 'app-home',
  styleUrl: './home.scss',
  templateUrl: './home.html',
})
export class Home {
  protected readonly auth = inject(Auth);

  protected readonly featured = httpResource<FeaturedTour[]>(
    () => '/api/exploration/tours/featured',
  );

  // Empty while loading and after a failure, so the section simply stays hidden.
  protected readonly featuredTours = computed(() =>
    this.featured.hasValue() ? this.featured.value() : [],
  );

  protected linkTo(destination: string): string {
    return this.auth.isLoggedIn() ? destination : '/login';
  }

  protected remember(destination: string): void {
    if (!this.auth.isLoggedIn()) {
      this.auth.rememberDestination(destination);
    }
  }

  protected excerpt(description: string): string {
    if (description.length <= excerptLength) {
      return description;
    }
    return description.slice(0, excerptLength - 1).trimEnd() + '…';
  }
}
