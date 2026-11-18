import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { Auth, User } from '../../core/auth/auth';
import { Analytics } from './analytics';

@Component({ template: '' })
class EmptyPage {}

// A small routes table with the same shapes as the real one: a root page, a lazy-style
// parent with an empty child path, and a child path with a user id.
const ROUTES = [
  { path: '', component: EmptyPage },
  {
    path: 'payment',
    children: [
      { path: '', component: EmptyPage },
      { path: 'wallets/:userId', component: EmptyPage },
    ],
  },
  { path: 'games', component: EmptyPage },
];

const ANA: User = { id: 'ana-id', email: 'ana@explorer.dev', roles: ['tourist'] };
const MARKO: User = { id: 'marko-id', email: 'marko@explorer.dev', roles: ['author'] };

// gtag pushes every call into window.dataLayer, so the test reads what was sent from there.
function dataLayer(): unknown[][] {
  const calls = (window as unknown as { dataLayer?: IArguments[] }).dataLayer ?? [];
  return calls.map((call) => Array.from(call));
}

function pageViews(): Record<string, unknown>[] {
  return dataLayer()
    .filter((call) => call[0] === 'event' && call[1] === 'page_view')
    .map((call) => call[2] as Record<string, unknown>);
}

function rememberConsent(user: User, choice: 'accepted' | 'declined'): void {
  localStorage.setItem('explorer.analytics-consent.' + user.id, choice);
}

describe('Analytics page views', () => {
  // Stands in for Auth: the service only reads the signed-in user.
  const user = signal<User | null>(null);
  let analytics: Analytics;
  let harness: RouterTestingHarness;

  async function visit(url: string): Promise<void> {
    await harness.navigateByUrl(url);
    // Runs the effects, so the page-view effect sees the finished navigation.
    TestBed.tick();
  }

  beforeEach(async () => {
    localStorage.clear();
    (window as unknown as { dataLayer?: unknown[] }).dataLayer = [];
    user.set(null);

    TestBed.configureTestingModule({
      providers: [provideRouter(ROUTES), { provide: Auth, useValue: { user } }],
    });
    analytics = TestBed.inject(Analytics);
    harness = await RouterTestingHarness.create();
  });

  it('sends the route template, never the user id in the address (P3)', async () => {
    user.set(ANA);
    rememberConsent(ANA, 'accepted');

    await visit('/payment/wallets/3f2a-secret-id');

    expect(pageViews()).toHaveLength(1);
    expect(pageViews()[0]['page_path']).toBe('/payment/wallets/:userId');
    expect(JSON.stringify(dataLayer())).not.toContain('3f2a-secret-id');
  });

  it('sends exactly one page view per navigation, and a later change of user or consent sends no second one (P1)', async () => {
    user.set(ANA);
    rememberConsent(ANA, 'accepted');
    rememberConsent(MARKO, 'accepted');

    await visit('/games');
    user.set(MARKO);
    TestBed.tick();
    analytics.decline();
    TestBed.tick();

    expect(pageViews()).toHaveLength(1);
  });

  it('sends nothing without a signed-in user or after a decline (P2)', async () => {
    await visit('/games');

    user.set(ANA);
    rememberConsent(ANA, 'declined');
    await visit('/payment');

    expect(pageViews()).toHaveLength(0);
  });

  it('logs the current page on the first Accept, and nothing on a repeated Accept (P1)', async () => {
    user.set(ANA);
    await visit('/games');
    expect(pageViews()).toHaveLength(0);

    analytics.accept();
    analytics.accept();

    expect(pageViews()).toHaveLength(1);
    expect(pageViews()[0]['page_path']).toBe('/games');
  });

  it('adds nothing to the template for routes with an empty path', async () => {
    user.set(ANA);
    rememberConsent(ANA, 'accepted');

    await visit('/payment');
    await visit('/');

    expect(pageViews().map((view) => view['page_path'])).toEqual(['/payment', '/']);
  });
});
