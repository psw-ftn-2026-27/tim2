import { Component, ElementRef, effect, input, output, signal, viewChild } from '@angular/core';
import L from 'leaflet';
import 'leaflet-routing-machine';

export interface MapLocation {
  latitude: number;
  longitude: number;
  name?: string;
}

const NOVI_SAD: L.LatLngTuple = [45.2551, 19.8451];
const TILES_URL = 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
const TILES_ATTRIBUTION =
  '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors';
const WALKING_ROUTES_URL = 'https://routing.openstreetmap.de/routed-foot/route/v1';
// A walk through a city comes back in a second or two; after this long the server is not answering.
const ROUTING_TIMEOUT_MS = 10_000;

// The marker images are copied to /leaflet by angular.json.
const PIN = new L.Icon.Default({ imagePath: 'leaflet/' });

@Component({
  selector: 'app-map',
  styleUrl: './map.scss',
  templateUrl: './map.html',
})
export class MapView {
  readonly locations = input<MapLocation[]>([]);
  readonly showPath = input(false);

  readonly pathLength = output<number | null>();

  protected readonly loadingPath = signal(false);
  protected readonly routingFailed = signal(false);
  private readonly mapElement = viewChild.required<ElementRef<HTMLElement>>('map');
  private readonly map = signal<L.Map | null>(null);

  constructor() {
    // Leaflet draws outside the component tree, so effects hand the signal inputs over to it.
    effect((onCleanup) => {
      const map = L.map(this.mapElement().nativeElement);
      L.tileLayer(TILES_URL, { attribution: TILES_ATTRIBUTION }).addTo(map);
      this.map.set(map);
      onCleanup(() => map.remove());
    });

    effect((onCleanup) => {
      const map = this.map();
      if (map === null) {
        return;
      }

      const pins = this.locations().map((location) => {
        const pin = L.marker([location.latitude, location.longitude], { icon: PIN });
        if (location.name) {
          pin.bindTooltip(location.name);
        }
        return pin.addTo(map);
      });

      if (pins.length === 0) {
        map.setView(NOVI_SAD, 13);
      } else {
        map.fitBounds(L.latLngBounds(pins.map((pin) => pin.getLatLng())), {
          maxZoom: 16,
          padding: [32, 32],
        });
      }

      onCleanup(() => pins.forEach((pin) => pin.remove()));
    });

    effect((onCleanup) => {
      const map = this.map();
      if (map === null) {
        return;
      }

      // Until a new path is drawn there is no length, so the page never keeps an old one.
      const locations = this.locations();
      this.loadingPath.set(false);
      this.routingFailed.set(false);
      this.pathLength.emit(null);
      if (!this.showPath() || locations.length < 2) {
        return;
      }

      const waypoints = locations.map((location) =>
        L.latLng(location.latitude, location.longitude),
      );
      const path = L.Routing.control({
        router: L.Routing.osrmv1({
          serviceUrl: WALKING_ROUTES_URL,
          profile: 'foot',
          timeout: ROUTING_TIMEOUT_MS,
        }),
        plan: L.Routing.plan(waypoints, { createMarker: () => false, draggableWaypoints: false }),
        addWaypoints: false,
        fitSelectedRoutes: false,
        show: false,
      });

      // An answer that arrives after the inputs changed belongs to an old path, so it is ignored.
      let current = true;
      path.on('routesfound', (event) => {
        if (current) {
          this.loadingPath.set(false);
          this.pathLength.emit(event.routes[0].summary.totalDistance / 1000);
        }
      });
      path.on('routingerror', () => {
        if (current) {
          this.loadingPath.set(false);
          this.routingFailed.set(true);
          this.pathLength.emit(null);
        }
      });
      this.loadingPath.set(true);
      path.addTo(map);

      // The pins already name the stops, so the turn-by-turn panel stays out of the map.
      const panel = path.getContainer();
      if (panel) {
        panel.style.display = 'none';
      }

      onCleanup(() => {
        current = false;
        path.remove();
      });
    });
  }
}
