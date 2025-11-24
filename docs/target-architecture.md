# Zielarchitektur für die Migration von BuildToolService zu ASP.NET Core Web API (.NET 6)

## Ausgangslage
- Das aktuelle BuildToolService-Projekt ist ein WCF-Dienst (.NET Framework 4.5.2), der seine Geschäftslogik über die Implementierung `BuildToolService` bereitstellt. Die Service-Klasse ruft modulare Helfer auf, etwa für Auslieferungen, Groß-Migrationen, Migrationsordner, Datenbankmigrationen und Versionsaustausch.【F:BuildToolService.svc.cs†L8-L163】
- Konfigurationen werden heute aus INI-Dateien per Win32-API (`GetPrivateProfileString`) und `System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath` gelesen, wodurch eine starke Bindung an Windows/IIS besteht.【F:modules/Data.cs†L13-L182】
- Die API wird per `IBuildToolService`-Contract und `WebGet`-Attributen als REST-ähnlicher Endpunkt im WCF-Stil exponiert, was in .NET 6 nicht mehr verfügbar ist.【F:IBuildToolService.cs†L8-L68】

## Architekturziele
- Umstellung auf ASP.NET Core Web API (.NET 6) ohne WCF- oder IIS-Abhängigkeiten; ausschließlich REST/JSON.
- Integration des migrierten Moduls direkt in das bestehende EfcomDevOpsBackendAPI über Controller, Services und Dependency Injection.
- Beibehaltung der vorhandenen Geschäftslogik, jedoch mit zeitgemäßer Infrastruktur (Options-Pattern, Logging, DI, Configuration Provider).
- Klare Schichtung: API-Controller ➜ Service-Layer ➜ Helfer/Datei- und Prozesslogik ➜ Konfiguration.

## Zielstruktur
- **Controller**: `[ApiController]`-basierte Klassen im Muster `api/[controller]/[action]` oder flach `api/[controller]`. Beispiel: `BuildController` mit Aktionen für Auslieferung, Groß-Migration, Datenbankmigration, Versionsersetzung.
- **Services**: `IBuildService` (Auslieferungen), `IMigrationService` (Groß- und Testmigrationen), `IVersionService` (Versionsersetzung) kapseln die Geschäftslogik. Controller delegieren nur.
- **DTOs/Models**: Request- und Response-POCOs für jeden Endpunkt, um Parameterlisten aus dem WCF-Contract in klar benannte JSON-Objekte zu überführen.
- **Konfigurations-Provider**: Ablösung der INI- und Win32-Aufrufe durch `IConfiguration`/`Options` (z. B. `appsettings.json`). Pfade und Templates aus `config.ini` werden in eine stark typisierte `BuildToolPathsOptions` überführt; `IHostEnvironment.ContentRootPath` ersetzt `HostingEnvironment.ApplicationPhysicalPath`.
- **Helper**: Datei- und Prozessoperationen werden in wiederverwendbare Klassen ausgelagert (z. B. `FileSystemHelper`, `MigrationScriptRunner`). Bestehende Module wie `Auslieferung`, `Migrationsordner`, `Migrationsdurchfuehren` können in diese Helper/Service-Schicht überführt werden.
- **Dependency Injection**: Registrierung der Services, Helper und Optionen in `Program.cs` (`builder.Services.AddScoped<IBuildService, BuildService>()` etc.).

