using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Globalization;

namespace BuildToolService
{
    public class Migrationsordner
    {
        public static string MigrationsordnerErstellen(string as_versionVon, string as_versionBis, string as_datum_von, string as_datum_bis)
        {
            #region Eingabenprüfung

            // Version (x.x.xxx) + Kunde vorhanden + Migrationsordner vorhanden
            Regex regex = new Regex(@"\A[0-9]\.[0-9]\.[0-9]{3}\z");
            if (!regex.IsMatch(as_versionVon))
            {
                throw new ArgumentException("Version: \"" + as_versionVon + "\" ist ungültig!", "version");
            }
            if (!regex.IsMatch(as_versionBis))
            {
                throw new ArgumentException("Version: \"" + as_versionBis + "\" ist ungültig!", "version");
            }

            string[] las_VersionVon = as_versionVon.Split('.');
            string[] las_VersionBis = as_versionBis.Split('.');

            int versionVon = int.Parse(las_VersionVon[0] + las_VersionVon[1] + las_VersionVon[2]);
            int versionBis = int.Parse(las_VersionBis[0] + las_VersionBis[1] + las_VersionBis[2]);
            if (versionBis > versionVon)
            {
                if (!(versionVon + 1 == versionBis))
                {
                    if (!(las_VersionBis[2] == "001"))
                    {
                        throw new ArgumentException("Von Version " + as_versionVon + " und Bis Version " + as_versionBis + " sind nicht hintereinander!", "version");
                    }

                }
            }
            else
            {
                throw new ArgumentException("Von Version: " + as_versionBis + " is kleine als: " + as_versionVon + " sind nicht hintereinander!", "version");
            }

            // Datum Format Prüfen 
            DateTime datum;
            CultureInfo enUS = new CultureInfo("en-US");
            string format = "dd.MM.yyyy HH:mm:ss";
            bool istDatumVon = DateTime.TryParseExact(as_datum_von, format, enUS, DateTimeStyles.None, out datum);
            bool istDatumBis = DateTime.TryParseExact(as_datum_bis, format, enUS, DateTimeStyles.None, out datum);
                if (istDatumVon)
                {

                }
                else
                {
                throw new ArgumentException("Datum " + as_datum_von + " ist ungültig! Bitt das Datum in dem Format dd.mm.yyyy hh:mm:ss eingeben.", "Datum");
                }
                if (istDatumBis)
                {

                }
                else
                {
                throw new ArgumentException("Datum " + as_datum_bis + " ist ungültig! Bitt das Datum in dem Format dd.mm.yyyy hh:mm:ss eingeben.", "Datum");
                }

            #endregion

                 string migrationsOrdner = Data.GrossMigration5 + Data.GrossMigration1 + las_VersionBis[0] + "." + las_VersionBis[1] + "\\" + Data.GrossMigration2 + as_versionBis;
                //L:\ef3\4.2\Datenbank\Migration\4.2.048
                 string migrationsDateien = Data.GrossMigration5 + Data.GrossMigration1 + las_VersionBis[0] + "." + las_VersionBis[1] + "\\Datenbank\\";
                //L:\ef3\4.2\Datenbank\Migration\



            if (!Directory.Exists(Path.Combine(migrationsOrdner)))
            {
                throw new ArgumentException("Pfad existiert nicht: " + Path.Combine(migrationsOrdner, "ordner"));
            }
            if (!Directory.Exists(Path.Combine(migrationsDateien)))
            {
                throw new ArgumentException("Pfad existiert nicht: " + Path.Combine(migrationsDateien, "ordner"));
            }

            #region Schreibschutz entfernen
            SchreibschutzEntfernen(migrationsOrdner);
            #endregion

            #region dateien löschen 
            string[] las_entferneEinzel = Data.GetEinzeldateien();
            string [] las_entferneTyp = Data.GetDateitypen();
            string [] las_ausnahmen = Data.GetAusnahmen();

            //einzeldateien
            foreach (var einzelDatei in las_entferneEinzel)
            {
                if (File.Exists(Path.Combine(migrationsOrdner + "\\" + einzelDatei)))
                {
                    File.Delete(Path.Combine(migrationsOrdner + "\\" + einzelDatei));
                }
            }
            //dateitypen
            var dir = new DirectoryInfo(migrationsOrdner);
            foreach (var dateityp in las_entferneTyp)
            {
                foreach (var file in dir.EnumerateFiles(dateityp))
                {
                    foreach (var ausnahmetyp in las_ausnahmen)
                    {
                        string pattern = ausnahmetyp;
                        string filecheck = file.ToString();
                        string regexpattern = String.Format(@"^{0}$", pattern.Replace("*", ".*"));
                        //string regexPattern = pattern.Replace("*");
                        if (!Regex.IsMatch(filecheck, regexpattern))
                        {
                            File.Delete(Path.Combine(migrationsOrdner + @"\" + file));
                        }
                    }
                }
            }

            #endregion
            #region Dateien kopieren


            string[] mussDateien = Data.GetMussdateien();
            foreach (var file in mussDateien)
            {
                string file_to_copy = migrationsDateien + "\\" + file;
                string file_destination = migrationsOrdner + "\\" + file;

                if (File.Exists(Path.Combine(file_to_copy)))
                {
                    File.Copy(file_to_copy, file_destination,true);
                }
                else
                {
                    throw new ArgumentException("Mussdatei existiert nicht: " + Path.Combine(file_destination, "ordner"));
                }
            }
            #endregion

            CreateMigrationKFM(as_versionVon, as_versionBis, as_datum_von, as_datum_bis, migrationsOrdner);
            MigrationsskriptErzeugen(as_versionVon, as_versionBis, as_datum_von, as_datum_bis, migrationsOrdner, migrationsDateien);
            CreateToDo(migrationsOrdner, as_versionVon, as_versionBis);
            CreatePostMigration(migrationsOrdner, as_versionVon, as_versionBis);
            //SteuerDateiEntw(migrationsOrdner, as_versionVon, as_versionBis);

            return "Migrationsordner für die Version " + as_versionBis+ " erfolgreich erstellt und zu finden unter: " + migrationsOrdner;
        }

        private static void MigrationsskriptErzeugen(string as_versionVon, string as_versionBis, string as_datum_von, string as_datum_bis, string migrationsOrdner, string alterPfad) {
            string[] las_vonVersion = as_versionVon.Split('.');
            string[] las_bisVersion = as_versionBis.Split('.');
            string[] las_dateien, las_reste;
            string alterPath = Path.Combine(alterPfad  + "\\alter.sql");

            string dateiName = "migration_" + las_vonVersion[2] + "_" + las_bisVersion[2] + ".sql";

            List<string> migrationsdateiText = new List<string>()
            {
                "-- Ausführung aller Veränderungen im Zeitraum",
                "-- " + as_datum_von + " bis " + as_datum_bis,
                "",
                "set echo on",
                "set serveroutput on",
                "",
                "WHENEVER SQLERROR EXIT",
                "WHENEVER OSERROR EXIT",
                "",
                "@check_db_version.sql '" + as_versionVon + ", " + as_versionBis + "'",
                "@export_schema_stats.sql 'migration_" + las_vonVersion[2] + "_" + las_bisVersion[2] + "'",
                "",
                "WHENEVER SQLERROR CONTINUE",
                "WHENEVER OSERROR CONTINUE",
                "",
                "@init_special_character.sql",
                "",
                "exec ef_utilities.enable_hist_triggers(FALSE)",
                "",
                "",
            };

            #region alter_zusammenschneiden
           
            bool abbruch = false;

            if (File.Exists(alterPath))
            {
                string line;
                System.IO.StreamReader alter = new System.IO.StreamReader(alterPath, Encoding.Default);
                try
                {
                    while ((line = alter.ReadLine()) != null)
                    {
                        if (abbruch) { break; }
                        //finde startabschnitt
                        if (line.Contains("parameter_wert = '" + as_versionVon + "'"))
                        {
                            //ans ende des startabschnittes springen
                            for (int i = 0; i < 14; i++)
                            {
                                alter.ReadLine();
                            }
                            while (!abbruch)
                            {
                                while ((line = alter.ReadLine()) != null)
                                {
                                    //SCHREIBEN
                                    if (line.StartsWith("---")) { migrationsdateiText.Add(""); }
                                    else if (line.Contains("ANFANG k_"))
                                    {
                                        //überspring kundenabschnitte
                                        //hier kundenskripte erzeugen

                                        string kuerzel = "";
                                        string[] kundenKuerzel = Data.GetClients();
                                        int index = 0;
                                        foreach (string name in kundenKuerzel)
                                        {
                                            kundenKuerzel[index] = Data.GetKundenKuerzel(name).ToLower();
                                            index++;
                                        }
                                        //Kundenabschnitt zuordnen
                                        foreach (string krzl in kundenKuerzel)
                                        {
                                            if (line.EndsWith(krzl))
                                            {
                                                kuerzel = krzl;
                                                break;
                                            }
                                        }

                                        List<string> kundenAbschnitt = new List<string> { };

                                        //überspring kundenabschnitte
                                        //hier kundenskripte erzeugen
                                        bool stop = false;

                                        while (!stop)
                                        {
                                            line = alter.ReadLine();

                                             if (!line.Contains("ENDE k_"))
                                             {
                                                if(line.StartsWith("----------------") && line.EndsWith("----------------"))
                                                {
                                                    
                                                }
                                                else
                                                {
                                                    kundenAbschnitt.Add(line);
                                                }
                                                
                                             }
                                            
                                            else
                                            {
                                                stop = true;
                                            }
                                        }

                                        string kundenMigName = "migration_" + las_vonVersion[2] + "_" + las_bisVersion[2] + "_k_" + kuerzel + ".sql";
                                        string dateiPfad = migrationsOrdner + "//" + kundenMigName;
                                        if (File.Exists(dateiPfad))
                                        {
                                            System.IO.StreamReader datei = new System.IO.StreamReader(dateiPfad, Encoding.UTF8);
                                            int insertPostition = 0;
                                            string s;
                                            while ((s = datei.ReadLine()) != null)
                                            {
                                                if (s.StartsWith("@"))
                                                {
                                                    break;
                                                }
                                                else {
                                                    insertPostition += 1;
                                                }
                                            }

                                            datei.Close();

                                            //SchreibschutzEntfernen(dateiPfad);
                                            List<string> dateiInhalt = new List<string>(File.ReadAllLines(dateiPfad));
                                            dateiInhalt.InsertRange(insertPostition, kundenAbschnitt);

                                            //SchreibschutzEntfernen(dateiPfad);
                                            string[] toPrint= dateiInhalt.ToArray<string>();
                                            System.IO.File.WriteAllLines(dateiPfad, toPrint, Encoding.Default);
                                        }
                                        else
                                        {
                                            //CREATE NEW
                                            List<string> kundenMigDatei = new List<string>()
                                            {
                                            "-- Ausführung aller Veränderungen im Zeitraum",
                                            "-- " + as_datum_von + " bis " + as_datum_bis,
                                            "",
                                            "set echo on",
                                            "set serveroutput on",
                                            ""
                                            };

                                            kundenMigDatei.InsertRange(6, kundenAbschnitt);

                                            //SchreibschutzEntfernen(dateiPfad);
                                            string[] toPrint = kundenMigDatei.ToArray<string>();
                                            System.IO.File.WriteAllLines(dateiPfad, toPrint, Encoding.Default);
                                        }
                                    }
                                    else if (line.Contains("parameter_wert = '" + as_versionBis + "'"))
                                    {
                                        migrationsdateiText.Add(line);
                                        abbruch = true;
                                        break;
                                    }
                                    else
                                    {
                                        migrationsdateiText.Add(line);
                                    }
                                }

                                if (abbruch) { break; }
                            }
                        }
                    }

                    migrationsdateiText.Add(line);
                    for (int i = 0; i < 14; i++)
                    {
                        line = alter.ReadLine();
                        migrationsdateiText.Add(line);
                    }
                    migrationsdateiText.Add("");
                }
                catch (Exception) { throw; }
                finally { alter.Close(); }
            }
            #endregion

            #region skriptaufrufe
            
            string[] las_reihenfolge = Data.GetReihenfolge();
            string[] las_zusatzdateien = Data.GetZusatzdateien();

            var dir = new DirectoryInfo(migrationsOrdner);
            List<string> reste = new List<string> { };
            foreach (var file in dir.EnumerateFiles())
            {
                bool isAusnahme = false;
                foreach (var zusatzdatei in las_zusatzdateien)
                {
                    string pattern = zusatzdatei;
                    string filecheck = file.ToString();
                    string regexpattern = String.Format(@"^{0}$", pattern.Replace("*", ".*"));

                    if (Regex.IsMatch(filecheck, regexpattern))
                    {
                        isAusnahme = true;
                    }
                }
                if (!isAusnahme && !file.Name.Contains("_k_") && !file.Name.Contains("_f_") && !file.Name.Contains("_m_"))
                {
                    reste.Add(file.ToString());

                }
                
                
            }

            //Dateien in Reihenfolge
            foreach (var reihenfolgeTyp in las_reihenfolge)
            {
                las_dateien = reste.ToArray<string>();
                foreach (string file in las_dateien)
                {
                    string pattern = reihenfolgeTyp;
                    string filecheck = file.ToString();
                    string regexpattern = String.Format(@"^{0}$", pattern.Replace("*", ".*"));

                    if (Regex.IsMatch(filecheck, regexpattern))
                    {
                        migrationsdateiText.Add("@" + file);
                        reste.Remove(file);
                    }
                }
            }
            //Reste ohne Reihenfolge
            las_reste = reste.ToArray<string>();
            foreach (string file in las_reste)
            {
                migrationsdateiText.Add("@" + file);
            }
            // Migrationskripten für Freigabe und Modul aufrufen
            DirectoryInfo migrationSkripten = new DirectoryInfo(migrationsOrdner);
            migrationsdateiText.Add("");
            foreach (var migrationSkript in migrationSkripten.GetFiles())
            {
                if (migrationSkript.Name.StartsWith("migration") && (migrationSkript.Name.Contains("_f_") || migrationSkript.Name.Contains("_m_")))
                {
                    
                    migrationsdateiText.Add("@" + migrationSkript);

                }
            }

            #endregion


            // DATEI ERZEUGEN
            string[] textToPrint = migrationsdateiText.ToArray<string>();
            System.IO.File.WriteAllLines(migrationsOrdner + "\\" + dateiName, textToPrint, Encoding.Default);
            return;
        }

        private static void SchreibschutzEntfernen(string targetDir)
        {
            foreach (var datei in Directory.GetFiles(targetDir))
            {
                string filepath = Path.Combine(targetDir, Path.GetFileName(datei));
                FileInfo fileInfo = new FileInfo(filepath);
                fileInfo.IsReadOnly = false;
            }
        }

        private static void CopyFiles(string sourceDir, string targetDir)
        {
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string filePath = Path.Combine(targetDir, Path.GetFileName(file));
                if (!File.Exists(filePath))
                {
                    File.Copy(file, filePath, false);
                }
            }

            //Falls man unterordner mitkopieren möchte
            //foreach (var directory in Directory.GetDirectories(sourceDir))
            //{
            //    File.Copy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
            //}
        }

        private static void CreateMigrationKFM(string as_versionVon, string as_versionBis, string as_datum_von, string as_datum_bis, string as_zielpfad)
        {

            string[] las_VersionVon = as_versionVon.Split('.');
            string[] las_VersionBis = as_versionBis.Split('.');
            DirectoryInfo skriptNames = new DirectoryInfo(as_zielpfad);

            foreach (var skriptName in skriptNames.GetFiles())
            {

                if (skriptName.Name.StartsWith("init") && skriptName.Name.Contains("_k_"))
                {
                    List<string> migrationsdateiText = new List<string>();
                    migrationsdateiText.Add("--Ausführung aller Veränderungen im Zeitraum");
                    migrationsdateiText.Add("--" + as_datum_von + " bis " + as_datum_bis);
                    migrationsdateiText.Add("");
                    migrationsdateiText.Add("set echo on");
                    migrationsdateiText.Add("set serveroutput on");
                    migrationsdateiText.Add("");
                    migrationsdateiText.Add("@" + skriptName);
                    string[] textToPrint = migrationsdateiText.ToArray<string>();
                    string[] kundenName = skriptName.ToString().Split('.');
                    string skriptName_2 = kundenName[0];
                    string splitter = "_k_";
                    string[] splitted = skriptName_2.Split(new[] { splitter }, StringSplitOptions.None);
                    string lastName = splitted[1];

                    string skriptiNameKunde = "migration_" + las_VersionVon[2] + "_" + las_VersionBis[2] + "_k_" + lastName + ".sql";
                    if (File.Exists(as_zielpfad + "//" + skriptiNameKunde))
                    {
                        StreamWriter sw = new StreamWriter(as_zielpfad + "//" + skriptiNameKunde, true);
                        sw.WriteLine("@" + skriptName);
                        sw.Close();
                    }
                    else
                    {
                        System.IO.File.WriteAllLines(as_zielpfad + "\\" + skriptiNameKunde, textToPrint, Encoding.Default);
                    }


                }
                else if (skriptName.Name.StartsWith("init") && skriptName.Name.Contains("_f_"))
                {
                    List<string> migrationsdateiText = new List<string>();
                    migrationsdateiText.Add("--Ausführung aller Veränderungen im Zeitraum");
                    migrationsdateiText.Add("--" + as_datum_von + " bis " + as_datum_bis);
                    migrationsdateiText.Add("");
                    migrationsdateiText.Add("set echo on");
                    migrationsdateiText.Add("set serveroutput on");
                    migrationsdateiText.Add("");
                    migrationsdateiText.Add("@" + skriptName);
                    string[] textToPrint = migrationsdateiText.ToArray<string>();
                    string[] freigabeName = skriptName.ToString().Split('.');
                    string skriptName_2 = freigabeName[0];
                    string splitter = "_f_";
                    string[] splitted = skriptName_2.Split(new[] { splitter }, StringSplitOptions.None);
                    string lastName = splitted[1];

                    string skriptiNameFreigabe = "migration_" + las_VersionVon[2] + "_" + las_VersionBis[2] + "_f_" + lastName + ".sql";
                    if (File.Exists(as_zielpfad + "//" + skriptiNameFreigabe))
                    {
                        StreamWriter sw = new StreamWriter(as_zielpfad + "//" + skriptiNameFreigabe, true);
                        sw.WriteLine("@" + skriptName);
                        sw.Close();
                    }
                    else
                    {
                        System.IO.File.WriteAllLines(as_zielpfad + "\\" + skriptiNameFreigabe, textToPrint, Encoding.Default);
                    }

                }
                else if (skriptName.Name.StartsWith("init") && skriptName.Name.Contains("_m_"))
                {
                    List<string> migrationsdateiText = new List<string>();
                    migrationsdateiText.Add("--Ausführung aller Veränderungen im Zeitraum");
                    migrationsdateiText.Add("--" + as_datum_von + " bis " + as_datum_bis);
                    migrationsdateiText.Add("");
                    migrationsdateiText.Add("set echo on");
                    migrationsdateiText.Add("set serveroutput on");
                    migrationsdateiText.Add("");
                    migrationsdateiText.Add("@" + skriptName);
                    string[] textToPrint = migrationsdateiText.ToArray<string>();
                    string[] modulname = skriptName.ToString().Split('.');
                    string skriptName_2 = modulname[0];

                    string splitter = "_m_";
                    string[] splitted = skriptName_2.Split(new[] { splitter }, StringSplitOptions.None);
                    string lastName = splitted[1];

                    string skriptiNamemodul = "migration_" + las_VersionVon[2] + "_" + las_VersionBis[2] + "_m_" + lastName + ".sql";
                    if (File.Exists(as_zielpfad + "//" + skriptiNamemodul))
                    {
                        StreamWriter sw = new StreamWriter(as_zielpfad + "//" + skriptiNamemodul, true);
                        sw.WriteLine("@" + skriptName);
                        sw.Close();
                    }
                    else
                    {
                        System.IO.File.WriteAllLines(as_zielpfad + "\\" + skriptiNamemodul, textToPrint, Encoding.Default);
                    }



                }


            }



        }

        private static void CreatePostMigration(string as_zielpfad, string as_versionVon, string as_versionBis)
        {
            string[] las_VersionVon = as_versionVon.Split('.');
            string[] las_VersionBis = as_versionBis.Split('.');

            string dateiName = "post_migration_" + las_VersionVon[2] + "_" + las_VersionBis[2] + ".sql";
            string[] las_text = {
                "",
                "",
                "exec ef_utilities.analyze_schema('&METHOD')",
                "",
                "@analyze_schema_info.sql",
                "",
                "exec ef_utilities.create_hist_triggers",
                "",
                "exec ef_utilities.create_sequences",
                "",
                "exec ef_utilities.remove_locks",
                "",
                "COMMIT",
                "/",
                "",
                "exec compile_objects",
                "",
                "COMMIT",
                "/",
                "",
                "column OBJECT_NAME format a30",
                "column OBJECT_TYPE format a30",
                "set linesize 1000",
                "set pagesize 1000",
                "",
                "SELECT object_name, object_type FROM user_objects WHERE status = 'INVALID'",
                "/",
                "",
                ""
            };
            System.IO.File.WriteAllLines(as_zielpfad + "\\" + dateiName, las_text, Encoding.Default);
        }
        private static void CreateToDo(string as_zielpfad, string as_versionVon, string as_versionBis)
        {
            string[] las_VersionVon = as_versionVon.Split('.');
            string[] las_VersionBis = as_versionBis.Split('.');
            List<string> dateinName = new List<string>();
            string todoName = "todo.txt";
            dateinName.Add("Anleitung für Update auf Version " + as_versionBis);
            dateinName.Add("Zwingend erforderlich ist Version " + as_versionVon + " !!!");
            dateinName.Add("");
            dateinName.Add("ACHTUNG!    SQLPLUS wird geschlossen, wenn die zwingend erforderliche Version");
            dateinName.Add("            		mit Datenbank Version nicht übereinstimmt.");
            dateinName.Add("");
            dateinName.Add("");
            dateinName.Add("Bevor die Migration begonnen wird sollten folgende Tabellenerweiterungen");
            dateinName.Add("durchgeführt werden.");
            dateinName.Add("");
            dateinName.Add("");
            dateinName.Add("Clients:");
            dateinName.Add("ef.exe ersetzen.");
            dateinName.Add("pbd-dateien im Source-Verzeichnis ersetzen.");
            dateinName.Add("");
            dateinName.Add("Server:");
            dateinName.Add("Öffnen CMD-Konsole (Start -> Ausführen...)");
            dateinName.Add("Eingabe cmd, CMD-Konsole wird geöffnet.");
            dateinName.Add("Folgenden Befehl eingeben");
            dateinName.Add("");
            dateinName.Add("set nls_lang=AMERICAN_AMERICA.WE8MSWIN1252");
            dateinName.Add("");
            dateinName.Add("und in neuer Zeile");
            dateinName.Add("");
            dateinName.Add("sqlplus");
            dateinName.Add("");
            dateinName.Add("mit ef3 Schema anmelden (ef3-oracle user)");
            dateinName.Add("");
            dateinName.Add("Spooldatei im Migrationsverzeichnis öffnen (Verzeichnis mit Migrationsskripten)");
            dateinName.Add("");
            dateinName.Add("");
            dateinName.Add("Folgende Variablen können definiert werden,");
            dateinName.Add("damit Statistiken (Hinweise, die Datenbankprozesse optimieren) berechnet werden können.");
            dateinName.Add("");
            dateinName.Add("DEFINE METHOD = '<method>' (COMPUTE, COMPUTE_SUBSET, NONE)");
            dateinName.Add("");
            dateinName.Add("Defaultwert für diese Variable ist NONE!!!");
            dateinName.Add("");
            dateinName.Add("");
            dateinName.Add("METHOD Parameter:");
            dateinName.Add("1. COMPUTE        -- es werden Analysedaten zu allen nicht gekennzeichnenten Tabellen angelegt");
            dateinName.Add("2. COMPUTE_SUBSET -- es werden Analysedaten zu allen gekennzeichneten Tabellen angelegt");
            dateinName.Add("3. NONE           -- es werden keine Analysedaten angelegt - DEFAULT");
            dateinName.Add("");
            dateinName.Add("");
            dateinName.Add("");
            dateinName.Add("");
            dateinName.Add("Hinweis zu COMPUTE und COMPUTE_SUBSET");
            dateinName.Add("-------------------------------------");
            dateinName.Add("Im Pflegedialog Tabelle können die Tabellen in den Eckdaten gekennzeichnet werden, die z.B. während einer Migration zunächst");
            dateinName.Add("nicht analysiert werden sollen, weil das Zeitfenster (Wartungsfenster) nicht groß genug ist, um alle Tabellen des Schemas analysieren zu können.");
            dateinName.Add("Somit können einzelne Tabellen entweder alleine oder im Paket zu einem späteren Zeitpunkt verarbeitet werden, der für die Kunden günstig ist.");
            dateinName.Add("");
            dateinName.Add("");
            dateinName.Add("Folgende skripte direkt in sql+ ausführen");
            dateinName.Add("");
            dateinName.Add("@user_info.sql");
            dateinName.Add("");
            dateinName.Add("@migration_" + las_VersionVon[2] + "_" + las_VersionBis[2] + ".sql"); // letzte 3 Zeffer
            dateinName.Add("");

            DirectoryInfo migrationSkripten = new DirectoryInfo(as_zielpfad);
            foreach (var migrationSkripte in migrationSkripten.GetFiles())
            {
                string[] las_migrationSkripte = migrationSkripte.ToString().Split('.');
                string migrationSkripte_2 = las_migrationSkripte[0];
                if (migrationSkripte.Name.StartsWith("migration") && migrationSkripte.Name.Contains("_k_"))
                {
                    string splitter = "_k_";
                    string[] splitted = migrationSkripte_2.Split(new[] { splitter }, StringSplitOptions.None);
                    string lastName = splitted[1];
                    dateinName.Add("-- Nur für Kunde " + lastName.ToUpper());
                    dateinName.Add("@" + migrationSkripte);
                    dateinName.Add("");
                }
                else if (migrationSkripte.Name.StartsWith("migration") && migrationSkripte.Name.Contains("_f_"))
                {

                    string splitter = "_f_";
                    string[] splitted = migrationSkripte_2.Split(new[] { splitter }, StringSplitOptions.None);
                    string lastName = splitted[1];
                    dateinName.Add("-- Nur für Freigabe " + lastName.ToUpper());
                    dateinName.Add("@" + migrationSkripte);
                    dateinName.Add("");
                }
                else if (migrationSkripte.Name.StartsWith("migration") && migrationSkripte.Name.Contains("_m_"))
                {

                    string splitter = "_m_";
                    string[] splitted = migrationSkripte_2.Split(new[] { splitter }, StringSplitOptions.None);
                    string lastName = splitted[1];
                    dateinName.Add("-- Nur für Modul " + lastName.ToUpper());
                    dateinName.Add("@" + migrationSkripte);
                    dateinName.Add("");
                }
            }
            dateinName.Add("@post_migration_" + las_VersionVon[2] + "_" + las_VersionBis[2] + ".sql");
            dateinName.Add("");
            dateinName.Add("");
            dateinName.Add("Spooldatei schließen");
            dateinName.Add("");
            dateinName.Add("nun ist Datenbank auf Stand " + as_versionBis);

            string[] textToPrint = dateinName.ToArray<string>();


            System.IO.File.WriteAllLines(as_zielpfad + "\\" + todoName, dateinName, Encoding.Default);
        }

        //private static void SteuerDateiEntw(string as_zielpfad, string as_versionVon, string as_versionBis)
        //{
        //    string datenbankEntw = Data.GetEntwDatabase();
        //    string serverEntw = Data.GetEntwServer();
        //    string steuerDateiEntw = "STEUER_DATEI_" + datenbankEntw + "(" + serverEntw + ")" + ".sql";
        //    string[] las_VersionVon = as_versionVon.Split('.');
        //    string[] las_VersionBis = as_versionBis.Split('.');
        //    if (File.Exists(steuerDateiEntw))
        //    {

        //    }
        //    else
        //    {
        //        string steuerpfad = as_zielpfad + "\\" + steuerDateiEntw;

        //        FileStream fs = File.Create(steuerpfad);
        //        fs.Close();
        //    }
        //    List<string> steuerDatei = new List<string>();
        //    steuerDatei.Add("spool " + datenbankEntw + "§" + serverEntw + ".lst");
        //    steuerDatei.Add("@user_info.sql");
        //    steuerDatei.Add("@migration_" + las_VersionVon[2] + "_" + las_VersionBis[2] + ".sql");
        //    steuerDatei.Add("@post_migration_" + las_VersionVon[2] + "_" + las_VersionBis[2] + ".sql");
        //    steuerDatei.Add("spool off;");
        //    steuerDatei.Add("exit");
        //    steuerDatei.Add("");

        //    string[] textToPrint = steuerDatei.ToArray<string>();
        //    System.IO.File.WriteAllLines(as_zielpfad + "\\" + steuerDateiEntw, textToPrint, Encoding.Default);
        //}
    }
}