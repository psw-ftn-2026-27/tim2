import { Component, input } from '@angular/core';
import { ClubDto } from '../../api/social-api-types';

@Component({
  selector: 'app-club-card',
  styleUrl: './club-card.scss',
  templateUrl: './club-card.html',
})
export class ClubCard {
  readonly club = input.required<ClubDto>();
}
