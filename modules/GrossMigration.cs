using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

namespace BuildToolService
{
    public class GrossMigration
    {
        public static string OrdnerErstellen(string as_versionVon, string as_versionBis, string as_kunde)
        {
            string zielPfad, ef3pfad, migrationsPfad, versionsPrefix, versionCopyPrefix, dateiName, versionToCopy, versionToCheck, versionToCopyPath, versionToCheckPath;
            string[] las_versionVon, las_versionBis, las_versionToCopy, las_entferneEinzel, las_entferneTyp, las_ausnahmen;
            int li_hochVersionVon, li_mittelVersionVon, li_unterVersionVon, li_hochVersionBis, li_mittelVersionBis, li_unterVersionBis;
            bool postMigrationAlt = false;
            //TODO PFADE IN KONFIG SETZEN
            //versionsPfad zusammengesetzt aus ef3pfad + version + migrationspfad 
            //ef3pfad = @"C:\TEMP\test\";
            ef3pfad = Data.GrossMigration3 + Data.GrossMigration1;
            //ef3pfad = @"W:\\ef3\\"
            //migrationsPfad = @"\Datenbank\Migration\";
            migrationsPfad = Data.GrossMigration2;
            //string kundenKennung = Data.GetKundenKennung(as_kunde);
            string kundenKuerzel = Data.GetKundenKuerzel(as_kunde);
            //3.1.101- 5.0.100 - 
            #region Eingabenprüfung

            // Version (x.x.xxx) + Kunde vorhanden
            Regex regex = new Regex(@"\A[0-9]\.[0-9]\.[0-9]{3}\z");
            if (!regex.IsMatch(as_versionVon))
            {
                throw new ArgumentException("Version: \"" + as_versionVon + "\" ist ungültig!", "version");
            }
            if (!regex.IsMatch(as_versionBis))
            {
                throw new ArgumentException("Version: \"" + as_versionBis + "\" ist ungültig!", "version");
            }
            if (!Data.GetClients().Contains<string>(as_kunde.ToLower()))
            {
                throw new ArgumentException("Kunde: \"" + as_kunde + "\" ist ungültig!", "kunde");
            }

            las_versionBis = as_versionBis.Split('.');
            versionsPrefix = las_versionBis[0] + "." + las_versionBis[1];

            //check for version to be updated only to incremental versions and not to the previous versions(version von should be always lesser than the version bis
            las_versionVon = as_versionVon.Split('.');
            // hoch X.x.xxx mittel x.X.xxx unter x.x.XXX
            li_hochVersionVon = int.Parse(las_versionVon[0]);
            li_mittelVersionVon = int.Parse(las_versionVon[1]);
            li_unterVersionVon = int.Parse(las_versionVon[2]);

            versionToCopy = as_versionBis;
            versionCopyPrefix = versionsPrefix;
            las_versionToCopy = versionToCopy.Split('.');
            // hoch X.x.xxx mittel x.X.xxx unter x.x.XXX
            li_hochVersionBis = int.Parse(las_versionToCopy[0]);
            li_mittelVersionBis = int.Parse(las_versionToCopy[1]);
            li_unterVersionBis = int.Parse(las_versionToCopy[2]);

            //optimierbar
            if (li_hochVersionVon < li_hochVersionBis)
            {
                // Fertig
            }
            else
            {
                if (li_hochVersionVon == li_hochVersionBis)
                {
                    if (li_mittelVersionVon < li_mittelVersionBis)
                    {
                        // Fertig
                    }
                    else
                    {
                        if (li_mittelVersionVon == li_mittelVersionBis)
                        {
                            if (li_unterVersionVon >= li_unterVersionBis)
                            {
                                throw new ArgumentException("Version: \"" + as_versionVon + "\" ist größer als \"" + as_versionBis + "\"", "version");
                            }

                            // Fertig
                        }
                        else
                        {
                            throw new ArgumentException("Version: \"" + as_versionVon + "\" ist größer als \"" + as_versionBis + "\"", "version");
                        }
                    }

                }
                else
                {
                    throw new ArgumentException("Version: \"" + as_versionVon + "\" ist größer als \"" + as_versionBis + "\"", "version");
                }
            }

            //BT-26 check Version > 4.1.64
            //optimierbar
            if (li_hochVersionBis > 4) { }
            else if (li_hochVersionBis == 4 && li_mittelVersionBis > 1) { }
            else if (li_hochVersionBis == 4 && li_mittelVersionBis == 1 && li_unterVersionBis > 64) { }
            else { 
                postMigrationAlt = true; 
            }

            #endregion
            #region Ordnererstellen

            dateiName = "Migration_" + as_versionVon + "_" + as_versionBis + "_" + kundenKuerzel;

            zielPfad = Path.Combine(ef3pfad + versionsPrefix + migrationsPfad + dateiName);
            if (!Directory.Exists(zielPfad))
            {
                bool lb_exception = false;
                
                try
                {
                    Directory.CreateDirectory(zielPfad);
                }
                catch (Exception e)
                {
                    lb_exception = true;
                }

                if (lb_exception)
                {
                    ef3pfad = Data.GrossMigration4 + Data.GrossMigration1;
                    zielPfad = Path.Combine(ef3pfad + versionsPrefix + migrationsPfad + dateiName);

                    if (!Directory.Exists(zielPfad))
                    {
                        try
                        {
                            Directory.CreateDirectory(zielPfad);
                        }
                        catch (Exception e)
                        {
                            throw new ArgumentException("Fehler bei der Erstellung des Pfades: \"" + zielPfad + "\" ", "ordner");
                        }
                        
                    }
                    else
                    {
                        throw new ArgumentException("Ordner \"" + zielPfad + "\" existiert bereits", "ordner");
                    }

                }
            }
            else
            {
                throw new ArgumentException("Ordner \"" + zielPfad + "\" existiert bereits", "ordner");
            }
            #endregion
            #region Dateienkopieren                

            versionCopyPrefix = versionsPrefix;

            while (as_versionVon != versionToCopy)
            {
                if (!Directory.Exists(Path.Combine(ef3pfad + versionCopyPrefix + migrationsPfad + versionToCopy)))
                {
                    ef3pfad = Data.GrossMigration4 + Data.GrossMigration1;
                    if (!Directory.Exists(Path.Combine(ef3pfad + versionCopyPrefix + migrationsPfad + versionToCopy)))
                    {
                        throw new ArgumentException("Pfad existiert nicht: " + Path.Combine(ef3pfad + versionCopyPrefix + migrationsPfad, "ordner"));
                    }
                }

                //kopieren
                versionToCopyPath = Path.Combine(ef3pfad + versionCopyPrefix + migrationsPfad + versionToCopy);
                CopyFiles(versionToCopyPath, zielPfad);
                //nächste version identifizieren

                if (li_unterVersionBis > 1)
                {
                    li_unterVersionBis--;
                    versionToCopy = versionCopyPrefix + "." + (li_unterVersionBis).ToString("000");
                }
                else if (li_mittelVersionBis > 0)
                {
                    //init neue mittelVersion
                    li_unterVersionBis = 1;
                    li_mittelVersionBis--;
                    versionCopyPrefix = (li_hochVersionBis).ToString("0") + "." + (li_mittelVersionBis).ToString("0");

                    versionToCheck = versionCopyPrefix + ".001";
                    versionToCheckPath = Path.Combine(ef3pfad + versionCopyPrefix + migrationsPfad + versionToCheck);
                    if (!Directory.Exists(versionToCheckPath))
                    {
                        ef3pfad = Data.GrossMigration4 + Data.GrossMigration1;
                        versionToCheckPath = Path.Combine(ef3pfad + versionCopyPrefix + migrationsPfad + versionToCheck);
                        if (!Directory.Exists(versionToCheckPath))
                        {
                            throw new ArgumentException("Konnte Migrationsordner Anfang nicht finden: " + versionToCheckPath, "ordner");
                        }
                    }
                    
                    do
                    {
                        li_unterVersionBis++;
                        versionToCheck = versionCopyPrefix + "." + (li_unterVersionBis).ToString("000");
                        versionToCheckPath = Path.Combine(ef3pfad + versionCopyPrefix + migrationsPfad + versionToCheck);                    
                    }
                    while (Directory.Exists(versionToCheckPath));
                    li_unterVersionBis--;
                    versionToCopy = versionCopyPrefix + "." + (li_unterVersionBis).ToString("000");
                }
                else
                {
                    //init neue hochVersion
                    li_unterVersionBis = 1;
                    li_mittelVersionBis = 9;
                    li_hochVersionBis--;
                    versionCopyPrefix = (li_hochVersionBis).ToString("0") + "." + (li_mittelVersionBis).ToString("0");

                    do
                    {
                        li_unterVersionBis++;
                        versionToCheck = versionCopyPrefix + "." + (li_unterVersionBis).ToString("000");
                        versionToCheckPath = Path.Combine(ef3pfad + versionCopyPrefix + migrationsPfad + versionToCheck);
                    }
                    while (Directory.Exists(versionToCheckPath));

                    li_unterVersionBis--;
                    versionToCopy = versionCopyPrefix + "." + (li_unterVersionBis).ToString("000");
                }

            }

            #endregion
            #region Schreibschutzentfernen
            SchreibschutzEntfernen(zielPfad);
            #endregion
            #region Dateienlöschen
            las_entferneEinzel = Data.GetEinzeldateien();
            las_entferneTyp = Data.GetDateitypen();
            las_ausnahmen = Data.GetAusnahmen();

            //einzeldateien
            foreach (var einzelDatei in las_entferneEinzel)
            {
                if (File.Exists(Path.Combine(zielPfad + "\\" + einzelDatei)))
                {
                    File.Delete(Path.Combine(zielPfad + "\\" + einzelDatei));
                }
            }
            //dateitypen
            var dir = new DirectoryInfo(zielPfad);
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
                            File.Delete(Path.Combine(zielPfad + @"\" + file));
                        }
                    }
                }
            }

