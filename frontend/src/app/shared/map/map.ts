import { Component, ElementRef, computed, effect, input, output, signal, viewChild } from '@angular/core';
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

function sameStops(first: L.LatLng[], second: L.LatLng[]): boolean {
  return first.length === second.length && first.every((stop, index) => stop.equals(second[index]));
}

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
  private path: L.Routing.Control | null = null;

  // Compared by coordinates, so a new array with the same stops does not ask the server again.
  private readonly stops = computed(
    () => this.locations().map((location) => L.latLng(location.latitude, location.longitude)),
    { equal: sameStops },
  );

  constructor() {
    // Leaflet draws outside the component tree, so effects hand the signal inputs over to it.
    effect((onCleanup) => {
      const map = L.map(this.mapElement().nativeElement);
      L.tileLayer(TILES_URL, { attribution: TILES_ATTRIBUTION }).addTo(map);
      this.map.set(map);
      onCleanup(() => {
        // The path is drawn on the map, so it goes first.
        this.removePath();
        map.remove();
      });
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
      const stops = this.stops();
      this.loadingPath.set(false);
      this.routingFailed.set(false);
      this.pathLength.emit(null);
      if (!this.showPath() || stops.length < 2) {
        return;
      }

      // An answer that arrives after the stops changed or the page closed belongs to an old
      // path, so it is dropped before it reaches the routing library.
      let current = true;
      const walkingRoutes = L.Routing.osrmv1({
        serviceUrl: WALKING_ROUTES_URL,
        profile: 'foot',
        timeout: ROUTING_TIMEOUT_MS,
      });
      const router: L.Routing.IRouter = {
        route: (waypoints, callback, context, options) =>
          walkingRoutes.route(
            waypoints,
            (error?: L.Routing.IError, routes?: L.Routing.IRoute[]) => {
              if (current) {
                callback.call(context, error, routes);
              }
            },
            context,
            options,
          ),
      };

      const path = L.Routing.control({
        router,
        plan: L.Routing.plan(stops, { createMarker: () => false, draggableWaypoints: false }),
        addWaypoints: false,
        fitSelectedRoutes: false,
        show: false,
      });

      path.on('routesfound', (event) => {
        this.loadingPath.set(false);
        this.pathLength.emit(event.routes[0].summary.totalDistance / 1000);
      });
      path.on('routingerror', () => {
        this.loadingPath.set(false);
        this.routingFailed.set(true);
        this.pathLength.emit(null);
      });
      this.loadingPath.set(true);
      path.addTo(map);
      this.path = path;

      // The pins already name the stops, so the turn-by-turn panel stays out of the map.
      const panel = path.getContainer();
      if (panel) {
        panel.style.display = 'none';
      }

      onCleanup(() => {
        current = false;
        this.removePath();
      });
    });
  }

  private removePath(): void {
    this.path?.remove();
    this.path = null;
  }
}
