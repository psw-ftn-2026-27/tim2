import { Component, ElementRef, computed, effect, input, output, signal, viewChild } from '@angular/core';
import L from 'leaflet';
import 'leaflet-routing-machine';

export interface MapLocation {
  latitude: number;
  longitude: number;
  name?: string;
  // A short mark written on the pin itself, such as "S" or "1", so pins can be told apart.
  label?: string;
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
const LABEL_SIZE = 28;

// Leaflet draws the pin outside the component, so its look is the global class .map-label.
function labeledPin(label: string): L.DivIcon {
  const text = document.createElement('span');
  // textContent, not HTML, so a label is always shown as plain text.
  text.textContent = label;
  return L.divIcon({
    className: 'map-label',
    html: text,
    iconSize: [LABEL_SIZE, LABEL_SIZE],
    tooltipAnchor: [LABEL_SIZE / 2, 0],
  });
}

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
  readonly pickLocation = input(false);
  readonly selectedLocation = input<MapLocation | null>(null);

  readonly pathLength = output<number | null>();
  readonly locationPicked = output<MapLocation>();

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
        const icon = location.label ? labeledPin(location.label) : PIN;
        const pin = L.marker([location.latitude, location.longitude], { icon });
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

    // A click only reports the place; the page keeps it and passes it back as selectedLocation.
    effect((onCleanup) => {
      const map = this.map();
      if (map === null || !this.pickLocation()) {
        return;
      }

      // After the map is panned past the date line, Leaflet reports longitudes outside
      // -180..180; wrap() brings the clicked place back into that range.
      const pick = (event: L.LeafletMouseEvent) => {
        const place = event.latlng.wrap();
        this.locationPicked.emit({ latitude: place.lat, longitude: place.lng });
      };
      map.on('click', pick);
      onCleanup(() => map.off('click', pick));
    });

    // The picked pin moves only when the page passes a new location, and the view stays where it is.
    effect((onCleanup) => {
      const map = this.map();
      const selected = this.selectedLocation();
      if (map === null || selected === null) {
        return;
      }

      // Leaflet takes a colour value, not a CSS variable, so the token is read from the page.
      const accent = getComputedStyle(this.mapElement().nativeElement)
        .getPropertyValue('--color-accent')
        .trim();
      const pin = L.circleMarker([selected.latitude, selected.longitude], {
        className: 'picked-pin',
        color: accent,
        fillColor: accent,
        fillOpacity: 0.5,
        radius: 10,
        // A click on the pin goes through to the map, so it picks that place like any other.
        interactive: false,
      });
      // The pin takes no hover, so its name stays on show instead of waiting for the mouse.
      if (selected.name) {
        pin.bindTooltip(selected.name, { permanent: true });
      }
      pin.addTo(map);

      // When editing a place far away, the pin would start off screen; after a click it is already in view.
      if (!map.getBounds().contains(pin.getLatLng())) {
        map.panTo(pin.getLatLng());
      }

      onCleanup(() => pin.remove());
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
