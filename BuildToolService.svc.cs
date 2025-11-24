using System;
using System.Collections.Generic;
using System.Linq;


namespace BuildToolService
{
    public class BuildToolService : IBuildToolService
    {
        public BuildToolService()
        {
            Data.Init();
        }

        public string AuslieferungErstellen(string as_auslieferung, string as_versionVon, string as_versionBis, string as_fixVon, string as_fixBis, string as_kunde, string as_neueTodo, string as_ef3ordnerErstellen, string as_installationsroutineErstellen, string as_unterfixeBeruecksichtigen, string as_lokalErstellen, string as_woechentlichenBuild, string as_32bit, string as_internAuslieferung)
        {
            try
            {
                return Auslieferung.Erstellen(as_auslieferung, as_versionVon, as_versionBis, as_fixVon, as_fixBis, as_kunde, as_neueTodo, as_ef3ordnerErstellen, as_installationsroutineErstellen, as_unterfixeBeruecksichtigen, as_lokalErstellen, as_woechentlichenBuild, as_32bit, as_internAuslieferung);
            }
            catch (ArgumentException arge)
            {
            return arge.Message;
            }
        }

        public string GrossMigrationErstellen(string as_versionVon, string as_versionBis, string as_kunde)
        {
             try
            {
            return GrossMigration.OrdnerErstellen(as_versionVon, as_versionBis, as_kunde);
            }
              catch(ArgumentException arge)
            {
                  return arge.Message;
            }

        }

        public string MigrationsordnerZusammenErstellen(string as_versionVon, string as_versionBis, string as_datum_von, string as_datum_bis)
        {
            try
            {
                return Migrationsordner.MigrationsordnerErstellen(as_versionVon, as_versionBis, as_datum_von, as_datum_bis);
            }
            catch (ArgumentException arge)
            {
                return arge.Message;
            }

        }
        public string DatenbankenMigrieren(string as_versionVon, string as_versionBis,string as_kunde)
        {
            try
            {
                return Migrationsdurchfuehren.DbMigrieren(as_versionVon, as_versionBis, as_kunde);
            }
            catch (ArgumentException arge)
            {
                return arge.Message;
            }

        }
        public string TestDatenbankMigrieren(string as_versionVon, string as_versionBis)
        {
            try
            {
                return TestMigrationDurchfuehren.TestDbMigrieren(as_versionVon, as_versionBis);
            }
            catch (ArgumentException arge)
            {
                return arge.Message;
            }

        }

        public string VersionFuerEfErsetzen(string as_version, string as_kunde)
        {
            try
            {
                return VersionErsetzen.EfVersionErsetzen(as_version, as_kunde);
            }
            catch (ArgumentException arge)
            {
                return arge.Message;
            }

        }

        // Gibt Kunden Liste zurück
        public string[] GetKundeListe()
        {
            string[] kunden = Data.GetClients();
            Array.Sort(kunden);
            //string[] kunden = Data.GetClients();
            //List<string> list = kunden.Cast<string>().ToList();
            //list.Sort();
            //string[] retval = list.ToArray<string>();
            return kunden;
        }

        // gibt die Kunden-Liste zurück, die Test-datenabank haben.
        public string[] GetKundeTestDatenbankListe()
        {
            string[] kundenTDB = Data.GetClientTestDatenbank();
            Array.Sort(kundenTDB);
            return kundenTDB;
        }

        // gibt eine Liste zurück für die Test-Datenbank 
        public string[] GetKDatenbank()
        {
            string[] kundenTDB = Data.GetClientTestDatenbank();
            Array.Sort(kundenTDB);
            string[] las_datenabnk;
            List<string> list = new List<string>();
            foreach (string kunde in kundenTDB)
            {
                string datenbank = Data.GetKundenDatabase(kunde);
                list.Add(datenbank);
 
            }
            las_datenabnk = list.ToArray();
            
            return las_datenabnk;

        }

        // gibt eine Liste zurück für den Server 
        public string[] GetKServer()
        {
            string[] kundenTDB = Data.GetClientTestDatenbank();
            Array.Sort(kundenTDB);
            string[] las_server;
            List<string> list = new List<string>();
            foreach (string kunde in kundenTDB)
            {
                string server = Data.GetKundenServer(kunde);
                list.Add(server);

            }
            las_server = list.ToArray();
            return las_server;

        }

        // gibt eine Liste zurück für den Kürzel  
        public string[] GetKKuerzel()
        {
            string[] kundenTDB = Data.GetClientTestDatenbank();
            Array.Sort(kundenTDB);
            string[] las_kuerzel;
            List<string> list = new List<string>();
            foreach (string kunde in kundenTDB)
            {
                string kuerzel = Data.GetKundenKuerzel(kunde);
                list.Add(kuerzel);

            }
            las_kuerzel = list.ToArray();
            return las_kuerzel;

        }
    }

    

}
