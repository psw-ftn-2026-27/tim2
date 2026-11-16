import { ComponentFixture, TestBed } from '@angular/core/testing';
import L from 'leaflet';
import { MapLocation, MapView } from './map';

const FORTRESS: MapLocation = {
  latitude: 45.2517,
  longitude: 19.8636,
  name: 'Petrovaradinska tvrđava',
};
const SQUARE: MapLocation = { latitude: 45.2551, longitude: 19.8451, name: 'Trg slobode' };
const PARK: MapLocation = { latitude: 45.2559, longitude: 19.851, name: 'Dunavski park' };
const BEACH: MapLocation = { latitude: 45.244, longitude: 19.8462, name: 'Štrand' };

// Stands in for the walking-route server, so no test touches the network.
class FakeRouter {
  fails = false;
  // While holding, answers wait in `held` until a test releases them, like a slow server.
  holding = false;
  held: (() => void)[] = [];
  distance = 3200;
  calls = 0;
  receivedStops: L.LatLng[] = [];

  async route(
    waypoints: L.Routing.Waypoint[],
    callback: (error?: unknown, routes?: unknown[]) => void,
    context?: object,
  ) {
    this.calls++;
    const stops = waypoints.map((waypoint) => waypoint.latLng);
    this.receivedStops = stops;
    const fails = this.fails;
    const distance = this.distance;
    const answer = () => {
      if (fails) {
        callback.call(context, { status: -1, message: 'No route found' });
        return;
      }
      callback.call(context, undefined, [
        {
          name: 'Walk',
          coordinates: stops,
          waypoints: stops,
          inputWaypoints: waypoints,
          waypointIndices: stops.map((_, index) => index),
          instructions: [],
          summary: { totalDistance: distance, totalTime: 2400 },
        },
      ]);
    };
    if (this.holding) {
      this.held.push(answer);
      return;
    }
    // Like a real server, the answer comes back later, not while the map is still adding the path.
    await Promise.resolve();
    answer();
  }
}

