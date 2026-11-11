# Modul Images

Modul Images je platformski modul zadužen za otpremanje slika. Sastoji se od dva projekta. Projekat `Images` sadrži sam modul, a projekat `Images.Tests` njegove testove. Timovi funkcionalnih modula ovaj modul koriste kroz njegovu krajnju tačku i u svojim podacima čuvaju samo adresu slike koju krajnja tačka vrati. Projekat `Images` ne menjaju i ne referenciraju iz svojih modula.

## Problem koji rešava

Blog, tura i klub prikazuju fotografije koje autor dodaje sa svog uređaja. Kada bi svaki modul sam primao fajlove, svaki tim bi morao da rešava iste bezbednosne probleme: prevelike fajlove, fajlove koji se predstavljaju kao slike a nisu, i imena fajlova kojima napadač pokušava da upiše fajl van predviđenog direktorijuma. Zato o prijemu i čuvanju slika brine jedan modul, a ostali dobijaju samo adresu sačuvane slike.

## Odstupanje od strukture modula

Kao i modul Identity, modul Images ima jedan projekat sa kodom, podeljen na direktorijume koji odgovaraju slojevima. Nema bazu, agregat ni kontrakt prema drugim modulima, jer moduli ne pozivaju ovaj modul iz koda, već samo pamte adresu slike. Pet projekata bi u ovoj situaciji bila nepotrebna apstrakcija.

## Elementi projekta Images

### Api

Sadrži klasu `ImagesController` sa krajnjom tačkom `POST /api/images`, koja prima jedan fajl kao `multipart/form-data` u polju `file` i vraća `UploadedImageDto` sa adresom sačuvane slike. Krajnja tačka je dostupna samo prijavljenom korisniku. Veličina zahteva je ograničena na nešto više od 15MB, da bi fajl malo veći od dozvoljenog stigao do domenskog pravila i dobio razlog odbijanja.

Sadrži i klasu koja definiše metodu `AddImagesControllers` kojom `Host.Api` uključuje kontrolere ovog projekta.

### Core

Vrednosni objekat `ImageFile` čuva pravila fajla. Odbija fajl veći od 15MB, fajl čiji prvi bajtovi ne odgovaraju formatu JPEG, PNG ili WebP (bez obzira na ekstenziju, pa i SVG) i sliku čija širina ili visina prelazi `MaxDimensionInPixels`. Dimenzije čita iz zaglavlja samog formata, bez dodatne biblioteke.

Aplikacioni servis `ImageUploading` od primljenih bajtova pravi `ImageFile`, predaje ga sposobnosti `IImageSaver` i vraća adresu slike. Interfejs `IImageSaver` okreće zavisnost, pa servis ne zna da se slika čuva na disku.

### Infrastructure

Klasa `DiskImageSaver` implementira `IImageSaver`. Ime sačuvanog fajla generiše sama, od novog identifikatora i ekstenzije koja odgovara formatu, pa originalno ime fajla ne utiče na to gde se fajl čuva. Fajl upisuje u direktorijum `uploads` unutar web korena aplikacije (`Host.Api/wwwroot`) i vraća adresu oblika `/uploads/<ime>`. `Host.Api` taj direktorijum servira kao statičke fajlove, pa sliku na toj adresi može da otvori svako. Direktorijum `uploads` se ne čuva u repozitorijumu.

Metoda `AddImagesModule` registruje servis i sposobnost i jedino je mesto koje `Host.Api` poziva pored metode `AddImagesControllers`.

## Projekat Images.Tests

Jedinični testovi proveravaju pravila klase `ImageFile`. Integracioni testovi šalju prave HTTP zahteve krajnjoj tački i proveravaju da je slika sačuvana, da adresa ne zavisi od imena poslatog fajla i da se odbijen fajl ne čuva. Test aplikacija koristi privremeni web koren, koji se prazni pre svakog testa, pa testovi ne upisuju fajlove u `Host.Api/wwwroot`.
