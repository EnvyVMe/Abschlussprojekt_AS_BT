using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace BuildToolService
{
    public class TestMigrationDurchfuehren
    {

        public static string TestDbMigrieren(string as_versionVon, string as_versionBis)
        {
            string[] las_VersionVon = as_versionVon.Split('.');
            string[] las_VersionBis = as_versionBis.Split('.');
            
            bool db_status = true;
            string as_kunde = "entw_t";

            #region Eingabepruefung
            // Version besteht nur aus nummer x.x.xxx
            Regex regex = new Regex(@"\A[0-9]\.[0-9]\.[0-9]{3}\z");
            if (!regex.IsMatch(as_versionVon))
            {
                throw new ArgumentException("Version: \"" + as_versionVon + "\" ist ungültig!", "version");
            }
            if (!regex.IsMatch(as_versionBis))
            {
                throw new ArgumentException("Version: \"" + as_versionBis + "\" ist ungültig!", "version");
            }
            // Bis-Version > Von-Version
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
                throw new ArgumentException("Von Version: " + as_versionBis + " is kleiner gleich als: " + as_versionVon + " sind nicht hintereinander!", "Version");
            }

            #endregion

            #region Miragtionsordner
            //zielordner = migrationsordner
            string zielPfad = Data.GrossMigration5 + Data.GrossMigration1 + las_VersionVon[0] + "." + las_VersionVon[1] + "\\" + Data.GrossMigration2 + as_versionBis;
            #endregion

            #region Schreibschutzentfernen
            SchreibschutzEntfernen(zielPfad);
            #endregion

            #region Steuer Datein erstellen
            string steuerDateiName = SteuerDateiErstellen(zielPfad, as_versionVon, as_versionBis, as_kunde);
            #endregion

            #region Verbindung herstellen
            ////Variablen Zusammensetzen
            string client_id = Data.GetEntwDatabase().ToLower();
            string client_pw = Data.GetEntwDatabase().ToLower();
            string client_server = Data.GetEntwServer().ToLower();
            string processString = CreateProcessString(client_id, client_pw, client_server);
            string steuerDateiPfad = zielPfad + "\\" + steuerDateiName;

            db_status = ExecuteSqlFile(steuerDateiPfad, zielPfad, processString);
            #endregion

            //rückgabe
            if(!db_status)
            {
                return "-1";
            }
            return "1";
            
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
        
        private static string SteuerDateiErstellen(string targetDir, string versionVon, string versionBis, string kunde)
        {
            string steuerDatei = null;
            string[] las_VersionVon = versionVon.Split('.');
            string[] las_VersionBis = versionBis.Split('.');

            string kundenDatenbank = Data.GetEntwDatabase().ToLower();
            string kundenserver = Data.GetEntwServer().ToLower();

            steuerDatei = "STEUER_DATEI_" + kunde.ToUpper() + ".sql";

            if (File.Exists(steuerDatei))
            {

            }
            else
            {
                string steuerpfad = targetDir + "\\" + steuerDatei;

                FileStream fs = File.Create(steuerpfad);
                fs.Close();
            }


            List<string> migrationsdateiText = new List<string>();
            migrationsdateiText.Add("spool " + kundenDatenbank + "§" + kundenserver + ".lst");
            migrationsdateiText.Add("@user_info.sql");
            migrationsdateiText.Add("@migration_" + las_VersionVon[2] + "_" + las_VersionBis[2] + ".sql");
            string kuerzel = Data.GetKundenKuerzel(kunde);
            DirectoryInfo skripten = new DirectoryInfo(targetDir);

            foreach (var skript in skripten.GetFiles())
            {
                if (skript.Name.StartsWith("migration") && (skript.Name.Contains("_" + kuerzel.ToLower() + ".sql")))
                {
                    if (skript.Name.Contains(".bak"))
                    {

                    }
                    else
                    {
                        migrationsdateiText.Add("@" + skript.Name);
                    }

                }
            }

            migrationsdateiText.Add("@post_migration_" + las_VersionVon[2] + "_" + las_VersionBis[2] + ".sql");
            migrationsdateiText.Add("spool off;");
            migrationsdateiText.Add("exit");
            migrationsdateiText.Add("");

            string[] textToPrint = migrationsdateiText.ToArray<string>();
            System.IO.File.WriteAllLines(targetDir + "\\" + steuerDatei, textToPrint, Encoding.Default);

            return steuerDatei;
        }

        private static string CreateProcessString(string username, string password, string database)
        {
            string processString = $"{username}/{password}@{database}";
            return processString;
        }

        public static bool ExecuteSqlFile(string filePath,string workingDir, string connectionString)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    //FileName muss auf sqlplus verweisen evtl. mit absolutem Pfad
                    FileName = "sqlplus.exe",
                    Arguments = $"{connectionString} @{filePath}",
                    WorkingDirectory = workingDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.StartInfo.EnvironmentVariables["NLS_LANG"] = "AMERICAN_AMERICA.WE8MSWIN1252";
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    int exitCode = process.ExitCode;

                    if (!string.IsNullOrEmpty(output))
                    {
                        Console.WriteLine("Output: ");
                        Console.WriteLine(output);
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("Error: ");
                        Console.WriteLine(error);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: " + e.Message);
                return false;
            }
            return true;
        }
        

        #region todelete
        // ALTE METHODEN 

        //private static string LoadSqlScript(string filePath) {
        //    try
        //    {
        //        return File.ReadAllText(filePath);
        //    }
        //    catch(Exception e) 
        //    {
        //        throw new Exception("fehler beim lesen der steuerdatei: " + e.Message);
        //    }
        //}

        //private static string ExecuteSqlSkript(string connectionString, string sqlScript)
        //{
        //    try 
        //    {
        //        using (OracleConnection connection = new OracleConnection(connectionString)) 
        //        {
        //            connection.Open();

        //            using (OracleCommand command = new OracleCommand(sqlScript, connection)) {
        //                int rowsAffected = command.ExecuteNonQuery();

        //                return $"{rowsAffected} Zeilen beeinflusst";
        //            }
        //        }
        //    } 
        //    catch (Exception e) 
        //    {
        //        throw new Exception("fehler beim ausführen des skriptes: " + e.Message);
        //    }
        //}


        /*
        List<object> list = new List<object>();
        decimal result = 0;

        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            try
            {
                // Open the connection
                connection.Open();
                /////////////////////////
                //Migration hier einfügen

                //string sqlPluscommand = $"sqlplus {uesername}/{password}@{instance} @\"{scriptPath}\"";

                /////////////////////////
                string ls_sql = "SELECT * FROM benutzer";
                // Create the command
                OracleCommand command = new OracleCommand(ls_sql, connection);
                // Execute the command and use a data reader to read the data
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    //Process the data
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            list.Add(reader.GetValue(i));
                        }
                    }
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            if (list.Count == 1)
            {
                result = (decimal)list.ElementAt(0);
            }
        }
        */
        #endregion
    }
}