## Endpunktabbildung (WCF ➜ Web API)
- `AuslieferungErstellen(...)` ➜ `POST api/build/deliveries` mit JSON-Body für Auslieferung, Versionen, Kunde, TODOs und Flags.【F:BuildToolService.svc.cs†L15-L25】
- `GrossMigrationErstellen(...)` ➜ `POST api/migration/gross` mit Versionen und Kunde.【F:BuildToolService.svc.cs†L27-L38】
- `MigrationsordnerZusammenErstellen(...)` ➜ `POST api/migration/folders` mit Versions- und Datumsbereich.【F:BuildToolService.svc.cs†L40-L51】
- `DatenbankenMigrieren(...)` ➜ `POST api/migration/databases` mit Versionsspanne und Kunde.【F:BuildToolService.svc.cs†L52-L63】
- `TestDatenbankMigrieren(...)` ➜ `POST api/migration/tests` mit Versionsparametern.【F:BuildToolService.svc.cs†L64-L75】
- `VersionFuerEfErsetzen(...)` ➜ `POST api/versions/replace` mit Version und Kunde.【F:BuildToolService.svc.cs†L77-L88】
- Kundendaten-Endpunkte (`GetKundeListe`, `GetKundeTestDatenbankListe`, `GetKDatenbank`, `GetKServer`, `GetKKuerzel`) ➜ `GET api/clients`/`api/clients/test-db`/`api/clients/databases`/`api/clients/servers`/`api/clients/codes`. Diese Endpunkte greifen auf einen Service zu, der die INI-basierten Aufrufe aus `Data` ersetzt.【F:BuildToolService.svc.cs†L90-L163】【F:modules/Data.cs†L184-L195】

## Konfigurations- und Infrastrukturmigration
- **appsettings**: Alle INI-Schlüssel aus `config.ini` (z. B. `paths:grossMigration1`, `paths:auslieferungsordner`) werden in `appsettings.json` gespiegelt. Umgebungsabhängige Werte können per `appsettings.Development.json` überschrieben werden.
- **Options-Pattern**: `BuildToolPathsOptions` spiegelt die bisherigen statischen Eigenschaften (`GrossMigration1`, `Auslieferung1`, `Webservice*`, `sharedpfad64/32` usw.) wider und wird via `services.Configure<BuildToolPathsOptions>(configuration.GetSection("BuildToolPaths"))` geladen.【F:modules/Data.cs†L21-L45】【F:modules/Data.cs†L50-L175】
- **Plattformunabhängigkeit**: Win32-P/Invoke (`GetPrivateProfileString`) entfällt. Datei- und Pfadoperationen nutzen `System.IO` und `IFileProvider`. Hosting-Pfade werden über `IHostEnvironment.ContentRootPath` ermittelt.
- **Logging & Observability**: Nutzung von `ILogger<T>` im Service-Layer für alle Datei-/Prozessaktionen; strukturierte Logs mit Kontext (Kunde, Version, Pfad).

## Integrationspunkte mit EfcomDevOpsBackendAPI
- **Projektstruktur**: Einbinden als neues Modul (z. B. Projekt `EfcomDevOpsBackendAPI.BuildTools`) innerhalb der bestehenden Solution. Shared-Modelle werden in `EfcomDevOpsShared` referenziert.
- **Routing & Versionierung**: Übernahme des bestehenden API-Stils des Backend-APIs (z. B. `api/[controller]`). Optional API-Versionierung, falls im Zielprojekt vorhanden.
- **Security**: Anbindung an die vorhandene Authentifizierung/Autorisierung des Backend-APIs (z. B. JWT). Services erhalten den Benutzerkontext über `IHttpContextAccessor`, falls notwendig.
- **Deployment**: Single-Container/Single-Process-Hosting ohne IIS. Konsolidierte `appsettings`-Konfiguration innerhalb des bestehenden Backend-APIs.

## Migrationsschritte (hohes Niveau)
1. Neues ASP.NET Core Web API-Projekt (.NET 6) im Solution-Verbund anlegen oder das bestehende Backend-Projekt erweitern.
2. WCF-Contracts (`IBuildToolService`) durch Controller/DTOs ersetzen, Services/Helper implementieren und über DI registrieren.
3. Geschäftslogik aus den Modulen (`Auslieferung`, `Migrationsordner`, `Migrationsdurchfuehren`, `VersionErsetzen` etc.) in Services/Helper migrieren und `System.Web`/P/Invoke-Abhängigkeiten eliminieren.
4. INI-Konfiguration in `appsettings*.json` überführen und per Options laden; alle Pfade über `IHostEnvironment.ContentRootPath` auflösen.
5. Logging, Fehlerbehandlung und REST-konforme Statuscodes hinzufügen; Endpunkte gegen bestehende Prozesse/Funktionen testen.
6. Integration in EfcomDevOpsBackendAPI (Routing, DI, AuthN/Z) und automatisierte Builds/CI anpassen.
