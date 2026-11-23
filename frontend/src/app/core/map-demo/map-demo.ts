import { Component, signal } from '@angular/core';
import { MapCircle, MapLocation, MapView } from '../../shared/map/map';

@Component({
  imports: [MapView],
  selector: 'app-map-demo',
  templateUrl: './map-demo.html',
})
export class MapDemo {
  // Made-up stops, so the map can be tried without a backend.
  protected readonly locations: MapLocation[] = [
    { latitude: 45.2517, longitude: 19.8636, name: 'Petrovaradinska tvrđava', label: 'S' },
    { latitude: 45.2551, longitude: 19.8451, name: 'Trg slobode', label: '1' },
    { latitude: 45.2559, longitude: 19.851, name: 'Dunavski park', label: '2' },
    { latitude: 45.244, longitude: 19.8462, name: 'Štrand', label: '3' },
  ];

  // Reaches south past Štrand, so the view must take in the pins and the circle together.
  protected readonly searchArea: MapCircle = { latitude: 45.244, longitude: 19.8462, radiusMeters: 800 };

  protected readonly showPath = signal(false);
  protected readonly pathLength = signal<number | null>(null);
  // Starts at Trg slobode, like an existing location being changed.
  protected readonly picked = signal<MapLocation>({ latitude: 45.2551, longitude: 19.8451 });

  protected togglePath(): void {
    this.showPath.update((shown) => !shown);
  }
}
