//using BuildToolService.DataTypes;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace BuildToolService
{
    [ServiceContract]
    public interface IBuildToolService
    {
        [OperationContract]
        [WebGet(UriTemplate = "?auslieferung={as_auslieferung}&version_von={as_versionVon}&version_bis={as_versionBis}&fix_von={as_fixVon}&fix_bis={as_fixBis}&kunde={as_kunde}&neue_todo={as_neueTodo}&ef_online_ordner_erstellen={as_ef3ordnerErstellen}&installationsroutine_erstellen={as_installationsroutineErstellen}&unterfixe_beruecksichtigen={as_unterfixeBeruecksichtigen}&auslieferung_lokal_erstellen={as_lokalErstellen}&woechentlichen_build={as_woechentlichenBuild}&32bit={as_32bit}&internAuslieferung={as_internAuslieferung}")]
        string AuslieferungErstellen(string as_auslieferung, string as_versionVon, string as_versionBis, string as_fixVon, string as_fixBis, string as_kunde, string as_installationsroutineErstellen, string as_unterfixeBeruecksichtigen, string as_neueTodo, string as_ef3ordnerErstellen, string as_lokalErstellen, string as_woechentlichenBuild, string as_32bit, string as_internAuslieferung);

        [OperationContract]
        [WebGet(UriTemplate = "grossmigration?version_von={as_versionVon}&version_bis={as_versionBis}&kunde={as_kunde}")]
        string GrossMigrationErstellen(string as_versionVon, string as_versionBis, string as_kunde);

        [OperationContract]
        [WebGet(UriTemplate = "migrationsordner?version_von={as_versionVon}&version_bis={as_versionBis}&datum_von={as_datum_von}&datum_bis={as_datum_bis}")]
        string MigrationsordnerZusammenErstellen(string as_versionVon, string as_versionBis, string as_datum_von, string as_datum_bis);

        [OperationContract]
        [WebGet(UriTemplate = "migrationdurchfuehren?version_von={as_versionVon}&version_bis={as_versionBis}&kunde={as_kunde}")]
        string DatenbankenMigrieren(string as_versionVon, string as_versionBis, string as_kunde);

        [OperationContract]
        [WebGet(UriTemplate = "testmigrationdurchfuehren?version_von={as_versionVon}&version_bis={as_versionBis}")]
        string TestDatenbankMigrieren(string as_versionVon, string as_versionBis);

        [OperationContract]
        [WebGet(UriTemplate = "versionersetzen?version={as_version}&kunde={as_kunde}")]
        string VersionFuerEfErsetzen(string as_version, string as_kunde);

        [OperationContract]
        [WebGet(UriTemplate = "get_kundenliste")]
        string[] GetKundeListe();

        [OperationContract]
        [WebGet(UriTemplate = "get_kundetestdatenbankliste")]
        string[] GetKundeTestDatenbankListe();

        [OperationContract]
        [WebGet(UriTemplate = "get_kdatenbank")]
        string[] GetKDatenbank();

        [OperationContract]
        [WebGet(UriTemplate = "get_kserver")]
        string[] GetKServer();

        [OperationContract]
        [WebGet(UriTemplate = "get_kkuerzel")]
        string[] GetKKuerzel();



    }
}
