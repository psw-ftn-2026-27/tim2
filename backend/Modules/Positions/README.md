# Modul Positions

Modul Positions je platformski modul koji pamti trenutnu poziciju turiste. Ima punu strukturu od pet projekata, kao funkcionalni moduli, i projekat `Positions.Tests` sa testovima. Modul u celosti pripada platformskom timu, pa ga timovi funkcionalnih modula ne menjaju.

## Problem koji rešava

Delovi Exploration i Games zavise od toga gde se turista nalazi, na primer kada turista izvodi turu ili traži skrivenu lokaciju. Mobilna aplikacija koja bi slala pravu lokaciju uređaja još ne postoji, a timovi ne mogu da isprobaju ove funkcionalnosti šetnjom po gradu. Zato modul služi kao privremeni simulator pozicije: turista klikom na mapu odredi gde se nalazi, a server to zapamti kao njegovu trenutnu poziciju. Kada mobilna aplikacija bude postojala, ona će slati poziciju umesto klika na mapu.

Za svakog korisnika pamti se samo poslednja pozicija, bez vremena kada je postavljena. Exploration i Games čitaju poziciju turiste kroz projekat `Positions.Contracts`, a front-end im nikada ne šalje koordinate.

## Elementi modula

### Domain

Agregat `UserPosition` čuva identifikator korisnika i njegove trenutne koordinate. Metoda `MoveTo` zamenjuje stare koordinate novim. Jedinstveni indeks nad identifikatorom korisnika obezbeđuje da korisnik nema dve pozicije.

Vrednosni objekat `Coordinates` čuva geografsku širinu i dužinu i odbija širinu van opsega od -90 do 90 i dužinu van opsega od -180 do 180.

### Application

Grupa slučajeva korišćenja `PositionSimulating` ima komandu i upit. Komanda pravi poziciju kada je korisnik nema, a inače je pomera. Upit vraća samo poziciju prijavljenog korisnika, ili ništa kada je nema.

DTO `UserPositionDto` je u folderu `UserPositions`, jer ga koriste i grupa `PositionSimulating` i klasa `PositionsApi`.

Klasa `PositionsApi` implementira kontrakt `IPositionsApi` i čita poziciju kroz isti repozitorijum za čitanje kao upit.

### Contracts

Interfejs `IPositionsApi` ima jedan upit. Metoda `GetPositionAsync` vraća poslednju poziciju korisnika po identifikatoru, kao zapis `PositionSummary` sa geografskom širinom i dužinom, ili `null` kada korisnik još nije postavio poziciju. Projekte `Exploration.Application` i `Games.Application` platforma je povezala sa ovim projektom, pa modul samo doda `IPositionsApi` u konstruktor svog servisa ili upita.

Modul koji koristi kontrakt sam računa udaljenost i sam odlučuje šta je „blizu“. Kontrakt ne kaže kada je pozicija postavljena i ne vraća pozicije više korisnika odjednom. Novi član se dodaje samo na zahtev platformskom timu.

### Testovi drugih modula

Test modula koji koristi kontrakt ne postavlja poziciju pozivom `PUT /api/positions/mine`, jer bi greška u modulu Positions tada obarala i njegove testove. Umesto toga, pre svakog testa pozove `PositionsReseed.Apply(Factory)` u svom `BaseIntegrationTest.cs`, pa `IPositionsApi` vraća gotove pozicije iz `UserPositionSeed`. Za sada je ovo povezano za `Games.Tests` i `Exploration.Tests`. Pozicija igrača `ThirdExplorer` je u centru kviza `PublishedSynagogue`, pozicija igrača `Explorer` je daleko od svih kvizova, a igrač bez pozicije je novi korisnik sa `Guid.NewGuid()`.

### Api

Klasa `PositionSimulatingController` ima dve krajnje tačke dostupne samo ulozi `explorer`. `GET /api/positions/mine` vraća poziciju prijavljenog korisnika, ili odgovor bez sadržaja kada je nema. `PUT /api/positions/mine` postavlja njegovu poziciju.

### Infrastructure

Modul ima sopstvenu šemu `positions` u bazi. Metoda `AddPositionsModule` registruje kontekst baze, repozitorijume i aplikacione klase i, uz metodu `AddPositionsControllers`, jedino je mesto koje `Host.Api` poziva.
