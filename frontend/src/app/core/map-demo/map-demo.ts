import { Component, signal } from '@angular/core';
import { MapLocation, MapView } from '../../shared/map/map';

@Component({
  imports: [MapView],
  selector: 'app-map-demo',
  templateUrl: './map-demo.html',
})
export class MapDemo {
  // Made-up stops, so the map can be tried without a backend.
  protected readonly locations: MapLocation[] = [
    { latitude: 45.2517, longitude: 19.8636, name: 'Petrovaradinska tvrđava' },
    { latitude: 45.2551, longitude: 19.8451, name: 'Trg slobode' },
    { latitude: 45.2559, longitude: 19.851, name: 'Dunavski park' },
    { latitude: 45.244, longitude: 19.8462, name: 'Štrand' },
  ];

  protected readonly showPath = signal(false);
  protected readonly pathLength = signal<number | null>(null);

  protected togglePath(): void {
    this.showPath.update((shown) => !shown);
  }
}
