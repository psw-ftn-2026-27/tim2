import {
  SchemaPathTree,
  applyEach,
  max,
  maxLength,
  min,
  minLength,
  required,
} from '@angular/forms/signals';
import { MapLocation } from '../../../shared/map/map';
import {
  CreateLocationChallengeDto,
  LocationChallengeDto,
  LocationDto,
} from '../api/games-api-types';

// The client-only model of the location challenge form, used by the create page and the
// details page. A place that is not picked on the map yet is null.
export interface LocationChallengeFormModel {
  name: string;
  xp: number;
  startLocation: LocationDto | null;
  hiddenPoints: LocationChallengeFormPoint[];
}

export interface LocationChallengeFormPoint {
  // An empty string means that no image is uploaded for the point yet.
  imageUrl: string;
  location: LocationDto | null;
}

export function locationChallengeFormSchema(
  path: SchemaPathTree<LocationChallengeFormModel>,
): void {
  required(path.name, { message: 'Name is required.' });
  minLength(path.name, 3, { message: 'Name must have at least 3 characters.' });
  maxLength(path.name, 60, { message: 'Name can have at most 60 characters.' });
  required(path.xp, { message: 'XP is required.' });
  min(path.xp, 1, { message: 'XP must be at least 1.' });
  max(path.xp, 1000, { message: 'XP can be at most 1000.' });
  required(path.startLocation, { message: 'Pick the start location on the map.' });

  applyEach(path.hiddenPoints, (point) => {
    required(point.imageUrl, { message: 'Upload a photo of the point.' });
    required(point.location, { message: 'Pick the location of the point on the map.' });
  });
}

export function emptyPoint(): LocationChallengeFormPoint {
  return { imageUrl: '', location: null };
}

export function addPoint(challenge: LocationChallengeFormModel): LocationChallengeFormModel {
  return { ...challenge, hiddenPoints: [...challenge.hiddenPoints, emptyPoint()] };
}

export function removePoint(
  challenge: LocationChallengeFormModel,
  pointIndex: number,
): LocationChallengeFormModel {
  return {
    ...challenge,
    hiddenPoints: challenge.hiddenPoints.filter((_, index) => index !== pointIndex),
  };
}

export function setStartLocation(
  challenge: LocationChallengeFormModel,
  location: LocationDto,
): LocationChallengeFormModel {
  return { ...challenge, startLocation: location };
}

export function setPointLocation(
  challenge: LocationChallengeFormModel,
  pointIndex: number,
  location: LocationDto,
): LocationChallengeFormModel {
  return changePoint(challenge, pointIndex, (point) => ({ ...point, location }));
}

export function setPointImage(
  challenge: LocationChallengeFormModel,
  pointIndex: number,
  imageUrl: string,
): LocationChallengeFormModel {
  return changePoint(challenge, pointIndex, (point) => ({ ...point, imageUrl }));
}

// The name of a place as the page and the map show it.
export function placeName(place: 'start' | number): string {
  return place === 'start' ? 'Start' : `Point ${place + 1}`;
}

// The places already picked, named for the map, except the one being picked now: the map
// draws that one as the picked pin.
export function otherPlaces(
  challenge: LocationChallengeFormModel,
  picking: 'start' | number | null,
): MapLocation[] {
  const places: MapLocation[] = [];
  if (picking !== 'start' && challenge.startLocation !== null) {
    places.push({ ...challenge.startLocation, name: placeName('start') });
  }
  challenge.hiddenPoints.forEach((point, index) => {
    if (picking !== index && point.location !== null) {
      places.push({ ...point.location, name: placeName(index) });
    }
  });
  return places;
}

export function pickedPlace(
  challenge: LocationChallengeFormModel,
  picking: 'start' | number | null,
): LocationDto | null {
  if (picking === null) {
    return null;
  }
  if (picking === 'start') {
    return challenge.startLocation;
  }
  return challenge.hiddenPoints[picking]?.location ?? null;
}

// Saving is possible only once every place is picked, so the locations are never null here.
// CreateLocationChallengeDto and UpdateLocationChallengeDto have the same fields, so the
// result is sent by both commands.
export function toLocationChallengeDto(
  challenge: LocationChallengeFormModel,
): CreateLocationChallengeDto {
  return {
    name: challenge.name,
    xp: challenge.xp,
    startLocation: challenge.startLocation!,
    hiddenPoints: challenge.hiddenPoints.map((point) => ({
      imageUrl: point.imageUrl,
      location: point.location!,
    })),
  };
}

export function toLocationChallengeFormModel(
  challenge: LocationChallengeDto,
): LocationChallengeFormModel {
  return {
    name: challenge.name,
    xp: challenge.xp,
    startLocation: challenge.startLocation,
    hiddenPoints: challenge.hiddenPoints.map((point) => ({
      imageUrl: point.imageUrl,
      location: point.location,
    })),
  };
}

function changePoint(
  challenge: LocationChallengeFormModel,
  pointIndex: number,
  change: (point: LocationChallengeFormPoint) => LocationChallengeFormPoint,
): LocationChallengeFormModel {
  return {
    ...challenge,
    hiddenPoints: challenge.hiddenPoints.map((point, index) =>
      index === pointIndex ? change(point) : point,
    ),
  };
}
