# BuildToolService (.NET 6)

Dieses Repository enthält die migrierte ASP.NET Core Web API-Version des bisherigen WCF-Dienstes.

## Änderungen
- Projekt auf .NET 6 (ASP.NET Core Web API) umgestellt und Endpunkte als REST-Controller bereitgestellt.
- Konfigurationszugriff von Win32/PInvoke auf `Microsoft.Extensions.Configuration.Ini` umgestellt; Pfade werden relativ zum Content Root aufgelöst.
- Bestehende Geschäftslogik-Module weiterverwendet und über `IBuildToolService` in `BuildController` bereitgestellt.

## Build- und Laufzeitvoraussetzungen
- .NET 6 SDK (siehe `global.json` für die erwartete Version 6.0.418 oder neuer mit Rollforward auf neuere Minor-Releases)
- Visual Studio 2022 oder `dotnet` CLI ab Version 6, damit das Ziel-Framework `net6.0` unterstützt wird.

## Build und Start
```bash
dotnet restore
dotnet build
dotnet run
```
Standardmäßig lauscht der Dienst auf `http://localhost:5000`/`https://localhost:5001` und stellt die Swagger-Oberfläche im Development-Profil bereit.
