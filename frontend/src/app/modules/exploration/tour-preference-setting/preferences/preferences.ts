import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { serverMessage } from '../../../../shared/util/server-message';
import {
  TourDifficulty,
  TourPreferenceDto,
  TransportMode,
} from '../../api/exploration-api-types';
import { TourPreferenceSetting } from '../tour-preference-setting';

const emptyPreferences = {
  easy: false,
  moderate: false,
  hard: false,
  walking: false,
  bicycle: false,
  car: false,
  tags: '',
  applicableInSearch: false,
};

@Component({
  imports: [FormField],
  selector: 'app-preferences',
  styleUrl: './preferences.scss',
  templateUrl: './preferences.html',
})
export class Preferences {
  private readonly tourPreferenceSetting = inject(TourPreferenceSetting);

  protected readonly preferences = httpResource<TourPreferenceDto | null>(
    () => '/api/exploration/tour-preferences/mine',
  );

  protected readonly saved = computed(() =>
    this.preferences.hasValue() ? this.preferences.value() : null,
  );

  protected readonly editing = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly saveError = signal<string | null>(null);

  private readonly model = signal({ ...emptyPreferences });

  protected readonly form = form(this.model);

  protected edit(preferences: TourPreferenceDto): void {
    this.error.set(null);
    this.saveError.set(null);
    this.form().reset({
      easy: preferences.difficulties.includes('Easy'),
      moderate: preferences.difficulties.includes('Moderate'),
      hard: preferences.difficulties.includes('Hard'),
      walking: preferences.transportModes.includes('Walking'),
      bicycle: preferences.transportModes.includes('Bicycle'),
      car: preferences.transportModes.includes('Car'),
      tags: preferences.tags.join(', '),
      applicableInSearch: preferences.applicableInSearch,
    });
    this.editing.set(true);
  }

  protected cancel(): void {
    this.saveError.set(null);
    this.editing.set(false);
  }

  protected async save(event: Event): Promise<void> {
    event.preventDefault();
    const values = this.model();
    const difficulties: TourDifficulty[] = [];
    if (values.easy) {
      difficulties.push('Easy');
    }
    if (values.moderate) {
      difficulties.push('Moderate');
    }
    if (values.hard) {
      difficulties.push('Hard');
    }
    const transportModes: TransportMode[] = [];
    if (values.walking) {
      transportModes.push('Walking');
    }
    if (values.bicycle) {
      transportModes.push('Bicycle');
    }
    if (values.car) {
      transportModes.push('Car');
    }
    this.pending.set(true);
    this.error.set(null);
    this.saveError.set(null);
    try {
      await this.tourPreferenceSetting.save({
        difficulties,
        transportModes,
        tags: values.tags
          .split(',')
          .map((tag) => tag.trim())
          .filter((tag) => tag !== ''),
        applicableInSearch: values.applicableInSearch,
      });
      this.preferences.reload();
      this.editing.set(false);
    } catch (failure) {
      this.saveError.set(serverMessage(failure, 'Could not save your preferences.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async delete(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.saveError.set(null);
    try {
      await this.tourPreferenceSetting.delete();
      this.preferences.reload();
      this.editing.set(false);
      this.form().reset({ ...emptyPreferences });
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete your preferences.'));
    } finally {
      this.pending.set(false);
    }
  }
}
