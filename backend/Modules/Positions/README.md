# Modul Positions

Modul Positions je platformski modul koji pamti trenutnu poziciju turiste. Ima punu strukturu od pet projekata, kao funkcionalni moduli, i projekat `Positions.Tests` sa testovima. Modul u celosti pripada platformskom timu, pa ga timovi funkcionalnih modula ne menjaju.

## Problem koji rešava

Delovi Exploration i Games zavise od toga gde se turista nalazi, na primer kada turista izvodi turu ili traži skrivenu lokaciju. Mobilna aplikacija koja bi slala pravu lokaciju uređaja još ne postoji, a timovi ne mogu da isprobaju ove funkcionalnosti šetnjom po gradu. Zato modul služi kao privremeni simulator pozicije: turista klikom na mapu odredi gde se nalazi, a server to zapamti kao njegovu trenutnu poziciju. Kada mobilna aplikacija bude postojala, ona će slati poziciju umesto klika na mapu.

Za svakog korisnika pamti se samo poslednja pozicija. Exploration i Games će kasnije čitati poziciju turiste kroz projekat `Positions.Contracts`, kada im zatreba. Kontrakt se dodaje tek na njihov zahtev, pa je projekat za sada prazan.

## Elementi modula

### Domain

Agregat `UserPosition` čuva identifikator korisnika i njegove trenutne koordinate. Metoda `MoveTo` zamenjuje stare koordinate novim. Jedinstveni indeks nad identifikatorom korisnika obezbeđuje da korisnik nema dve pozicije.

Vrednosni objekat `Coordinates` čuva geografsku širinu i dužinu i odbija širinu van opsega od -90 do 90 i dužinu van opsega od -180 do 180.

### Application

Grupa slučajeva korišćenja `PositionSimulating` ima komandu i upit. Komanda pravi poziciju kada je korisnik nema, a inače je pomera. Upit vraća samo poziciju prijavljenog korisnika, ili ništa kada je nema.

### Api

Klasa `PositionSimulatingController` ima dve krajnje tačke dostupne samo ulozi `explorer`. `GET /api/positions/mine` vraća poziciju prijavljenog korisnika, ili odgovor bez sadržaja kada je nema. `PUT /api/positions/mine` postavlja njegovu poziciju.

### Infrastructure

Modul ima sopstvenu šemu `positions` u bazi. Metoda `AddPositionsModule` registruje kontekst baze, repozitorijume i aplikacione klase i, uz metodu `AddPositionsControllers`, jedino je mesto koje `Host.Api` poziva.
