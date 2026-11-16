import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { Auth } from '../../core/auth/auth';
import { environment } from '../../../environments/environment';
import { AnalyticsEvent } from './analytics-events';

const CONSENT_KEY_PREFIX = 'explorer.analytics-consent.';
const SCRIPT_URL = 'https://www.googletagmanager.com/gtag/js?id=';

export type Consent = 'none' | 'accepted' | 'declined';
type Choice = 'accepted' | 'declined';

// The two globals Google's gtag.js reads from the window.
interface GoogleWindow {
  dataLayer?: unknown[];
  [optOutFlag: string]: unknown;
}

function googleWindow(): GoogleWindow {
  return window as unknown as GoogleWindow;
}

// Google's own snippet: gtag.js reads the arguments object, not an array, so the
// function keeps the classic `function` form and pushes `arguments` as it is. The
// parameter list only tells TypeScript that gtag takes any arguments.
// eslint-disable-next-line @typescript-eslint/no-unused-vars
function gtag(..._args: unknown[]): void {
  const target = googleWindow();
  target.dataLayer = target.dataLayer ?? [];
  // eslint-disable-next-line prefer-rest-params
  target.dataLayer.push(arguments);
}

@Injectable({ providedIn: 'root' })
export class Analytics {
  private readonly auth = inject(Auth);

  // Choices made in this session, by user id, so a click is seen immediately.
  private readonly choices = signal<Record<string, Choice>>({});
  private scriptLoaded = false;

  readonly consent = computed<Consent>(() => {
    const user = this.auth.user();
    if (user === null) {
      return 'none';
    }
    const stored = this.choices()[user.id] ?? localStorage.getItem(CONSENT_KEY_PREFIX + user.id);
    if (stored === 'accepted' || stored === 'declined') {
      return stored;
    }
    return 'none';
  });

  private readonly sendingAllowed = computed(
    () =>
      this.auth.user() !== null &&
      this.consent() === 'accepted' &&
      environment.measurementId !== '',
  );

  // An effect, because the consent must drive something outside Angular: Google's
  // opt-out flag. Once gtag.js is loaded it can also send events on its own, so a
  // decline or a logout has to switch the whole library off, not just our calls.
  private readonly optOut = effect(() => {
    if (environment.measurementId !== '') {
      googleWindow()['ga-disable-' + environment.measurementId] = !this.sendingAllowed();
    }
  });

  accept(): void {
    this.choose('accepted');
  }

  decline(): void {
    this.choose('declined');
  }

  // Modules call this after the server confirms their command. Without a signed-in
  // user, consent or measurement ID it does nothing, silently.
  logEvent(event: AnalyticsEvent): void {
    const user = this.auth.user();
    if (user === null || !this.sendingAllowed()) {
      return;
    }
    this.loadScript();
    gtag('event', event.name, { ...event.params, user_role: user.roles.join(',') });
  }

  private choose(choice: Choice): void {
    const user = this.auth.user();
    if (user === null) {
      return;
    }
    localStorage.setItem(CONSENT_KEY_PREFIX + user.id, choice);
    this.choices.update((choices) => ({ ...choices, [user.id]: choice }));
  }

  // Loads gtag.js the first time sending is allowed. If the browser blocks it, the
  // queued calls simply stay in dataLayer and the app keeps working.
  private loadScript(): void {
    if (this.scriptLoaded) {
      return;
    }
    this.scriptLoaded = true;

    gtag('js', new Date());
    // Google treats any debug_mode value, even false, as debug, so it is set only when on.
    if (environment.debugMode) {
      gtag('config', environment.measurementId, { send_page_view: false, debug_mode: true });
    } else {
      gtag('config', environment.measurementId, { send_page_view: false });
    }

    const script = document.createElement('script');
    script.async = true;
    script.src = SCRIPT_URL + environment.measurementId;
    document.head.appendChild(script);
  }
}
