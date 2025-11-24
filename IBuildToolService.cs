namespace BuildToolService
{
    public interface IBuildToolService
    {
        string AuslieferungErstellen(string as_auslieferung, string as_versionVon, string as_versionBis, string as_fixVon, string as_fixBis, string as_kunde, string as_installationsroutineErstellen, string as_unterfixeBeruecksichtigen, string as_neueTodo, string as_ef3ordnerErstellen, string as_lokalErstellen, string as_woechentlichenBuild, string as_32bit, string as_internAuslieferung);

        string GrossMigrationErstellen(string as_versionVon, string as_versionBis, string as_kunde);

        string MigrationsordnerZusammenErstellen(string as_versionVon, string as_versionBis, string as_datum_von, string as_datum_bis);

        string DatenbankenMigrieren(string as_versionVon, string as_versionBis, string as_kunde);

        string TestDatenbankMigrieren(string as_versionVon, string as_versionBis);

        string VersionFuerEfErsetzen(string as_version, string as_kunde);

        string[] GetKundeListe();

        string[] GetKundeTestDatenbankListe();

        string[] GetKDatenbank();

        string[] GetKServer();

        string[] GetKKuerzel();
    }
}
