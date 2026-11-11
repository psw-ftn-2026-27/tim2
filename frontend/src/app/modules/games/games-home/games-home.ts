import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../core/auth/auth';

@Component({
  imports: [RouterLink],
  selector: 'app-games-home',
  template: `
    <div class="stack">
      <h1>Games</h1>

      @if (isAdministrator()) {
        <p><a routerLink="/games/challenges">Social challenges</a></p>
      }
    </div>
  `,
})
export class GamesHome {
  private readonly auth = inject(Auth);

  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );
}