            //kundendateien
            dir = new DirectoryInfo(zielPfad);

            foreach (var file in dir.EnumerateFiles("*_k_*"))
            {
                string kundenpattern = @"^.*_k_" + kundenKuerzel.ToLower() + "[._].*$";
                string filecheck = file.ToString();
                if (!Regex.IsMatch(filecheck, kundenpattern))
                {
                    File.Delete(Path.Combine(zielPfad + @"\" + file));
                }
            }

            #endregion
            #region Dateien erzeugen

            CreateMigration(as_versionVon, as_versionBis, as_kunde, zielPfad);
            CreatePostMigration(zielPfad, as_versionVon, as_versionBis, postMigrationAlt);
            CreateToDo(zielPfad, as_versionVon, as_versionBis);

            #endregion

            string message = "Ordner erfolgreich erstellt und zu finden unter: " + zielPfad;
            return message;
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
        private static void SchreibschutzEntfernen(string targetDir)
        {
            foreach (var datei in Directory.GetFiles(targetDir))
            {
                string filepath = Path.Combine(targetDir, Path.GetFileName(datei));
                FileInfo fileInfo = new FileInfo(filepath);
                fileInfo.IsReadOnly = false;
            }
        }
        private static void CreateToDo(string targetDir, string as_versionVon, string as_versionBis)
        {
            string dateiName = "todo.txt";
            string[] las_text = {
                "Anleitung für Update auf Version " + as_versionBis,
                "",
                "Zwingend erforderlich ist Version "+ as_versionVon + " !!!",
                "",
                "ACHTUNG !!! SQLPLUS wird geschlossen, wenn zwingend erforderliche Version",
                "mit Datenbank Version nicht übereinstimmt",
                "",
                "Clients:",
                "",
                "ef.exe ersetzen",
                "pbd-dateien im Source-Verzeichnis ersetzen",
                "dateien im Shared-Verzeichnis ersetzen",
                "",
                "Server:",
                "Öffnen CMD-Konsole (Start -> Ausführen...)",
                "Eingabe cmd, CMD-Konsole wird geöffnet.",
                "Folgenden Befehl eingeben",
                "",
                "set nls_lang=AMERICAN_AMERICA.WE8MSWIN1252",
                "",
                "und in neuer Zeile",
                "",
                "sqlplus",
                "",
                "Anmelden mit ef3 Schema (ef3-oracle user)",
                "",
                "spooldatei im Verzeichnis offnen, wo zip-datei extrahiert wurde.",
                "",
                "ACHTUNG !!! DB-Administrator muß folgende Systemberechtigung \"CREATE TRIGGER\" vergeben werden,",
                "um danach Aktualisierungstrigger aufrufen zu können.",
                "Aktualisierungstrigger werden direkt nach Migration aufgerufen werden",
                "",
                "Folgende Variablen müssen definiert werden,",
                "damit die Datenbank am Ende der Migration analysiert werden kann.",
                "",
                "Defaultwert für diese Variable ist NONE!!!",
                "",
                "METHOD Parameter:",
                "1. COMPUTE        -- es werden Analysedaten zu allen nicht gekennzeichnenten Tabellen angelegt",
                "2. COMPUTE_SUBSET -- es werden Analysedaten zu allen gekennzeichneten Tabellen angelegt",
                "3. NONE           -- es werden keine Analysedaten angelegt - DEFAULT",
                "",
                "Hinweis zu COMPUTE und COMPUTE_SUBSET",
                "-------------------------------------",
                "Im Pflegedialog Tabelle können die Tabellen in den Eckdaten gekennzeichnet werden, die z.B. während einer Migration zunächst",
                "nicht analysiert werden sollen, weil das Zeitfenster (Wartungsfenster) nicht groß genug ist, um alle Tabellen des Schemas analysieren zu können.",
                "Somit können einzelne Tabellen entweder alleine oder im Paket zu einem späteren Zeitpunkt verarbeitet werden, der für die Kunden günstig ist.",
                "",
                "",
                "folgende scripte direkt in sql+ ausführen",
                "",
                "@user_info.sql",
                "",
                "@migration_" + as_versionVon + "_" + as_versionBis + ".sql",
                "",
                "@post_migration_" + as_versionVon + "_" + as_versionBis + ".sql",
                "",
                "",
                "nun ist Datenbank auf Stand " + as_versionBis,
                ""
                };
            System.IO.File.WriteAllLines(targetDir + "\\" + dateiName, las_text, Encoding.Default);
        }
        private static void CreatePostMigration(string targetDir, string as_versionVon, string as_versionBis, bool alt)
        {
            string compile = "";
            if (alt)
            {
                compile = "exec ef_utilities.compile_objects";
            }
            else 
            {
                compile = "exec compile_objects";
            }
            string dateiName = "post_migration_" + as_versionVon + "_" + as_versionBis + ".sql";
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
                compile,
                "",
                "COMMIT",
                "/",
                "",
                "column OBJECT_NAME format a30",
                "column OBJECT_TYPE format a30",
                "",
                "set linesize 1000",
                "set pagesize 1000",
                "",
                "SELECT object_name, object_type FROM user_objects WHERE status = 'INVALID'",
                "/",
                "",
                ""
            };
            System.IO.File.WriteAllLines(targetDir + "\\" + dateiName, las_text, Encoding.Default);
        }
        private static string SucheAlterVonVersion(string as_version, string as_dbpfad = @"\Datenbank\")
        {
            // sucht die alter Datei in der die angebene Version enthalten ist und gibt Pfad zurück
            string ef3pfad = Data.GrossMigration3 + Data.GrossMigration1;
            string[] las_version = as_version.Split('.');
            string versionsPrefix = las_version[0] + "." + las_version[1];
            string targetAlterFile = "alter.sql";
            string alterPath = Path.Combine(ef3pfad + versionsPrefix + as_dbpfad + targetAlterFile);
            int alterGeneration = 1;
            if (File.Exists(alterPath))
            {
                while (File.Exists(alterPath))
                {
                    System.IO.StreamReader alter = new System.IO.StreamReader(alterPath, Encoding.Default);
                    string line;
                    try
                    {

                        while ((line = alter.ReadLine()) != null)
                        {
                            if (line.Contains("parameter_wert = '" + as_version + "'"))
                            {
                                alter.Close();
                                return alterPath;
                            }
                        }
                        alterGeneration++;
                        targetAlterFile = "alter" + alterGeneration.ToString() + ".sql";
                        alterPath = Path.Combine(ef3pfad + versionsPrefix + as_dbpfad + targetAlterFile);
                    }
                    catch (Exception) { throw; }
                    finally { alter.Close(); }
                }
            }
            else
            {
                ef3pfad = Data.GrossMigration4 + Data.GrossMigration1;
                alterPath = Path.Combine(ef3pfad + versionsPrefix + as_dbpfad + targetAlterFile);
                
                while (File.Exists(alterPath))
                {
                    System.IO.StreamReader alter = new System.IO.StreamReader(alterPath, Encoding.Default);
                    string line;
                    try
                    {

                        while ((line = alter.ReadLine()) != null)
                        {
                            if (line.Contains("parameter_wert = '" + as_version + "'"))
                            {
                                alter.Close();
                                return alterPath;
                            }
                        }
                        alterGeneration++;
                        targetAlterFile = "alter" + alterGeneration.ToString() + ".sql";
                        alterPath = Path.Combine(ef3pfad + versionsPrefix + as_dbpfad + targetAlterFile);
                    }
                    catch (Exception) { throw; }
                    finally { alter.Close(); }
                }
            }
            return null;
        }
        private static void CreateMigration(string as_versionVon, string as_versionBis, string as_kunde, string zielPfad)
        {
            string aktuelleVersion = as_versionVon;
            string finalVersion = as_versionBis;
            string migPfad = Data.GrossMigration2;
            string alterPath = SucheAlterVonVersion(aktuelleVersion);
            string[] las_aktuelleVersion = aktuelleVersion.Split('.');
            string[] las_reihenfolge, las_zusatzdateien, las_dateien, las_reste;
            string dateiName = "migration_" + as_versionVon + "_" + as_versionBis + ".sql";
            bool abbruch = false;
            //string kundenKennung = Data.GetKundenKennung(as_kunde);
            string kundenKuerzel = Data.GetKundenKuerzel(as_kunde);


            #region anfangsabschnitt
            // INIT MIT ANFANGSABSCHNITT
            List<string> migrationsdateiText = new List<string>()
            {
              "-- Grossmigrationsdatei",
              "-- Für Kunde: " + as_kunde,
              "-- Von Version: "+ as_versionVon + " bis Version: " + as_versionBis,
              "",
              "set echo on",
              "set serveroutput on",
              "",
              "WHENEVER SQLERROR EXIT",
              "WHENEVER OSERROR EXIT",
              "",
              "@check_db_version.sql '"+ as_versionVon + ", " + as_versionBis + "'",
              "@export_schema_stats.sql 'migration_"+ as_versionVon + "_"+ as_versionBis + "'",
              "",
              "WHENEVER SQLERROR CONTINUE",
              "WHENEVER OSERROR CONTINUE",
              "",
              "@init_special_character.sql",
              "",
              "exec ef_utilities.enable_hist_triggers(FALSE)",
              "",
              "VARIABLE start_migration NUMBER;",
              "EXEC :start_migration := DBMS_UTILITY.get_time;",
              ""
            };
            #endregion
            #region alter_zusammenschneiden
            //ALTER EINTRÄGE ZUSAMMENSCHNEIDEN

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
                        if (line.Contains("parameter_wert = '" + aktuelleVersion + "'"))
                        {
                            aktuelleVersion = las_aktuelleVersion[0] + "." + las_aktuelleVersion[1] + "." + (int.Parse(las_aktuelleVersion[2]) + 1).ToString("000");
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
                                    if (line.StartsWith("---")) { migrationsdateiText.Add(""); }    //
                                    // ab, abs, abc 
                                    else if (line.Contains("ANFANG k_" + kundenKuerzel.ToLower()) && line.EndsWith(kundenKuerzel.ToLower())) { migrationsdateiText.Add("--EIGENER KUNDENABSCHNITT ANFANG"); }        //--EIGENER KUNDENABSCHNITT ANFANG
                                    else if (line.Contains("ENDE k_" + kundenKuerzel.ToLower()) && line.EndsWith(kundenKuerzel.ToLower())) { migrationsdateiText.Add("--EIGENER KUNDENABSCHNITT ENDE"); }        // --EIGENER KUNDENABSCHNITT ENDE
                                    else if (line.Contains("ANFANG k_"))
                                    {
                                        //migrationsdateiText.Add("--FREMD KUNDENABSCHNITT ANFANG");        //
                                        do
                                        {
                                            line = alter.ReadLine();
                                        }
                                        while (!line.Contains("ENDE k_"));
                                        //migrationsdateiText.Add("--FREMD KUNDENABSCHNITT ENDE");        //
                                    }
                                    else if (line.Contains("parameter_wert = '" + aktuelleVersion + "'"))
                                    {
                                        if (aktuelleVersion == finalVersion)
                                        {
                                            migrationsdateiText.Add(line);
                                            abbruch = true;
                                            break;
                                        }
                                        else
                                        {
                                            //aktuelleVersion inkrementieren
                                            las_aktuelleVersion = aktuelleVersion.Split('.');
                                            aktuelleVersion = las_aktuelleVersion[0] + "." + las_aktuelleVersion[1] + "." + (int.Parse(las_aktuelleVersion[2]) + 1).ToString("000");
                                            migrationsdateiText.Add(line);
                                            //migrationsdateiText.Add("NEUE AKTUELLE VERSION " + aktuelleVersion);
                                        }
                                    }
                                    else
                                    {
                                        migrationsdateiText.Add(line);
                                    }
                                }

                                if (abbruch) { break; }

                                alterPath = SucheAlterVonVersion(aktuelleVersion);
                                if (alterPath != null)
                                {
                                    alter.Close();
                                    alter = new System.IO.StreamReader(alterPath, Encoding.Default);
                                    //goto NeueAlter;
                                }
                                else
                                {
                                    las_aktuelleVersion = aktuelleVersion.Split('.');
                                    if (int.Parse(las_aktuelleVersion[1]) == 9)
                                    {
                                        aktuelleVersion = (int.Parse(las_aktuelleVersion[0]) + 1).ToString() + ".0.001";
                                    }
                                    else
                                    {
                                        aktuelleVersion = las_aktuelleVersion[0] + "." + (int.Parse(las_aktuelleVersion[1]) + 1).ToString() + ".001";
                                    }
                                    alterPath = SucheAlterVonVersion(aktuelleVersion);
                                    alter.Close();
                                    alter = new System.IO.StreamReader(alterPath, Encoding.Default);
                                    //goto NeueAlter;
                                }
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
            #region skriptaufrufe_einfügen
            //TODO SKRIPTE AUFRUFEN
            las_reihenfolge = Data.GetReihenfolge();
            las_zusatzdateien = Data.GetZusatzdateien();
            
            var dir = new DirectoryInfo(zielPfad);
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
                if (!isAusnahme)
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

            // Endzeile einfügen 22886
            migrationsdateiText.Add("");
            migrationsdateiText.Add("SELECT TO_CHAR(SYSDATE - (ROUND((DBMS_UTILITY.get_time - :start_migration) / 100, 0) + 1) / 86400, 'DD.MM.RRRR HH24:MI:SS') AS \"Startzeit\",");
            migrationsdateiText.Add("TO_CHAR(SYSDATE, 'DD.MM.RRRR HH24:MI:SS') AS \"Endzeit\",");
            migrationsdateiText.Add("SUBSTR(TO_CHAR(NUMTODSINTERVAL(ROUND((DBMS_UTILITY.get_time - :start_migration) / 100, 0) + 1, 'SECOND')), 9, 11) AS \"Differenz\"");
            migrationsdateiText.Add("FROM dual;");
            migrationsdateiText.Add("");
            #endregion

            // DATEI ERZEUGEN
            string[] textToPrint = migrationsdateiText.ToArray<string>();
            System.IO.File.WriteAllLines(zielPfad + "\\" + dateiName, textToPrint, Encoding.Default);
            return;
        }
    }
}