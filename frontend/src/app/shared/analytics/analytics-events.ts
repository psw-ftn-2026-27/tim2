// The catalog of every event a module may log. Each member names one event and
// exactly the params it carries. A param is a content identifier or a value from a
// closed set, never a user id, name, email or free text. The user's role is added
// by the Analytics service, so no event lists it. See README.md next to this file.
export type AnalyticsEvent =
  | { name: 'tour_published'; params: { tour_id: string } };
