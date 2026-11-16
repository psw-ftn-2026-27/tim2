import { Component, input, output } from '@angular/core';
import { Consent } from '../analytics';

@Component({
  selector: 'app-consent-banner',
  styleUrl: './consent-banner.scss',
  templateUrl: './consent-banner.html',
})
export class ConsentBanner {
  readonly consent = input.required<Consent>();

  readonly accept = output<void>();
  readonly decline = output<void>();
}
