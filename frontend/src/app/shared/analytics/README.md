# Analitički događaji

`shared/analytics` je jedini način da modul zabeleži događaj u Google Analytics 4.
Servis `Analytics` sam proverava da li je korisnik prijavljen, da li je dao saglasnost i
da li je ID za merenje podešen. Ako bilo šta od toga nije ispunjeno, događaj se tiho
odbacuje, a stranica radi kao i inače. Modul zato nikada ne proverava saglasnost sam.

Pravila:

- Modul nikada ne poziva `gtag`, ne učitava GA skriptu i ne piše u `dataLayer`. Sve ide
  kroz `logEvent` servisa `Analytics`.
- Svaki događaj postoji u katalogu `analytics-events.ts`, sa imenom i tačno onim
  parametrima koje sme da nosi.
- Ulogu korisnika (`user_role`) dodaje servis. Modul je ne prosleđuje.

## Kako se dodaje događaj u katalog

Katalog je tip `AnalyticsEvent` u `analytics-events.ts`. Novi događaj je još jedan član
unije:

```ts
export type AnalyticsEvent =
  | { name: 'tour_published'; params: { tour_id: string } }
  | { name: '<predmet>_<radnja>'; params: { <parametar>: <tip> } };
```

Ime događaja ima format `<predmet>_<radnja u prošlom vremenu>`, malim slovima sa donjom
crtom: `tour_published`, a ne `publishTour` ni `tour_publish`.

Parametar sme da bude samo:

- identifikator sadržaja (ID ture, izazova, proizvoda), kao `string`, ili
- vrednost iz zatvorenog skupa, kao unija string literala (`'quiz' | 'hidden_location'`).

Parametar nikada nije ID korisnika, ime, mejl ni slobodan tekst koji je korisnik uneo
(naslov, opis, komentar). Ako niste sigurni da li parametar sme, pitajte platformski tim
pre nego što ga dodate.

Izmena kataloga je izmena deljenog koda, pa je pregleda platformski tim.

## Kako se beleži događaj sa stranice

Stranica injektuje `Analytics` i poziva `logEvent` tek kada server potvrdi komandu,
odmah posle `await`. U `catch` grani se ne beleži ništa, jer neuspela komanda nije
događaj.

```ts
private readonly analytics = inject(Analytics);

protected async publish(tourId: string): Promise<void> {
  this.pending.set(true);
  this.error.set(null);
  try {
    await this.tourAuthoring.publish(tourId);
    this.analytics.logEvent({ name: 'tour_published', params: { tour_id: tourId } });
    this.tours.reload();
  } catch (failure) {
    this.error.set(serverMessage(failure, 'Could not publish the tour.'));
  } finally {
    this.pending.set(false);
  }
}
```

TypeScript proverava poziv: ime mora postojati u katalogu, a parametri moraju biti tačno
oni navedeni uz to ime.

## Provera

Razvojna konfiguracija (`src/environments/environment.development.ts`) već sadrži ID za
merenje zajedničkog razvojnog GA4 svojstva, pa za proveru nije potreban nikakav nalog.
ID nije tajna: njime se podaci samo šalju, a čitanje traži pristup svojstvu.

Nazivi su dati na srpskom i engleskom, jer Chrome prati jezik sistema.

1. Pokrenite aplikaciju (`npm start`), prijavite se i kliknite "Accept".
2. Otvorite alate pregledača (F12) i karticu **Mreža** (*Network*).
3. U polje **Filtriraj** (*Filter*) upišite `collect`, a desno od njega izaberite tip
   **Sve** (*All*). GA šalje događaje kao *ping*, pa se ne vide pod *Fetch/XHR*.
4. Uradite radnju na stranici. Ako je događaj poslat, pojavi se red `collect?v=2&...`.
   Kliknite na njega i otvorite karticu **Korisni teret** (*Payload*): `en` je ime
   događaja, `ep.<parametar>` su njegovi parametri, a `ep.user_role` je uloga. Ne sme biti
   mejla ni ID-ja korisnika.
5. Posle "Decline" (u podnožju, "Cookie settings") ili bez prijave lista mora ostati
   prazna.

### Ako ne vidite događaj

- **Nema nijednog reda `collect`.** Zaustavite `npm start` (`Ctrl+C`) i pokrenite ga
  ponovo, pa osvežite stranicu. Zatim u filter upišite `gtag`: red
  `gtag/js?id=G-...` mora imati status 200. Ako je crven (`blocked`,
  `ERR_BLOCKED_BY_CLIENT`), skriptu blokira dodatak za reklame (uBlock, AdBlock) ili
  pregledač Brave; isključite ga za `localhost`.
- **Ima reda `collect`, ali u njemu nema vašeg događaja.** GA ponekad spoji više
  događaja u jedan zahtev. Tada ime nije u **Parametri niza upita** (*Query String
  Parameters*), već u odeljku **Korisni teret zahteva** (*Request Payload*), gde je svaki
  događaj jedan red (`en=tour_published&ep.tour_id=...`). Pogledajte i ostale redove
  `collect`, jer prvi ponekad nosi samo GA-ov tehnički događaj (`user_engagement`).
- **I dalje ništa.** U kartici **Konzola** (*Console*) upišite `dataLayer`. Ako je
  `undefined`, `logEvent` nije stigao do slanja: proverite da ste prijavljeni, da ste
  kliknuli "Accept" i da se poziv nalazi posle `await` komande.

Ko treba da vidi događaje i u GA4 DebugView, traži od platformskog tima pristup
razvojnom svojstvu sa ulogom "Viewer".
