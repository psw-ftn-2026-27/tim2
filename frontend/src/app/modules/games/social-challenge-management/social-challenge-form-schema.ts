import { SchemaPathTree, max, maxLength, min, minLength, required } from '@angular/forms/signals';
import { MapLocation } from '../../../shared/map/map';
import { CreateSocialChallengeDto, SocialChallengeDto } from '../api/games-api-types';

// The client-only model of the social challenge form, used by the create page and the details
// page. The start is a time as the time input gives it (HH:mm), and the duration is in minutes.
// The location is kept apart, in its own signal of each page, because it is picked on the map.
export interface SocialChallengeFormModel {
  name: string;
  start: string;
  durationMinutes: number;
  requiredParticipants: number;
  rangeInMeters: number;
  xp: number;
  completionMessage: string;
}

export function emptySocialChallenge(): SocialChallengeFormModel {
  return {
    name: '',
    start: '',
    durationMinutes: 60,
    requiredParticipants: 2,
    rangeInMeters: 100,
    xp: 10,
    completionMessage: '',
  };
}

export function socialChallengeFormSchema(path: SchemaPathTree<SocialChallengeFormModel>): void {
  required(path.name, { message: 'Name is required.' });
  minLength(path.name, 3, { message: 'Name must have at least 3 characters.' });
  maxLength(path.name, 60, { message: 'Name can have at most 60 characters.' });
  required(path.start, { message: 'Start time is required.' });
  required(path.durationMinutes, { message: 'Duration is required.' });
  min(path.durationMinutes, 15, { message: 'Duration must be at least 15 minutes.' });
  max(path.durationMinutes, 1439, { message: 'Duration must be shorter than 24 hours.' });
  required(path.requiredParticipants, { message: 'Number of people is required.' });
  min(path.requiredParticipants, 2, { message: 'At least 2 people are required.' });
  max(path.requiredParticipants, 100, { message: 'At most 100 people can be required.' });
  required(path.rangeInMeters, { message: 'Range is required.' });
  min(path.rangeInMeters, 10, { message: 'Range must be at least 10 meters.' });
  max(path.rangeInMeters, 1000, { message: 'Range can be at most 1000 meters.' });
  required(path.xp, { message: 'XP is required.' });
  min(path.xp, 1, { message: 'XP must be at least 1.' });
  max(path.xp, 1000, { message: 'XP can be at most 1000.' });
  required(path.completionMessage, { message: 'Completion message is required.' });
  maxLength(path.completionMessage, 200, {
    message: 'Completion message can have at most 200 characters.',
  });
}

// CreateSocialChallengeDto and UpdateSocialChallengeDto have the same fields, so the result is
// sent by both commands. The server reads times as HH:mm:ss.
export function toSocialChallengeDto(
  challenge: SocialChallengeFormModel,
  location: MapLocation | null,
): CreateSocialChallengeDto {
  const hours = String(Math.floor(challenge.durationMinutes / 60)).padStart(2, '0');
  const minutes = String(challenge.durationMinutes % 60).padStart(2, '0');
  return {
    name: challenge.name,
    start: `${challenge.start}:00`,
    duration: `${hours}:${minutes}:00`,
    requiredParticipants: challenge.requiredParticipants,
    rangeInMeters: challenge.rangeInMeters,
    xp: challenge.xp,
    completionMessage: challenge.completionMessage,
    location: location ? { latitude: location.latitude, longitude: location.longitude } : null,
  };
}

export function toSocialChallengeFormModel(challenge: SocialChallengeDto): SocialChallengeFormModel {
  const [hours, minutes] = challenge.duration.split(':').map(Number);
  return {
    name: challenge.name,
    start: challenge.start.slice(0, 5),
    durationMinutes: hours * 60 + minutes,
    requiredParticipants: challenge.requiredParticipants,
    rangeInMeters: challenge.rangeInMeters,
    xp: challenge.xp,
    completionMessage: challenge.completionMessage,
  };
}
