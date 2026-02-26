Flådesystem v1.0 (Studieprojekt)
=================================

Kort beskrivelse
----------------
Dette er et simpelt konsolbaseret administrationssystem udviklet som et studieprojekt af gruppen (CCDL). Vi har implementeret følgende funktioner:

- Login (tjekker `admin`-tabellen i PostgreSQL)
- Vis/tilføj/slet kunder
- Vis/tilføj/slet biler
- Opret/slet udlejninger (koblet til kunder og biler)
- Vis/slet salg
- Vis/slet serviceposter

Hvordan køre projektet
----------------------
1. Åbn løsningen i Microsoft Visual Studio (mål: .NET Framework 4.7.2).
2. Sørg for at `Npgsql` og `Spectre.Console` er installeret (projektet bruger dem allerede via NuGet).
3. Start programmet.

Test-login
----------
- Brugernavn: `ccdl`
- Kodeord: `12345678`

Bemærk: vi har oprettet test-admin i databasen til bedømmelse. I produktion skal kodeord opbevares sikkert (ikke i klartekst).

Database og schema
-------------------
Programmet forventer en PostgreSQL database med tabellerne: `admin`, `customers`, `cars`, `rentals`, `sales`, `service_center`, `car_sold`.
Nogle kolonner i `cars` bruger quoted camelCase (fx `"carId"`) i SQL fordi de blev oprettet med anførselstegn i DB.
Hvis din database bruger andre kolonnenavne, tilpas SQL i kildekoden eller opret de forventede kolonner.

Hvilke filer er vigtige
-----------------------
- `Program.cs` - hovedprogrammet og konsolmenuer (her findes de fleste CRUD-kommandoer)
- `AdminAuth.cs` - simpel autentificering mod `admin`-tabellen
- `Customers.cs`, `Cars.cs`, `Rentals.cs`, `Sales.cs`, `ServiceCenter.cs`, `CarSold.cs` - model/repository-klasser

Sikkerhed og kendte begrænsninger
--------------------------------
- Password for admin er i klartekst i databasen (tilladt til dette studieprojekt). Dette er ikke sikkert i produktion.
- Der er ingen avanceret input-validering; vær forsigtig ved manuel databasemanipulation.


Kontakt
-------
- Repository: https://github.com/danrogit/SIBDAT25-2S-Databaser-CCDL