describe('MapView', () => {
  let fixture: ComponentFixture<MapView>;
  let router: FakeRouter;
  let lengths: (number | null)[];

  async function show(locations: MapLocation[], showPath: boolean): Promise<void> {
    fixture.componentRef.setInput('locations', locations);
    fixture.componentRef.setInput('showPath', showPath);
    await fixture.whenStable();
  }

  function count(selector: string): number {
    return fixture.nativeElement.querySelectorAll(selector).length;
  }

  beforeEach(() => {
    router = new FakeRouter();
    vi.spyOn(L.Routing, 'osrmv1').mockReturnValue(router as unknown as L.Routing.OSRMv1);

    fixture = TestBed.createComponent(MapView);
    lengths = [];
    fixture.componentInstance.pathLength.subscribe((length) => lengths.push(length));
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('shows exactly one pin for each location (P1)', async () => {
    await show([FORTRESS, SQUARE, PARK], false);

    expect(count('.leaflet-marker-icon')).toBe(3);
  });

  it('draws no path while showPath is off (P2)', async () => {
    await show([FORTRESS, SQUARE, PARK], false);

    expect(router.calls).toBe(0);
    expect(count('.leaflet-overlay-pane path')).toBe(0);
    expect(lengths.at(-1)).toBeNull();
  });

  it('asks for a walking path through the locations in order and reports its length (P3, P4)', async () => {
    await show([FORTRESS, SQUARE, PARK, BEACH], true);

    const stops = router.receivedStops.map((stop) => [stop.lat, stop.lng]);
    expect(stops).toEqual([
      [FORTRESS.latitude, FORTRESS.longitude],
      [SQUARE.latitude, SQUARE.longitude],
      [PARK.latitude, PARK.longitude],
      [BEACH.latitude, BEACH.longitude],
    ]);
    expect(count('.leaflet-overlay-pane path')).toBeGreaterThan(0);
    expect(lengths.at(-1)).toBe(3.2);
  });

  it('shows a message and reports no length when a new path cannot be created (P5)', async () => {
    await show([FORTRESS, SQUARE], true);
    expect(lengths.at(-1)).toBe(3.2);

    router.fails = true;
    await show([PARK, BEACH], true);

    expect(fixture.nativeElement.textContent).toContain('Path could not be created');
    expect(lengths.at(-1)).toBeNull();
  });

  it('shows that the path is loading while the server has not answered (P8)', async () => {
    router.holding = true;
    await show([FORTRESS, SQUARE], true);

    expect(fixture.nativeElement.textContent).toContain('Loading path...');
    expect(lengths.at(-1)).toBeNull();
  });

  it('draws no path with fewer than two locations (P6)', async () => {
    await show([FORTRESS], true);

    expect(router.calls).toBe(0);
    expect(count('.leaflet-overlay-pane path')).toBe(0);
    expect(lengths.at(-1)).toBeNull();
  });
  it('does not ask for a new path when the same stops arrive in a new array', async () => {
    await show([FORTRESS, SQUARE], true);

    await show([{ ...FORTRESS }, { ...SQUARE }], true);

    expect(router.calls).toBe(1);
    expect(lengths.at(-1)).toBe(3.2);
  });

  it('ignores the answer for an old path that arrives after the stops changed', async () => {
    router.holding = true;
    await show([FORTRESS, SQUARE], true);

    router.holding = false;
    router.distance = 5000;
    await show([PARK, BEACH], true);
    router.held.forEach((answer) => answer());
    await fixture.whenStable();

    expect(lengths.at(-1)).toBe(5);
  });

  it('can leave the page while the path is loading', async () => {
    router.holding = true;
    await show([FORTRESS, SQUARE], true);
    const before = lengths.length;

    expect(() => fixture.destroy()).not.toThrow();
    expect(() => router.held.forEach((answer) => answer())).not.toThrow();
    expect(lengths.length).toBe(before);
  });

  describe('picking a location', () => {
    let leafletMap: L.Map;
    let picks: MapLocation[];

    // Keeps the Leaflet map the component creates, so a test can click on it.
    beforeEach(() => {
      const createMap = L.map;
      vi.spyOn(L, 'map').mockImplementation((element, options) => {
        leafletMap = createMap(element, options);
        return leafletMap;
      });
      picks = [];
      fixture.componentInstance.locationPicked.subscribe((location) => picks.push(location));
    });

    async function pick(pickLocation: boolean, selectedLocation: MapLocation | null): Promise<void> {
      fixture.componentRef.setInput('pickLocation', pickLocation);
      fixture.componentRef.setInput('selectedLocation', selectedLocation);
      await fixture.whenStable();
    }

    function clickAt(location: MapLocation): void {
      leafletMap.fire('click', { latlng: L.latLng(location.latitude, location.longitude) });
    }

    function pickedPinPlace(): number[] {
      const places: number[][] = [];
      leafletMap.eachLayer((layer) => {
        if (layer instanceof L.CircleMarker) {
          places.push([layer.getLatLng().lat, layer.getLatLng().lng]);
        }
      });
      expect(places.length).toBe(1);
      return places[0];
    }

    it('does not report a click while pickLocation is off (P1)', async () => {
      await pick(false, null);

      clickAt(PARK);

      expect(picks).toEqual([]);
    });

    it('reports the latitude and longitude of the clicked place (P4)', async () => {
      await pick(true, null);

      clickAt(PARK);

      expect(picks).toEqual([{ latitude: PARK.latitude, longitude: PARK.longitude }]);
    });

    it('shows one picked pin at the given location next to the ordinary pins and moves it when the location changes (P2, P3, P5)', async () => {
      await show([FORTRESS, PARK], false);
      await pick(true, SQUARE);

      expect(count('.picked-pin')).toBe(1);
      expect(count('.leaflet-marker-icon')).toBe(2);
      expect(pickedPinPlace()).toEqual([SQUARE.latitude, SQUARE.longitude]);

      await pick(true, BEACH);

      expect(count('.picked-pin')).toBe(1);
      expect(count('.leaflet-marker-icon')).toBe(2);
      expect(pickedPinPlace()).toEqual([BEACH.latitude, BEACH.longitude]);
    });

    it('does not move the picked pin on a click until the page passes the new location', async () => {
      await pick(true, SQUARE);

      clickAt(BEACH);
      await fixture.whenStable();

      expect(count('.picked-pin')).toBe(1);
      expect(pickedPinPlace()).toEqual([SQUARE.latitude, SQUARE.longitude]);
    });
  });
});
