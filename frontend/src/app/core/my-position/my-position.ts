import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { UserPositionDto } from '../../shared/api/user-position';
import { MapLocation, MapView } from '../../shared/map/map';
import { Positions } from '../../shared/positions/positions';

@Component({
  imports: [MapView],
  selector: 'app-my-position',
  templateUrl: './my-position.html',
})
export class MyPosition {
  private readonly positions = inject(Positions);

  // The server answers 204 while the user has no position, which arrives here as null.
  protected readonly position = httpResource<UserPositionDto | null>(() => '/api/positions/mine');

  // The pin shows the position the server keeps, so it moves only after the server confirms.
  protected readonly youAreHere = computed<MapLocation | null>(() => {
    const position = this.position.hasValue() ? this.position.value() : null;
    if (position === null) {
      return null;
    }
    return { latitude: position.latitude, longitude: position.longitude, name: 'You are here' };
  });

  protected readonly saving = signal(false);
  protected readonly saved = signal(false);
  protected readonly failed = signal(false);

  protected async save(location: MapLocation): Promise<void> {
    // One request at a time, so a click while saving is ignored.
    if (this.saving()) {
      return;
    }
    this.saving.set(true);
    this.saved.set(false);
    this.failed.set(false);
    try {
      await this.positions.save({ latitude: location.latitude, longitude: location.longitude });
      this.position.reload();
      this.saved.set(true);
    } catch {
      this.failed.set(true);
    } finally {
      this.saving.set(false);
    }
  }
}
