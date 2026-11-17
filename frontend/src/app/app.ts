import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { Auth } from './core/auth/auth';
import { Analytics } from './shared/analytics/analytics';
import { ConsentBanner } from './shared/analytics/consent-banner/consent-banner';

@Component({
  imports: [ConsentBanner, RouterLink, RouterOutlet],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {
  protected readonly auth = inject(Auth);
  protected readonly analytics = inject(Analytics);

  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );

  // Whether "Cookie settings" opened the banner again; view state, not a service operation.
  private readonly settingsOpen = signal(false);

  protected readonly showBanner = computed(
    () =>
      this.auth.user() !== null && (this.analytics.consent() === 'none' || this.settingsOpen()),
  );

  protected openSettings(): void {
    this.settingsOpen.set(true);
  }

  protected logout(): void {
    this.settingsOpen.set(false);
    this.auth.logout();
  }

  protected accept(): void {
    this.analytics.accept();
    this.settingsOpen.set(false);
  }

  protected decline(): void {
    this.analytics.decline();
    this.settingsOpen.set(false);
  }
}
