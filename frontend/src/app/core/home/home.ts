import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../auth/auth';

@Component({
  imports: [RouterLink],
  selector: 'app-home',
  styleUrl: './home.scss',
  templateUrl: './home.html',
})
export class Home {
  protected readonly auth = inject(Auth);

  protected linkTo(destination: string): string {
    return this.auth.isLoggedIn() ? destination : '/login';
  }

  protected remember(destination: string): void {
    if (!this.auth.isLoggedIn()) {
      this.auth.rememberDestination(destination);
    }
  }
}
