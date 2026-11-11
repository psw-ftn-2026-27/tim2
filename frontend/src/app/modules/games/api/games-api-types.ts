export type SocialChallengeStatus = 'Draft' | 'Published' | 'Retired' | 'Archived';

export interface CreateSocialChallengeDto {
  name: string;
  start: string;
  duration: string;
  requiredParticipants: number;
  rangeInMeters: number;
  xp: number;
  completionMessage: string;
}

export interface UpdateSocialChallengeDto {
  name: string;
  start: string;
  duration: string;
  requiredParticipants: number;
  rangeInMeters: number;
  xp: number;
  completionMessage: string;
}

export interface SocialChallengeDto {
  id: string;
  name: string;
  start: string;
  duration: string;
  end: string;
  requiredParticipants: number;
  rangeInMeters: number;
  xp: number;
  completionMessage: string;
  status: SocialChallengeStatus;
}
