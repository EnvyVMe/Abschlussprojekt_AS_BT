# BuildToolService (.NET 6)

Dieses Repository enthält die migrierte ASP.NET Core Web API-Version des bisherigen WCF-Dienstes.

## Änderungen
- Projekt auf .NET 6 (ASP.NET Core Web API) umgestellt und Endpunkte als REST-Controller bereitgestellt.
- Konfigurationszugriff von Win32/PInvoke auf `Microsoft.Extensions.Configuration.Ini` umgestellt; Pfade werden relativ zum Content Root aufgelöst.
- Bestehende Geschäftslogik-Module weiterverwendet und über `IBuildToolService` in `BuildController` bereitgestellt.
