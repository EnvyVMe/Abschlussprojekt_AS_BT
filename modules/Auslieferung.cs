using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Diagnostics;

using System.IO.Compression;
using System.Collections;

namespace BuildToolService
{
    public class Auslieferung
    {
        public static string Erstellen(string as_auslieferung, string as_versionVon, string as_versionBis, string as_fixVon, string as_fixBis, string as_kunde, string as_installationsroutineErstellen, string as_unterfixeBeruecksichtigen, string as_neueTodo, string as_ef3ordnerErstellen, string as_lokalErstellen, string as_woechentlichenBuild, string as_32bit, string as_internAuslieferung)
        {

            string dateiname, zielpfad, fixnr="", fixnr_zaehler, versionsPrefix="", kundenKuerzel = Data.GetKundenKuerzel(as_kunde), weitereSkripte = Data.GetWeitereSkripte(as_kunde), fixpath="", fixtocopy, serverpfad, clientpfad, migrationspfad="", schluesselpfad, auslieferungspfad;
            string[] las_versionBis;
            bool webRelevant = false, containsFix = true, webRelevantStd = false, clientIsEmpty = false;
            int abversion64;
            if (as_auslieferung == "2")
            {
                as_versionBis = as_versionVon;
            }

            las_versionBis = as_versionBis.Split('.');
            versionsPrefix = las_versionBis[0] + "." + las_versionBis[1];
            if(versionsPrefix == "4.3")
            {
                migrationspfad = Data.GrossMigration5 + Data.GrossMigration1 + versionsPrefix + Data.GrossMigration2 + "Migration_" + as_versionVon + "_" + as_versionBis + "_" + kundenKuerzel.ToUpper();
            }
            else
            {
                migrationspfad = Data.GrossMigration3 + Data.GrossMigration1 + versionsPrefix + Data.GrossMigration2 + "Migration_" + as_versionVon + "_" + as_versionBis + "_" + kundenKuerzel.ToUpper();
            }
            
            
            #region Variablen zusammensetzen
            if (as_auslieferung == "1") {
                if (as_fixBis == "" || as_fixBis == null)
                {
                    fixpath = GetSourcePath(as_versionBis);//Pfad der Version
                    containsFix = checkFix(fixpath);
                    if (containsFix)
                    {
                        if (!isFixClosed(fixpath + "\\Fix_01"))
                        {
                            throw new ArgumentException("Fix ist nicht geschlossen: \"" + fixnr + "\"");
                        }
                        fixnr = FindClosedFix(fixpath);
                    }
                }
                else {
                    fixpath = GetSourcePath(as_versionBis);
                    fixnr = int.Parse(as_fixBis).ToString("00");
                    if (!isFixClosed(fixpath + "\\Fix_" + fixnr))
                    {
                        throw new ArgumentException("Fix ist nicht geschlossen: \"" + fixnr + "\"");
                    }
                }
            }
            if (as_auslieferung == "2")
            {
                fixpath = GetSourcePath(as_versionVon);//Pfad der Version

                if(as_internAuslieferung == null)
                {
                    if (!isFixClosed(fixpath + "\\Fix_01"))
                    {
                        throw new ArgumentException("Fix ist nicht geschlossen: \"" + fixnr + "\"");
                    }

                    fixnr = int.Parse(as_fixBis).ToString("00");
                    if (!isFixClosed(fixpath + "\\Fix_" + fixnr))
                    {
                        throw new ArgumentException("Fix ist nicht geschlossen: \"" + fixnr + "\"");
                    }
                }
                else
                {
                    fixnr = int.Parse(as_fixBis).ToString("00");
                }

                         
            }
            abversion64 = int.Parse(las_versionBis[0] + las_versionBis[1] + las_versionBis[2]);

             if(as_auslieferung=="1" && as_32bit == null && abversion64 >= 42116 && containsFix == false)
             {
                dateiname = as_versionBis + "_K_" + kundenKuerzel.ToUpper() + "x64";
             }
            else if (as_auslieferung == "1" && as_32bit == null && abversion64 >= 42116 && containsFix == true)
            {
                dateiname = as_versionBis + "." + fixnr + "_K_" + kundenKuerzel.ToUpper() + "x64";
            }
            else if (containsFix)
            {
                if(as_internAuslieferung ==null)
                {
                    dateiname = as_versionBis + "." + fixnr + "_K_" + kundenKuerzel.ToUpper();
                }
                else
                {
                    dateiname = as_versionBis + "." + fixnr + "_K_" + kundenKuerzel.ToUpper() + "(OPEN)";
                }

            }
            else 
            {
                dateiname = as_versionBis + "_K_" + kundenKuerzel.ToUpper();
            }

             if(as_internAuslieferung!= null)
             {
                zielpfad = Data.AuslieferungsOrdnerIntern + versionsPrefix + "\\" + as_versionBis + "\\" + dateiname;
                auslieferungspfad = Data.AuslieferungsOrdnerIntern + versionsPrefix + "\\" + as_versionBis;
             }
            
            else if (as_lokalErstellen != null)
            {
                zielpfad = Data.AuslieferungsOrdnerLokal + versionsPrefix + "\\" + as_versionBis + "\\" + dateiname;
                auslieferungspfad = Data.AuslieferungsOrdnerLokal + versionsPrefix + "\\" + as_versionBis;
            }
            else
            {
                zielpfad = Data.AuslieferungsOrdner + versionsPrefix + "\\" + as_versionBis + "\\" + dateiname;
                auslieferungspfad = Data.AuslieferungsOrdner + versionsPrefix + "\\" + as_versionBis;
            }

            #endregion
            #region Eingabenprüfung
            // Version (x.x.xxx) + Kunde vorhanden + Migrationsordner vorhanden
            Regex regex = new Regex(@"\A[0-9]\.[0-9]\.[0-9]{3}\z");
            if (!regex.IsMatch(as_versionVon))
            {
                throw new ArgumentException("Version: \"" + as_versionVon + "\" ist ungültig!", "version");
            }
            if (as_auslieferung == "1") {
                if (!regex.IsMatch(as_versionBis))
                {
                    throw new ArgumentException("Version: \"" + as_versionBis + "\" ist ungültig!", "version");
                }
            }
            if (!Data.GetClients().Contains<string>(as_kunde.ToLower()))
            {
                throw new ArgumentException("Kunde: \"" + as_kunde + "\" ist ungültig!", "kunde");
            }
            if (as_auslieferung == "1") {
                if (!Directory.Exists(migrationspfad))
                {
                    //BT-18 migrationsdatei mit Kundenkuerzel erzeugen
                    migrationspfad = Data.GrossMigration4 + Data.GrossMigration1 + versionsPrefix + Data.GrossMigration2 + "Migration_" + as_versionVon + "_" + as_versionBis + "_" + kundenKuerzel.ToUpper();
                    if (!Directory.Exists(migrationspfad))
                    {
                        throw new ArgumentException("Verzeichnis: \"" + migrationspfad + "\" nicht vorhanden!", "Migrationsordner");
                    }
                }
            }
            //BT-11
            PruefeFreigabeSchluessel(zielpfad, versionsPrefix, kundenKuerzel);

            #endregion
            //Serverordner
            #region Ordner erstellen
            bool erstellenOrdner = false;
            
            if (!Directory.Exists(zielpfad))
            {
                
                Directory.CreateDirectory(zielpfad);

                serverpfad = zielpfad + "\\Server";
                Directory.CreateDirectory(serverpfad);
                //clientpfad = zielpfad + "\\Client";
                erstellenOrdner = true;
            }
            else if (!erstellenOrdner)
            {
                int auslieferungZaehler = 10;
                for (int zaehler = 1; zaehler <= auslieferungZaehler; zaehler++)
                {

                    string auslieferungName = dateiname + "_" + zaehler.ToString();
                    zielpfad = Path.Combine(auslieferungspfad, auslieferungName);
                    if (!Directory.Exists(zielpfad))
                    {
                        Directory.CreateDirectory(zielpfad);

                        serverpfad = zielpfad + "\\Server";
                        Directory.CreateDirectory(serverpfad);
                        //clientpfad = zielpfad + "\\Client";
                        erstellenOrdner = true;
                        break;
                    }


                }
            }
            if(!erstellenOrdner)
            {
                throw new ArgumentException("Mehr als 11 Auslieferungen können nicht erstellt werden");
            }
            serverpfad = zielpfad + "\\Server";
            clientpfad = zielpfad + "\\Client";
            
            #endregion
            #region SourceDateien kopieren
            //1 = mehrere Versionen /2 = bestimmte Version
            if (as_auslieferung == "1") {
                //Mit DirectoryInfo anpassen und keine Dateien von anderen Kunden kopieren
                if (containsFix) {
                    fixnr_zaehler = fixnr;
                    while (int.Parse(fixnr_zaehler) != 0)
                    {
                        fixtocopy = fixpath + "//Fix_" + fixnr_zaehler;
                        var dirx = new DirectoryInfo(fixtocopy);
                        foreach (var file in dirx.EnumerateFiles())
                        {
                            CopyFiles(fixtocopy, serverpfad);
                        }
                        fixnr_zaehler = ((int.Parse(fixnr_zaehler)) - 1).ToString("00");
                    }
                }
                CopyFiles(fixpath, serverpfad);
            }
            if (as_auslieferung == "2") {
                fixnr_zaehler = fixnr;
                int fixnr_abbruch = int.Parse(as_fixVon);
                while (int.Parse(fixnr_zaehler) >= fixnr_abbruch)
                {
                    fixtocopy = fixpath + "//Fix_" + fixnr_zaehler;
                    if(as_internAuslieferung != null)
                    {
                        if (Directory.Exists(fixtocopy))
                        {
                            var dirx = new DirectoryInfo(fixtocopy);
                            foreach (var file in dirx.EnumerateFiles())
                            {
                                CopyFiles(fixtocopy, serverpfad);
                            }
                            fixnr_zaehler = ((int.Parse(fixnr_zaehler)) - 1).ToString("00");
                        }
                        else
                        {
                            fixtocopy = fixpath + "//Fix_" + fixnr_zaehler + "_LOCK";
                            var dirx = new DirectoryInfo(fixtocopy);
                            foreach (var file in dirx.EnumerateFiles())
                            {
                                CopyFiles(fixtocopy, serverpfad);
                            }
                            fixnr_zaehler = ((int.Parse(fixnr_zaehler)) - 1).ToString("00");
                        }
                    }
                    else
                    {
                        var dirx = new DirectoryInfo(fixtocopy);
                        foreach (var file in dirx.EnumerateFiles())
                        {
                            CopyFiles(fixtocopy, serverpfad);
                        }
                        fixnr_zaehler = ((int.Parse(fixnr_zaehler)) - 1).ToString("00");
                    }
                    
                    
                }
            }
            #endregion
            #region MigDateienkopieren
            if (as_auslieferung == "1")
            {
                var dir = new DirectoryInfo(migrationspfad);
                foreach (var file in dir.EnumerateFiles())
                {
                    CopyFiles(migrationspfad, serverpfad);
                }
            }
            #endregion
            #region Schreibschutz entfernen
            SchreibschutzEntfernen(serverpfad, false);
            #endregion
            #region Dateienlöschen
            //BT-28 -->add \.
            string kuerzelPattern = @"^.*_k_" + kundenKuerzel.ToLower() + "[._].*$";
            string weitereSkriptePattern = @"^.*_k_" + weitereSkripte.ToLower() + "[._].*$";
            string kundenPattern = @"^.*_k_.*$";

            //löschen von kundendateien
            var dirs = new DirectoryInfo(serverpfad);
            List<string> ll_sourcedateien = new List<string>();
            List<string> ll_dateitypen = new List<string>();

            foreach (var file in dirs.EnumerateFiles("*_k_*"))
            {
                string filecheck = file.ToString();
                filecheck = filecheck.ToLower();
                if (Regex.IsMatch(filecheck, kuerzelPattern) || Regex.IsMatch(filecheck, weitereSkriptePattern)) { }
                else if (Regex.IsMatch(filecheck, kundenPattern)) { File.Delete(Path.Combine(serverpfad, filecheck)); }
            }
            //löschen von dateitypen
            //var dir = new DirectoryInfo(zielPfad);
            foreach (var dateityp in Data.GetAuslieferunglöschen())
            {
                foreach (var file in dirs.EnumerateFiles(dateityp))
                {
                    //string filecheck = file.ToString();
                    File.Delete(Path.Combine(serverpfad + @"\" + file));
                }
            }

            //fremde sprachen löschen BT-14
            const Int32 BufferSize = 128;
            // flag true => file delete
            bool eng_flag = false, esp_flag = false;
            //BT-12
            using (var fileStream = File.OpenRead(Data.KundenKennung))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize)) {
                string kennung = Data.GetKundenKennung(as_kunde);
                string line,ss_nein = "Nein";
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line != "")
                    {
                        if (line.IndexOf(kennung.ToString(), 0, kennung.Length) == 0)
                        {
                            if (line.IndexOf(ss_nein, 67, ss_nein.Length) == 67)
                            {
                                eng_flag = true;
                            }
                            if (line.IndexOf(ss_nein, 88, ss_nein.Length) == 88)
                            {
                                esp_flag = true;
                            }
                        }
                    }
                }
            }

            if (eng_flag) {
                //string[] masks = { "*_eng*", "*_english*", "_englisch" };
                string[] masks = Data.GetWebserviceEnglisch();
                foreach (var file in masks.SelectMany(dirs.EnumerateFiles))
                {
                    File.Delete(Path.Combine(serverpfad + @"\" + file));
                }
            }
            if (esp_flag) {
                //string[] masks = { "*_esp*", "*_spanish*", "_spanisch" };
                string[] masks = Data.GetWebserviceSpanisch();
                foreach (var file in masks.SelectMany(dirs.EnumerateFiles))
                {
                    File.Delete(Path.Combine(serverpfad + @"\" + file));
                }
            }

            #endregion
            #region Dateien erzeugen
            //Webservice relevanz
            if (as_ef3ordnerErstellen != null)
            {
                webRelevant = CreateWebserviceDirectory(zielpfad, as_versionBis, versionsPrefix, as_fixVon, fixnr, as_auslieferung, kundenKuerzel);
            }
            // Webservice Standart
            int ab_version = int.Parse(las_versionBis[0] + las_versionBis[1] + las_versionBis[2]);
            if(as_kunde == "riw" || ab_version >= 42079)
            {
                webRelevantStd = CreateWebserviceDirectoryStandard(zielpfad, as_versionBis, versionsPrefix, as_fixVon, fixnr, as_auslieferung);
            }
            CreateServerToDo(serverpfad, as_versionVon, as_versionBis, as_fixVon, fixnr, as_kunde, fixpath, as_auslieferung, containsFix, webRelevant, as_internAuslieferung);
            #endregion
            //Clientordner
            CreateClientDirectory(clientpfad, as_versionBis, as_fixVon, fixnr, as_kunde, as_auslieferung, as_installationsroutineErstellen, containsFix, as_32bit, as_internAuslieferung);
            
            //Freigabeschluessel kopieren
            CopyFreigabeSchluessel(zielpfad, versionsPrefix, kundenKuerzel);
            //PDF-Druck kpoieren

            if (as_auslieferung == "1")
            {
                if (as_woechentlichenBuild != null)
                {

                }
                else
                {
                    CopyPdfDruck(zielpfad, versionsPrefix);
                }

            }
            if (as_auslieferung == "2")
            {
                IrrlevantSkriptenEntfernen(serverpfad, as_versionBis, as_fixBis);
            }

            //Todo anlegen
            if (as_neueTodo != null)
            {
                MigrationCmdErzeugen(zielpfad);
                MigrationSqlErzeugen(serverpfad, as_versionBis, fixnr, containsFix);
            }

            //Todoerstellen
            clientIsEmpty = IsDirectoryEmpty(clientpfad);
            CreateAuslieferungTodo(zielpfad, as_versionBis, as_versionVon, fixnr, webRelevant, as_neueTodo, as_installationsroutineErstellen, containsFix, as_auslieferung, webRelevantStd, as_woechentlichenBuild, clientIsEmpty);
            
            //Installationsroutine
            if (as_installationsroutineErstellen != null)
            {
                if  (as_auslieferung == "1")
                {
                    InstTemplateverarbeiten(zielpfad, clientpfad, as_versionBis, fixnr, containsFix);
                }
                if  (as_auslieferung == "2")
                {
                    FixTemplateverarbeiten(zielpfad, clientpfad, as_versionBis, as_fixVon, as_fixBis, containsFix);
                }
            }
            //Cleanup
            if (clientIsEmpty)
            {
                if (Directory.Exists(clientpfad)) {
                    SchreibschutzEntfernen(clientpfad, true);
                    Directory.Delete(clientpfad, true);
                }
            }

            VerzeichnisKomprimieren(zielpfad, auslieferungspfad, dateiname, as_versionBis,as_32bit, as_auslieferung);
            
            return "Ordner erfolgreich erstellt und zu finden unter: " + zielpfad;
        }
        private static bool CreateWebserviceDirectory(string as_targetDir, string as_versionBis, string as_versionPrefix, string as_FixVon, string as_highestFix, string as_auslArt, string as_kundenKuerzel)
        {
            string as_ef3Webservice = as_targetDir + "\\ef3Webservice",
                    as_efOnline = as_ef3Webservice + "\\efOnline",
                    as_ef3 = as_ef3Webservice + "\\ef3",
                    as_webEinstellungen = Data.Webservice3,
                    as_webPortal = Data.Webservice1 + "\\" + as_versionPrefix + Data.Webservice2,
                    //as_webShared = Data.Webservice1 + "\\" + as_versionPrefix + "\\shared",
                    as_webPruefung,
                    as_webPruefung_00,
                    as_webTypeFiles = "",
                    as_filePath;


            string[] las_ordner = Data.GetWebserviceOrdner(),
                     las_eintrag = Data.GetWebserviceEintrag();

            int zaehler = 0, fixVon = 0, as_fixNr, fixV, fixB;

            bool fix_not_found,
                 ret_val = false,
                 ret_val_ws = false,
                 ret_val_ef3 = false,
                 ret_val_ws_00 = false,
                 ret_val_ef3_00 = false;

            if (as_auslArt == "1")
            {
                as_webTypeFiles = Data.Webservice1 + "\\" + as_versionPrefix + Data.WebserviceInstallation;
            }

            if (as_auslArt == "2")
            {
                as_webTypeFiles = Data.Webservice1 + "\\" + as_versionPrefix + Data.WebserviceAktualisierung;
                fixVon = int.Parse(as_FixVon);
            }

            Directory.CreateDirectory(as_ef3Webservice);
            Directory.CreateDirectory(as_efOnline);
            Directory.CreateDirectory(as_ef3);
            #region efOnline
            //CopyFiles(as_webShared, as_ef3Webservice);
            CopyFiles(as_webTypeFiles, as_ef3Webservice);
            // kopiert die *.msi Fix_00
            if(as_auslArt == "1")
            {
                while (zaehler < las_ordner.Length)
                {
                    as_webPruefung_00 = as_webPortal + las_ordner[zaehler] + "\\" + as_versionBis + "\\" + as_versionBis + ".00";
                    string as_webPruefung_ = as_webPortal + las_ordner[zaehler] + "\\" + as_versionBis;

                    if (Directory.Exists(as_webPruefung_00))
                    {
                        var dir_web_00 = new DirectoryInfo(as_webPruefung_00);
                        string[] files = Directory.GetDirectories(as_webPruefung_);
                        if (files.Length == 1)
                        {
                            foreach (var file_msi in dir_web_00.EnumerateFiles("*.msi"))
                            {
                                as_filePath = as_efOnline + "\\" + file_msi.Name;
                                file_msi.CopyTo(as_filePath, false);
                            }
                            ret_val_ws_00 = true;

                        }
                        else if(files.Length > 1)
                        {
                            string pfad_web_portal = files[1]; //4.2.013.03  erstellung bis Fix 3
                            string [] pfad_web_portal_split = pfad_web_portal.Split('.');
                            int dirOrdnerFix_web = int.Parse(pfad_web_portal_split[6]); //03
                            as_fixNr = int.Parse(as_highestFix);
                            if (dirOrdnerFix_web > as_fixNr)
                            {
                                foreach (var file_msi in dir_web_00.EnumerateFiles("*.msi"))
                                {
                                    as_filePath = as_efOnline + "\\" + file_msi.Name;
                                    file_msi.CopyTo(as_filePath, false);
                                }
                                ret_val_ws_00 = true;
                            }
                           
                        }
                    }
                 zaehler++;
                }                
            }
            zaehler = 0;

            while (zaehler < las_ordner.Length)
            {
                fix_not_found = true;
                as_fixNr = int.Parse(as_highestFix);
                while (fix_not_found)
                {
                    ///// hier webpruefung erweiter BT0_8
                    as_webPruefung = as_webPortal + las_ordner[zaehler] + "\\" + as_versionBis + "\\" + as_versionBis + "." + as_fixNr.ToString("00");
                    if (Directory.Exists(as_webPruefung))
                    {
                        var dir_web = new DirectoryInfo(as_webPruefung);
                        foreach (var file_msi in dir_web.EnumerateFiles("*.msi"))
                        {
                            as_filePath = as_efOnline + "\\" + file_msi.Name;
                            file_msi.CopyTo(as_filePath, false);
                        }
                        fix_not_found = false;
                        ret_val_ws = true;
                    }
                    else
                    {
                        as_fixNr--;
                        //testing
                        if (as_auslArt == "1" && 0 == as_fixNr) { fix_not_found = false; }
                        if (as_auslArt == "2" && fixVon > as_fixNr) { fix_not_found = false; }
                    }
                }
                zaehler++;
            }
            SchreibschutzEntfernen(as_ef3Webservice, false);
            SchreibschutzEntfernen(as_efOnline, false);
            if (!ret_val_ws && !ret_val_ws_00) { Directory.Delete(as_efOnline, true); }

            #endregion
            #region ef3
            string[] las_relevanteFreigaben = Data.GetWebserviceKundenRelevanteFreigaben();
            List<string> l_kundenFreigaben = new List<string>();
            string as_freigabeschlüssel = Path.Combine(Data.KundenFreigabe + "\\freigabeschluessel_k_" + as_kundenKuerzel.ToLower() + ".txt");

            if (File.Exists(as_freigabeschlüssel))
            {
                string line;
                System.IO.StreamReader alter = new System.IO.StreamReader(as_freigabeschlüssel, Encoding.Default);
                for (int i = 0; (line = alter.ReadLine()) != null; i++)
                {
                    foreach (var freigabe in las_relevanteFreigaben) {
                        if (line.StartsWith(freigabe + "	"))
                        {
                            l_kundenFreigaben.Add(freigabe);
                        }
                    }
                }
            }
            string[] las_kundenFreigaben = l_kundenFreigaben.ToArray();
            string s_fixDir;
            string[] las_versionBis = as_versionBis.Split('.');
            string webpath = Data.Webservice1 + "\\" + las_versionBis[0] + "." + las_versionBis[1];

            foreach (var k_freigabe in las_kundenFreigaben) {
                string[] to_copy = Data.GetWebserviceFreigabenPfade(k_freigabe);
                foreach (var pfad in to_copy) {
                    s_fixDir = webpath + "\\" + pfad + "\\" + as_versionBis;
                    // kopiert die *.msi Fix_00 
                    if (as_auslArt == "1")
                    {
                        string[] files = Directory.GetDirectories(s_fixDir);
                        string dir_web_ef3_00_ = s_fixDir + "\\" + as_versionBis + ".00";

                        if (files.Length == 1)
                        {
                            if (Directory.Exists(dir_web_ef3_00_))
                            {
                                var dir_web_ef3_00 = new DirectoryInfo(dir_web_ef3_00_);
                                foreach (var file_msi in dir_web_ef3_00.EnumerateFiles("*.msi"))
                                {
                                    as_filePath = as_ef3 + "\\" + file_msi.Name;
                                    file_msi.CopyTo(as_filePath, false);
                                }
                                // Spezifischen *msi  für bestimmte Kunden 
                                string spezifischen_ef3_00 = dir_web_ef3_00 + "\\" + as_kundenKuerzel;
                                if(Directory.Exists(spezifischen_ef3_00))
                                {
                                    CopySpezifischenMsi(spezifischen_ef3_00, as_ef3);
                                }
                                
                                ret_val_ef3_00 = true;
                            }

                        }    

                        else if (files.Length > 1 || !Directory.Exists(dir_web_ef3_00_))
                        {
                            fixB = int.Parse(as_highestFix);
                            int highestFix = 0;
                            DirectoryInfo dir = new DirectoryInfo(s_fixDir);
                            DirectoryInfo[] dirs = dir.GetDirectories("*" + as_versionBis + "*");

                            foreach (var d in dirs)
                            {
                                string[] dirSplit = d.ToString().Split('.');
                                if (ContainsLetter(dirSplit[3]))
                                {
                                    //ignoriert fixe mit buchstaben z.B. fix01aon12
                                }
                                else
                                {
                                    int dirNr = int.Parse(dirSplit[3]);
                                    if (dirNr >= highestFix && dirNr <= fixB)
                                    {
                                        highestFix = dirNr;
                                    }
                                }
                            }

                            string fullPath = s_fixDir + "\\" + as_versionBis + "." + highestFix.ToString("00");

                            if (Directory.Exists(fullPath))
                            {
                                var dir_fix_to_copy = new DirectoryInfo(fullPath);
                                foreach (var file_msi in dir_fix_to_copy.EnumerateFiles("*.msi"))
                                {
                                    as_filePath = as_ef3 + "\\" + file_msi.Name;
                                    file_msi.CopyTo(as_filePath, false);
                                }
                                // Spezifischen *msi  für bestimmte Kunden 
                                string spezifischen_ef3 = fullPath + "\\" + as_kundenKuerzel;
                                if (Directory.Exists(spezifischen_ef3))
                                {
                                    CopySpezifischenMsi(spezifischen_ef3, as_ef3);
                                }
                                ret_val_ef3 = true;
                            }


                        }

                    }
                    
                    else if (as_auslArt == "2" && Directory.Exists(s_fixDir))
                    {
                        fixV = int.Parse(as_FixVon);
                        fixB = int.Parse(as_highestFix);
                        int highestFix = 0;

                        DirectoryInfo dir = new DirectoryInfo(s_fixDir);
                        DirectoryInfo[] dirs = dir.GetDirectories("*"+as_versionBis+"*");

                        foreach (var d in dirs) 
                        {
                            string[] dirSplit = d.ToString().Split('.');
                            if (ContainsLetter(dirSplit[3]))
                            {
                                //ignoriert fixe mit buchstaben z.B. fix01aon12
                            }
                            else
                            {
                                int dirNr = int.Parse(dirSplit[3]);
                                if (dirNr >= highestFix && dirNr >= fixV && dirNr <= fixB)
                                {
                                    highestFix = dirNr;
                                }
                            }
                        }

                        string fullPath = s_fixDir + "\\" + as_versionBis + "." + highestFix.ToString("00");

                        if (Directory.Exists(fullPath) && (highestFix != 0))
                        {
                            var dir_fix_to_copy = new DirectoryInfo(fullPath);
                            foreach (var file_msi in dir_fix_to_copy.EnumerateFiles("*.msi"))
                            {
                                as_filePath = as_ef3 + "\\" + file_msi.Name;
                                file_msi.CopyTo(as_filePath, false);
                                
                            }
                            // Spezifischen *msi  für bestimmte Kunden 
                            string spezifischen_ef3 = fullPath + "\\" + as_kundenKuerzel;
                            if (Directory.Exists(spezifischen_ef3))
                            {
                                CopySpezifischenMsi(spezifischen_ef3, as_ef3);
                            }
                            
                            ret_val_ef3 = true;
                        }
                    }
                }
            }
            
            #endregion

            SchreibschutzEntfernen(as_ef3Webservice, false);
            SchreibschutzEntfernen(as_ef3, false);

            if (!ret_val_ef3 && !ret_val_ef3_00 ) { Directory.Delete(as_ef3, true); }
            if (!ret_val_ef3 && !ret_val_ws && !ret_val_ws_00 && !ret_val_ef3_00) 
            {
                Directory.Delete(as_ef3Webservice, true); 
            }
            else
            {
                ret_val = true;
            }
            return ret_val;
        }

        private static bool CreateWebserviceDirectoryStandard(string as_targetDir, string as_versionBis, string as_versionPrefix, string as_FixVon, string as_highestFix, string as_auslArt)
        {
            string  as_filePath,
                    as_ef3Webservice = as_targetDir + "\\ef3Webservice",
                    as_ef3 = as_ef3Webservice + "\\ef3",
                    as_webTypeFiles = "";
            int  fixVon = 0, fixV, fixB;
            bool ret_val_ef3_std_00 = false,
                ret_val_ef3_std = false,
                ret_val_ef3_erstllen = false,
                ret_val_efws_efstellen = false,
                ret_val_std = false;
            string[] las_versionBis = as_versionBis.Split('.');
            string webpath = Data.Webservice1 + "\\" + las_versionBis[0] + "." + las_versionBis[1];

            string[] las_ordner_ef3_std = Data.GetWebserviceStandart();
            if(!Directory.Exists(as_ef3Webservice))
            {
                Directory.CreateDirectory(as_ef3Webservice);
                if (as_auslArt == "1")
                {
                    as_webTypeFiles = Data.Webservice1 + "\\" + as_versionPrefix + Data.WebserviceInstallation;
                }

                if (as_auslArt == "2")
                {
                    as_webTypeFiles = Data.Webservice1 + "\\" + as_versionPrefix + Data.WebserviceAktualisierung;
                    fixVon = int.Parse(as_FixVon);
                }
                CopyFiles(as_webTypeFiles, as_ef3Webservice);

                ret_val_efws_efstellen = true;
            }
            if (!Directory.Exists(as_ef3))
            {
                Directory.CreateDirectory(as_ef3);
                ret_val_ef3_erstllen = true;
            }
            

            foreach (var ef3_std in las_ordner_ef3_std)
            {
                string fullpath_ef3_std = webpath + "\\" + ef3_std + "\\" + as_versionBis;

                if (Directory.Exists(fullpath_ef3_std))
                {
                    if (as_auslArt == "1")
                    {
                        string[] files = Directory.GetDirectories(fullpath_ef3_std);
                        string dir_web_ef3_std_00 = fullpath_ef3_std + "\\" + as_versionBis + ".00";

                        if (files.Length == 1)
                        {
                            if (Directory.Exists(dir_web_ef3_std_00))
                            {
                                var dir_std_00 = new DirectoryInfo(dir_web_ef3_std_00);
                                foreach (var file_msi in dir_std_00.EnumerateFiles("*.msi"))
                                {
                                    as_filePath = as_ef3 + "\\" + file_msi.Name;
                                    file_msi.CopyTo(as_filePath, false);
                                }

                                ret_val_ef3_std_00 = true;

                            }
                        }
                        else if (files.Length > 1 || !Directory.Exists(dir_web_ef3_std_00))
                        {
                            fixB = int.Parse(as_highestFix);
                            int highestFix = 0;
                            DirectoryInfo dir = new DirectoryInfo(fullpath_ef3_std);
                            DirectoryInfo[] dirs = dir.GetDirectories("*" + as_versionBis + "*");

                            foreach (var d in dirs)
                            {
                                string[] dirSplit = d.ToString().Split('.');
                                if (ContainsLetter(dirSplit[3]))
                                {
                                    //ignoriert fixe mit buchstaben z.B. fix01aon12
                                }
                                else
                                {
                                    int dirNr = int.Parse(dirSplit[3]);
                                    if (dirNr >= highestFix && dirNr <= fixB)
                                    {
                                        highestFix = dirNr;
                                    }
                                }
                            }

                            string fullPath = fullpath_ef3_std + "\\" + as_versionBis + "." + highestFix.ToString("00");

                            if (Directory.Exists(fullPath))
                            {
                                var dir_fix_to_copy = new DirectoryInfo(fullPath);
                                foreach (var file_msi in dir_fix_to_copy.EnumerateFiles("*.msi"))
                                {

                                    as_filePath = as_ef3 + "\\" + file_msi.Name;
                                    file_msi.CopyTo(as_filePath, false);
                                }

                                ret_val_ef3_std = true;
                            }



                        }
                        


                    }
                    if(as_auslArt == "2")
                    {
                        fixV = int.Parse(as_FixVon);
                        fixB = int.Parse(as_highestFix);
                        int highestFix = 0;

                        DirectoryInfo dir = new DirectoryInfo(fullpath_ef3_std);
                        DirectoryInfo[] dirs = dir.GetDirectories("*" + as_versionBis + "*");

                        foreach (var d in dirs)
                        {
                            string[] dirSplit = d.ToString().Split('.');
                            if (ContainsLetter(dirSplit[3]))
                            {
                                //ignoriert fixe mit buchstaben z.B. fix01aon12
                            }
                            else
                            {
                                int dirNr = int.Parse(dirSplit[3]);
                                if (dirNr >= highestFix && dirNr >= fixV && dirNr <= fixB)
                                {
                                    highestFix = dirNr;
                                }
                            }
                        }

                        string fullPath = fullpath_ef3_std + "\\" + as_versionBis + "." + highestFix.ToString("00");

                        if (Directory.Exists(fullPath) && (highestFix != 0))
                        {
                            var dir_fix_to_copy = new DirectoryInfo(fullPath);
                            foreach (var file_msi in dir_fix_to_copy.EnumerateFiles("*.msi"))
                            {
                                as_filePath = as_ef3 + "\\" + file_msi.Name;
                                file_msi.CopyTo(as_filePath, false);

                            }


                            ret_val_ef3_std = true;
                             
                        }
                    }
                }
            }

            SchreibschutzEntfernen(as_ef3Webservice, false);
            SchreibschutzEntfernen(as_ef3, false);

            if (ret_val_efws_efstellen && ret_val_ef3_erstllen && !ret_val_ef3_std && !ret_val_ef3_std_00) 
            { 
                Directory.Delete(as_ef3, true);
                Directory.Delete(as_ef3Webservice, true);
            }
            if (!ret_val_efws_efstellen && ret_val_ef3_erstllen && !ret_val_ef3_std && !ret_val_ef3_std_00)
            {
                Directory.Delete(as_ef3, true); 
            }

            if (ret_val_ef3_std || ret_val_ef3_std_00)
            {
                ret_val_std = true;
            }
            else
            {
                ret_val_std = false;
            }

            return ret_val_std;
        }
        private static void CreateClientDirectory(string as_clientPath, string as_versionClient, string as_FixVon, string as_FixBis, string as_kunde, string as_auslArt, string installationFlag, bool hasFix, string as_32bit, string as_internAuslieferung)
        {
            //Erstellt das Client Verzeichnis
            Directory.CreateDirectory(as_clientPath);
            string as_exePath, as_exeHelp, as_exeShared, as_exeInfomaker, as_exeModule, as_exeKunde, as_exeSource, as_versionMain, as_fixPath, as_kundepbdPattern, as_filePath, as_filetocopy, as_filetocheck, as_vorlagenIni, as_sourceExt, checkShared, as_exeShared64, as_exeShared32;
            string[] las_versionClient, las_kunde_pbd = Data.GetKundenPBD(as_kunde);
            int li_zaehler_pbds, li_abbruch=1, abverion64;

            las_versionClient = as_versionClient.Split('.');
            as_versionMain = las_versionClient[0] + '.' + las_versionClient[1];
            abverion64 = int.Parse(las_versionClient[0] + las_versionClient[1] + las_versionClient[2]);
            as_vorlagenIni = Data.VorlagenIni;
            as_exePath = Data.Auslieferung1 + as_versionMain + "\\" + as_versionClient + Data.Auslieferung2;
            as_exeShared64 = Data.Auslieferung1 + as_versionMain + Data.sharedpfad64;
            as_exeShared32 = Data.Auslieferung1 + as_versionMain + Data.sharedpfad32;
            as_exeSource = as_exePath + as_versionMain + "\\Source";
            
            as_exeKunde = as_exeSource + "\\Kunde";
            as_exeHelp = as_exePath + "\\help";
            as_exeShared = as_exePath + "\\shared";
            as_exeInfomaker = as_exeSource + "\\Infomaker";
            as_exeModule = as_exeSource + "\\Module";

            if (as_auslArt == "2") {
                li_abbruch = int.Parse(as_FixVon);
            }

            #region Kein Source Ordner(SourceOrdnerAlteMethode) 
            //BT-05 BT-40
            if (installationFlag != null || as_auslArt == "2")
            {
                as_sourceExt = "";
            }
            else {
                as_sourceExt = "//Source";
                if (!Directory.Exists(as_clientPath + as_sourceExt))
                {
                    Directory.CreateDirectory(as_clientPath + as_sourceExt);
                }
            }
            #endregion
            
            var dir_source = new DirectoryInfo(as_exeSource);

            #region Fixdateien kopieren
            if (hasFix) {
                while (li_abbruch <= int.Parse(as_FixBis))
                {
                    as_fixPath = as_exeSource + "//Fix_" + as_FixBis;
                    if(as_internAuslieferung != null)
                    {
                        if (Directory.Exists(as_fixPath))
                        {

                        }
                        else
                        {
                            as_fixPath = as_exeSource + "//Fix_" + as_FixBis + "_LOCK";
                        }
                    }
                    
                    var dir_currentFix = new DirectoryInfo(as_fixPath);
                    foreach (var file_pbd in dir_currentFix.EnumerateFiles("*.pbd"))
                    {
                        // prüfen auf fremde efcudateien
                        as_filetocheck = file_pbd.ToString();
                        if (Regex.IsMatch(as_filetocheck, @"^.*efcu.*$"))
                        {
                            int zaehler_pbds = 0;
                            while (zaehler_pbds < las_kunde_pbd.Length)
                            {
                                as_kundepbdPattern = @".*efcu" + las_kunde_pbd[zaehler_pbds].ToLower() + "\\..*$";
                                if (Regex.IsMatch(as_filetocheck, as_kundepbdPattern))
                                {
                                    as_filePath = as_clientPath + as_sourceExt + "\\" + file_pbd.Name;
                                    if (!File.Exists(as_filePath))
                                    {
                                        file_pbd.CopyTo(as_filePath);
                                    }
                                }
                                zaehler_pbds++;
                            }
                        }
                        else
                        {
                            as_filePath = as_clientPath + as_sourceExt + "\\" + file_pbd.Name;
                            if (!File.Exists(as_filePath))
                            {
                                file_pbd.CopyTo(as_filePath);
                            }
                        }
                    }

                    foreach (var dateityp in Data.GetFixWhitelist())
                        foreach (var file in dir_currentFix.EnumerateFiles(dateityp))
                        {
                            as_filePath = as_clientPath + as_sourceExt + "\\" + file.Name;
                            if (!File.Exists(as_filePath))
                            {
                                file.CopyTo(as_filePath);
                            }
                        }
                    //Shared - Ordner in Fix - Ordner
                    checkShared = as_fixPath + "\\shared";
                    if (Directory.Exists(checkShared))
                    {
                        foreach (var file in Directory.GetFiles(checkShared))
                        {

                            as_filePath = as_clientPath + as_sourceExt;
                            if (!File.Exists(as_filePath))
                            {
                                CopyFiles(checkShared, as_filePath);
                            }



                        }
                        // unterordner in Shared-Ordner 
                        if(Directory.GetDirectories(checkShared).Length > 0)
                        {
                            foreach (var dir in Directory.GetDirectories(checkShared))
                            {
                                
                                as_filePath = as_clientPath + as_sourceExt;
                                Directory.CreateDirectory(Path.Combine(as_filePath, Path.GetFileName(dir)));

                                foreach (var file in Directory.GetFiles(dir))
                                {

                                    as_filePath = as_clientPath + as_sourceExt +"\\"+ Path.GetFileName(dir); 
                                    if (!File.Exists(as_filePath))
                                    {
                                        CopyFiles(dir, as_filePath);
                                    }



                                }


                            }
                        }
                    }
                    
                    else
                    {

                    }
                    as_FixBis = ((int.Parse(as_FixBis)) - 1).ToString("00");
                }
            }
            #endregion

            if (as_auslArt == "1") {
                #region Sourcedateien kopieren
                foreach (var file_source in dir_source.EnumerateFiles())
                {
                    // prüfen auf fremde efcudateien
                    as_filetocheck = file_source.ToString();
                    if (Regex.IsMatch(as_filetocheck, @"^.*efcu.*.pbd$"))
                    {
                        li_zaehler_pbds = 0;
                        while (li_zaehler_pbds < las_kunde_pbd.Length)
                        {
                            as_kundepbdPattern = @".*efcu" + las_kunde_pbd[li_zaehler_pbds].ToLower() + "\\..*$";
                            if (Regex.IsMatch(as_filetocheck, as_kundepbdPattern))
                            {
                                as_filePath = as_clientPath + as_sourceExt + "\\" + file_source.Name;
                                if (!File.Exists(as_filePath))
                                {
                                    file_source.CopyTo(as_filePath);
                                }
                            }
                            li_zaehler_pbds++;
                        }
                    }
                    else
                    {
                        as_filePath = as_clientPath + as_sourceExt + "\\" + file_source.Name;
                        if (!File.Exists(as_filePath))
                        {
                            file_source.CopyTo(as_filePath);
                        }
                    }
                }
                #endregion
                #region Kundendateien kopieren

                li_zaehler_pbds = 0;
                while (li_zaehler_pbds < las_kunde_pbd.Length)
                {
                    as_filetocopy = as_exeKunde + "\\efcu" + las_kunde_pbd[li_zaehler_pbds].ToLower() + ".pbd";
                    as_filePath = as_clientPath + as_sourceExt + "\\efcu" + las_kunde_pbd[li_zaehler_pbds].ToLower() + ".pbd";
                    if (!File.Exists(as_filePath))
                    {
                        File.Copy(as_filetocopy, as_filePath, false);
                    }
                    li_zaehler_pbds++;
                }


                #endregion
                #region Module/Infomaker/help/shared/efr2 kopieren
                //BT-5 alte Ordnerstruktur wenn keine Installationsroutine ausgewählt wird
                if (installationFlag != null)
                {
                    //keine Ordnerstruktur
                    CopyFiles(as_exeHelp, as_clientPath);

                    // Version 4.2.116 wird Shared-Ordner zentral kopiert
                    //für 32bit
                    if (abverion64 >= 42116 && as_32bit != null)
                    {
                        CopyFiles(as_exeShared32, as_clientPath);
                    }
                    // für 64bit
                    else if(abverion64 >= 42116)
                    {
                        CopyFiles(as_exeShared64, as_clientPath);
                    }
                    // vor die Version 4.2.116 wird Shared-Ordner von unterordner kopiert
                    else
                    {
                        CopyFiles(as_exeShared, as_clientPath);
                    }
                    CopyFiles(as_exeInfomaker, as_clientPath);
                    CopyFiles(as_exeModule, as_clientPath);
                }
                else {
                    //"alte"-Methode
                    CopyDirectory(as_exeHelp, as_clientPath + "\\help", false);
                    // Version 4.2.116 wird Shared-Ordner zentral kopiert
                    //für 32bit
                    if (abverion64 >= 42116 && as_32bit != null)
                    {
                        
                        CopyDirectory(as_exeShared32, as_clientPath + "\\Shared", false);
                    }
                    //für 64bit
                    else if (abverion64 >= 42116)
                    {
                        CopyDirectory(as_exeShared64, as_clientPath + "\\Shared", false);
                    }
                    // vor die Version 4.2.116 wird Shared-Ordner von unterordner kopiert 
                    else
                    {
                        CopyDirectory(as_exeShared, as_clientPath + "\\Shared", false);
                    }
                    CopyDirectory(as_exeInfomaker, as_clientPath + "\\Infomaker", false);
                    CopyDirectory(as_exeModule, as_clientPath + "\\Source", false);
                }
                if (abverion64 >= 42116 && as_32bit == null)
                {
                    as_filetocopy = as_exePath + as_versionMain + "\\ef64.exe";
                    as_filePath = as_clientPath + "\\ef.exe";
                    File.Copy(as_filetocopy, as_filePath, false);
                }
                else
                {
                    as_filetocopy = as_exePath + as_versionMain + "\\efr2.exe";
                    as_filePath = as_clientPath + "\\ef.exe";
                    File.Copy(as_filetocopy, as_filePath, false);
                }

                //vorlage_ef.ini
                as_filetocopy = as_vorlagenIni;
                as_filePath = as_clientPath + as_sourceExt + "\\vorlage_ef.ini";
                File.Copy(as_filetocopy, as_filePath, false);

                SchreibschutzEntfernen(as_clientPath, true);

                #endregion
            }
        }
        public static string FindClosedFix(string as_sourcePath)
        {
            //Bestimmt die höchte geschlossene Fixversion
            int li_highestClosedFix = 01;
            string as_dateiname, as_fixpath, as_highestClosedFix = "01";
            bool fixisClosed = true;

            while (fixisClosed)
            {
                as_dateiname = "\\Fix_" + li_highestClosedFix.ToString("00");
                as_fixpath = as_sourcePath + as_dateiname;
                if (Directory.Exists(as_fixpath) && fixisClosed)
                {
                    if (File.Exists(as_fixpath + "\\CLOSED.txt") || File.Exists(as_fixpath + "\\close.txt"))
                    {
                        as_highestClosedFix = li_highestClosedFix.ToString("00");
                        li_highestClosedFix++;
                    }
                    else
                    {
                        fixisClosed = false;
                    }
                }
                else
                {
                    fixisClosed = false;
                }
            }
            return as_highestClosedFix;
        }
        public static bool checkFix(string as_sourcePath) 
        {
            bool containsFix = true;
            string as_dateiname = as_sourcePath + "\\Fix_01";

            if (Directory.Exists(as_dateiname))
            {
                containsFix = true;
            }
            else 
            {
                containsFix = false;
            }
            return containsFix;
        }
        public static bool isFixClosed(string as_fixPath)
        {
            if (Directory.Exists(as_fixPath))
            {
                if (File.Exists(as_fixPath + "\\CLOSED.txt") || File.Exists(as_fixPath + "\\close.txt"))
                {                    
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new ArgumentException("Ordner existiert nicht: {0}", as_fixPath);
            }

            return true;
        }
        public static string GetSourcePath(string as_versionBis)
        {
            //Gibt gewünschten Pfad aus config.ini Datei zurück
            string as_sourcePath, as_oberversion;
            string[] las_Version;

            las_Version = as_versionBis.Split('.');
            as_oberversion = las_Version[0] + "." + las_Version[1];
            as_sourcePath = Data.Auslieferung1 + as_oberversion + "\\" + as_versionBis + Data.Auslieferung2 + as_oberversion + "\\Source";

            return as_sourcePath;
        }
        private static void CopyFiles(string sourceDir, string targetDir)
        {
            //Kopiert alle Dateien von sourceDir in targetDir OHNE ÜBERSCHREIBEN
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
        private static void CopyDirectory(string sourceDir, string targetDir, bool subDirectory) {

            if (!Directory.Exists(targetDir)) {
                Directory.CreateDirectory(targetDir);
            }

            //Kopiert alle Dateien von sourceDir in targetDir OHNE ÜBERSCHREIBEN
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string filePath = Path.Combine(targetDir, Path.GetFileName(file));
                if (!File.Exists(filePath))
                {
                    File.Copy(file, filePath, false);
                }
            }

            //Falls man unterordner mitkopieren möchte
            if (subDirectory) {
                foreach (var directory in Directory.GetDirectories(sourceDir))
                {
                    File.Copy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
                }
            }
        }
        private static void SchreibschutzEntfernen(string targetDir, bool subDirectory)
        {
            //Entferntschreibschutz aus Zielverzeichnis
            foreach (var datei in Directory.GetFiles(targetDir))
            {
                string filepath = Path.Combine(targetDir, Path.GetFileName(datei));
                FileInfo fileInfo = new FileInfo(filepath);
                fileInfo.IsReadOnly = false;
            }

            //unterordner
            if (subDirectory)
            {
                foreach (var directory in Directory.GetDirectories(targetDir))
                {
                    foreach (var datei in Directory.GetFiles(directory))
                    {
                        string filepath = Path.Combine(directory, Path.GetFileName(datei));
                        FileInfo fileInfo = new FileInfo(filepath);
                        fileInfo.IsReadOnly = false;
                    }
                }
            }
        }
        private static void CreateServerToDo(string targetDir, string as_versionVon, string as_versionBis, string as_FixVon, string as_FixBis, string as_kunde, string as_fixpath, string as_auslArt, bool hasFix, bool webRelevant, string as_internAuslieferung)
        {
            int zaehler = 0;
            string dateiName;
            int highest_fix = 0;
            if (hasFix)
            {
                highest_fix = int.Parse(as_FixBis);
                dateiName = "todo_" + as_versionBis + "." + as_FixBis + ".sql";
            }
            else
            {
                dateiName = "todo_" + as_versionBis + ".sql";
            }

            if (as_auslArt == "1")
            {
                zaehler = 1;
            }
            if (as_auslArt == "2")
            {
                zaehler = int.Parse(as_FixVon);
            }

            string fixpath;
            string[] las_ordner = Data.GetWebserviceOrdner();
            string[] las_eintrag = Data.GetWebserviceEintrag();
            string[] las_eintrag_ef3 = Data.GetWebserviceEintrag_ef3();
            string[] las_eintarg_ef3_std = Data.GetWebserviceStandartEintrag();
            string[] las_ordner_ef3_std = Data.GetWebserviceStandart();
            string[] las_versionBis = as_versionBis.Split('.');
            string webpath = Data.Webservice1 + "\\" + las_versionBis[0] + "." + las_versionBis[1] + Data.Webservice2;
            string webpath_ef3 = Data.Webservice1 + "\\" + las_versionBis[0] + "." + las_versionBis[1];
            string[] las_kunde_pbd = Data.GetKundenPBD(as_kunde);
            string filecheck;
            string efcu_pattern = @"^.*efcu.*$";
            string kundepbd_pattern;
            string[] las_relevanteFreigaben = Data.GetWebserviceKundenRelevanteFreigaben();
            string kundenKuerzel = Data.GetKundenKuerzel(as_kunde);

            List<string> l_kundenFreigaben = new List<string>();
            string as_freigabeschlüssel = Path.Combine(Data.KundenFreigabe + "\\freigabeschluessel_k_" + kundenKuerzel.ToLower() + ".txt");

            if (File.Exists(as_freigabeschlüssel))
            {
                string line;
                System.IO.StreamReader alter = new System.IO.StreamReader(as_freigabeschlüssel, Encoding.Default);
                for (int i = 0; (line = alter.ReadLine()) != null; i++)
                {
                    foreach (var freigabe in las_relevanteFreigaben)
                    {
                        if (line.StartsWith(freigabe + "	"))
                        {
                            l_kundenFreigaben.Add(freigabe);
                        }
                    }
                }
            }
            string[] las_kundenFreigaben = l_kundenFreigaben.ToArray();
            List<string> l_kundenOrdner = new List<string>();

            foreach (var f in las_kundenFreigaben)
            {
                string[] ordner = Data.GetWebserviceFreigabenPfade(f);
                foreach (var o in ordner)
                {
                    l_kundenOrdner.Add(o);
                }
            }
            string[] las_kundenOrdner = l_kundenOrdner.ToArray();

            List<string> todoDatei = new List<string>()
            {
                "--------------------------------------------------",
                "set echo on",
                "set serveroutput on",
                "--------------------------------------------------",
                "DEFINE VERSION = '" + as_versionBis + "'",
            };
            if (as_auslArt == "2")
            {
                todoDatei.Add("DEFINE FIXS = '" + as_FixVon.PadLeft(2, '0') + "'");
                todoDatei.Add("DEFINE FIXE = '" + as_FixBis.PadLeft(2, '0') + "'");
            }
            todoDatei.Add("--------------------------------------------------");
            todoDatei.Add("@user_info.sql");
            if (as_auslArt == "2")
            {
                todoDatei.Add("");
                todoDatei.Add("exec ef_user_session.setsession('ef');");
                todoDatei.Add("");

            }
            todoDatei.Add("--------------------------------------------------");
            if (as_auslArt == "1")
            {
                todoDatei.Add("--Migration from the older Version to the new main version");
                todoDatei.Add("@migration_" + as_versionVon + "_" + as_versionBis + ".sql");
            }
            else
            {
                todoDatei.Add("");
            }
            todoDatei.Add("--------------------------------------------------");
            todoDatei.Add("-- Version check will be executed and sql+");
            todoDatei.Add("-- terminated in case of version conflicts");
            todoDatei.Add("--------------------------------------------------");
            todoDatei.Add("WHENEVER SQLERROR EXIT");
            todoDatei.Add("WHENEVER OSERROR EXIT");
            todoDatei.Add("");
            if (as_auslArt == "1")
            {
                todoDatei.Add("@check_db_version.sql '&VERSION, &VERSION'");
            }
            else if (as_auslArt == "2")
            {
                todoDatei.Add("@check_db_version.sql '&VERSION..&FIXS, &VERSION..&FIXE'");
            }
            todoDatei.Add("");
            todoDatei.Add("WHENEVER SQLERROR CONTINUE");
            todoDatei.Add("WHENEVER OSERROR CONTINUE");
            todoDatei.Add("--------------------------------------------------");
            todoDatei.Add("");
            todoDatei.Add("--------------------------------------------------");
            todoDatei.Add("-- Fix level and the respective libraries");
            todoDatei.Add("-- will be registered");
            todoDatei.Add("--------------------------------------------------");
            todoDatei.Add("");

            //hierdynamischer Teil
            //
            if (hasFix)
            {
                while (zaehler <= highest_fix)
                {
                    todoDatei.Add("");
                    todoDatei.Add("DEFINE FIX = '" + zaehler.ToString("00") + "'");
                    todoDatei.Add("");
                    todoDatei.Add("exec add_fix_inst('&VERSION', '&FIX')");
                    //pbd-namen
                    fixpath = as_fixpath + "//Fix_" + zaehler.ToString("00");
                    if(as_internAuslieferung != null)
                    {
                        if (Directory.Exists(fixpath))
                        {

                        }
                        else
                        {
                            fixpath = as_fixpath + "//Fix_" + zaehler.ToString("00") + "_LOCK";
                        }
                    }
                    
                    var blacklist = Data.GetFixBlacklist();
                    var dirfix = new DirectoryInfo(fixpath);
                    

                    //var dirfix = new DirectoryInfo(fixpath);
                    //var files = dirfix.EnumerateFiles().Where(x => x.Equals(blacklist));


                    foreach (var file in dirfix.EnumerateFiles("*.pbd"))
                    {
                        // prüfen auf fremde efcudateien
                        filecheck = file.ToString();
                        if (!(Array.IndexOf(blacklist, filecheck) > -1))
                        {
                            if (Regex.IsMatch(filecheck, efcu_pattern))
                            {
                                int zaehler_pbds = 0;
                                while (zaehler_pbds < las_kunde_pbd.Length)
                                {
                                    kundepbd_pattern = @".*efcu" + las_kunde_pbd[zaehler_pbds].ToLower() +".pbd" + ".*$";
                                    if (Regex.IsMatch(filecheck, kundepbd_pattern))
                                    {
                                        todoDatei.Add("exec add_fix_inst_lib('&VERSION', '&FIX', '" + Path.GetFileNameWithoutExtension(file.ToString()) + "')");
                                    }
                                    zaehler_pbds++;
                                }
                            }
                            else
                            {
                                todoDatei.Add("exec add_fix_inst_lib('&VERSION', '&FIX', '" + Path.GetFileNameWithoutExtension(file.ToString()) + "')");
                            }
                        }
                    }
                    //webservice 
                    string[] las_toprint = WebservicePruefung(webpath, as_versionBis, zaehler, las_ordner, las_eintrag);
                    if (las_toprint.Length > 0 && webRelevant)
                    {
                        int zaehler_2 = 0;
                        todoDatei.Add("");
                        todoDatei.Add("-- Webservice");
                        while (zaehler_2 < las_toprint.Length)
                        {
                            todoDatei.Add("exec add_fix_inst_lib('&VERSION', '&FIX', '" + las_toprint[zaehler_2] + "')");
                            zaehler_2++;
                        }
                    }

                    //webservice_ef3
                    string[] las_toprint_ef3 = WebservicePruefung_ef3(webpath_ef3, as_versionBis, zaehler, las_kundenOrdner, las_eintrag_ef3);
                    if (las_toprint_ef3.Length > 0)
                    {
                        int zaehler_3 = 0;
                        todoDatei.Add("");
                        todoDatei.Add("-- Webservice_ef3");
                        while (zaehler_3 < las_toprint_ef3.Length)
                        {
                            todoDatei.Add("exec add_fix_inst_lib('&VERSION', '&FIX', '" + las_toprint_ef3[zaehler_3] + "')");
                            zaehler_3++;
                        }
                    }
                    //webservice_ef3_std
                    string[] las_toprint_ef3_std = WebservicePruefung_ef3_std(webpath_ef3, as_versionBis, zaehler, las_ordner_ef3_std, las_eintarg_ef3_std);
                    if (las_toprint_ef3_std.Length > 0)
                    {
                        int zaehler_4 = 0;
                        todoDatei.Add("");
                        todoDatei.Add("-- Webservice_ef3_std");
                        while (zaehler_4 < las_toprint_ef3_std.Length)
                        {
                            todoDatei.Add("exec add_fix_inst_lib('&VERSION', '&FIX', '" + las_toprint_ef3_std[zaehler_4] + "')");
                            zaehler_4++;
                        }
                    }
                    zaehler++;
                }
                todoDatei.Add("");
                todoDatei.Add("COMMIT");
                todoDatei.Add("/");
            }
            todoDatei.Add("");
            todoDatei.Add("-- < Library name > will be entered without suffix and");
            todoDatei.Add("-- in lower cases(i.e.efbasuo, efdeb)");
            todoDatei.Add("--------------------------------------------------");
            todoDatei.Add("--------------------------------------------------");
            todoDatei.Add("--Migration is divided in standard,");
            todoDatei.Add("--feature activation, modules, customers and language");
            todoDatei.Add("--------------------------------------------------");
            todoDatei.Add("");

            //SKRIPTE AUFRUFUFEN
            var dir = new DirectoryInfo(targetDir);
            List<string> migs = new List<string> { };
            string filetocheck;

            //TODO REGEX anpassen
            string regexpattern = String.Format(@"^.*migration.*$");
            foreach (var file in dir.EnumerateFiles())
            {
                filetocheck = file.ToString();
                if (Regex.IsMatch(filetocheck, regexpattern, RegexOptions.IgnoreCase))
                {
                    // BT-1
                    var matchingitem = todoDatei.FirstOrDefault(x => x.Contains(filetocheck));
                    if (matchingitem == null)
                    {
                        migs.Add(file.ToString());
                    }
                }
            }
            
            //migs.Sort();
            string[] las_migs = migs.ToArray<string>();
            Array.Sort(las_migs, new NaturalSortComparer());
            foreach (string file in las_migs)
            {
                todoDatei.Add("@" + file);
            }

            todoDatei.Add("");
            todoDatei.Add("--------------------------------------------------");

            // DATEI ERZEUGEN
            string[] textToPrint = todoDatei.ToArray<string>();
            System.IO.File.WriteAllLines(targetDir + "\\" + dateiName, textToPrint, Encoding.Default);
            return;
        }
        private static string[] WebservicePruefung(string targetDir, string as_versionBis, int fixNR, string[] las_ordner, string[] las_eintrag)
        {
            //Prüft EINE Fixnummer auf das vorhandensein von relevanten Webservicedateien
            //Gibt alle aufzurufenden Dateien in einem Array zurück
            int zaehler = 0;
            List<string> ret_list = new List<string>();
            while (zaehler < las_ordner.Length)
            {
                string as_webpath = targetDir + las_ordner[zaehler] + "\\" + as_versionBis + "\\" + as_versionBis + "." + fixNR.ToString("00");
                if (Directory.Exists(as_webpath)) { ret_list.Add(las_eintrag[zaehler]); }
                zaehler++;
            }
            string[] ret_val = ret_list.ToArray();
            return ret_val;
        }
        private static string[] WebservicePruefung_ef3(string targetDir, string as_versionBis, int fixNR, string[] las_ordner_ef3, string[] las_eintrag_ef3)
        {
            List<string> ret_list_ef3 = new List<string>();
            foreach (var o in las_ordner_ef3)
            {
                string as_webpath_ef3 = targetDir + "\\" + o + "\\" + as_versionBis + "\\" + as_versionBis + "." + fixNR.ToString("00");
                if (Directory.Exists(as_webpath_ef3))
                {

                    foreach (var eintrag in las_eintrag_ef3)
                    {
                        string filetocheck = as_webpath_ef3 + "\\" + eintrag + ".msi";
                        if (File.Exists(filetocheck))
                        {
                            ret_list_ef3.Add(eintrag);
                        }

                    }
                }
            }
            string[] ret_val = ret_list_ef3.ToArray();
            return ret_val;
        }
        private static string[] WebservicePruefung_ef3_std(string targetDir, string as_versionBis, int fixNR, string[] las_ordner_ef3_std, string[] las_eintarg_ef3_std)
        {
            List<string> ret_list_ef3 = new List<string>();
            foreach (var o in las_ordner_ef3_std)
            {
                string as_webpath_ef3 = targetDir + "\\" + o + "\\" + as_versionBis + "\\" + as_versionBis + "." + fixNR.ToString("00");
                if (Directory.Exists(as_webpath_ef3))
                {

                    foreach (var eintrag in las_eintarg_ef3_std)
                    {
                        string filetocheck = as_webpath_ef3 + "\\" + eintrag + ".msi";
                        if (File.Exists(filetocheck))
                        {
                            ret_list_ef3.Add(eintrag);
                        }

                    }
                }
            }
            string[] ret_val = ret_list_ef3.ToArray();
            return ret_val;
        }
        private static void CreateAuslieferungTodo(string as_targetDir, string as_versionBis, string as_versionVon, string as_highestFix, bool webRelevant, string todoFlag, string installationFlag, bool hasFix, string as_auslieferung, bool webRelevantStd, string as_woechentlichenBuild, bool clientEmpty)
        {
            string dateiName = "todo.txt";
            string[] textToPrint;
            string versionName;

            if (hasFix)
            {
                versionName = as_versionBis + "." + as_highestFix;
            }
            else
            {
                versionName = as_versionBis;
            }

            List<string> todoDatei = new List<string>()
            {
                "Instruction for update to version " + versionName,
                "",
                "Version " + as_versionVon + " is necessary!!",
                "",
                "Attention !!! SQLPLUS is closed, if the version of the database is not valid",
                "",
                "Please follow the instruction in",
                "Check Report Translation PDF's",
                "Necessity Consulting PDF's",
                ""
            };
            if (as_auslieferung == "1")
            {
                if (as_woechentlichenBuild != null)
                {

                }
                else
                {
                    todoDatei.Add("PDF-Druck:");
                    todoDatei.Add("Please check / install Ghostscript and HP Universal Print Driver.");
                    todoDatei.Add("");
                }

            }
            if (webRelevant || webRelevantStd)
            {
                todoDatei.Add("ef3Webservice:");
                todoDatei.Add("Please follow the instruction in");
                todoDatei.Add("Installation IIS _Webservice_.pdf");
                todoDatei.Add("ReadMe.txt");
                todoDatei.Add("");
            }
            if (!clientEmpty) {
                todoDatei.Add("Clients:");
                if (installationFlag != null)
                {
                    todoDatei.Add("execute setup.exe");
                }
                else
                {
                    todoDatei.Add("replace files in source folder");
                }
            }
            if (clientEmpty && installationFlag != null)
            {
                todoDatei.Add("Clients:");
                todoDatei.Add("execute setup.exe");
            }
            todoDatei.Add("");
            todoDatei.Add("Server:");
            if (webRelevant || webRelevantStd)
            {
                todoDatei.Add("Before updating the database, the IIS server should also be shut down (stopped).");
            }

            if (todoFlag != null)
            {
                todoDatei.Add("execute migration.cmd");
                todoDatei.Add("");
                todoDatei.Add("sign on with ef3 Schema(ef3-oracle user)");
            }
            else
            {
                todoDatei.Add("Open Run command line(Start -> run...)");
                todoDatei.Add("Type cmd in input field, its open command prompt window.");
                todoDatei.Add("Type following command");
                todoDatei.Add("");
                todoDatei.Add("set nls_lang=AMERICAN_AMERICA.WE8MSWIN1252");
                todoDatei.Add("sqlplus");
                todoDatei.Add("");
                todoDatei.Add("sign on with ef3 Schema(ef3-oracle user)");
                todoDatei.Add("");
                todoDatei.Add("open spoolfile");
                todoDatei.Add("");
                todoDatei.Add("Execute the following sql-scripts in sql +");
                todoDatei.Add("");
                todoDatei.Add("@todo_" + versionName + ".sql");
                todoDatei.Add("");
                todoDatei.Add("Close Spool-file");
            }
            if (webRelevant || webRelevantStd)
            {
                todoDatei.Add("After updating the database, the IIS server should be restarted.");
            }
            todoDatei.Add("");
            todoDatei.Add("Now the database is in version " + versionName);
            todoDatei.Add("");
            todoDatei.Add("Send the generated spool file to support@efcom.de");
            if (as_auslieferung == "1")
            {
                todoDatei.Add("");
                todoDatei.Add("");
                todoDatei.Add("");
                todoDatei.Add("If the update was succesfully you may do an analysis of your database");
                todoDatei.Add("");
                todoDatei.Add("For analysis of the database the following variables have to be defined");
                todoDatei.Add("");
                todoDatei.Add("DEFINE METHOD = '<method>' (COMPUTE, COMPUTE_SUBSET, NONE)");
                todoDatei.Add("");
                todoDatei.Add("For example:");
                todoDatei.Add("# DEFINE METHOD = 'COMPUTE' (for exact calculation of statistics)");
                todoDatei.Add("");
                todoDatei.Add("-- METHOD uses the following parameters:");
                todoDatei.Add("1. COMPUTE        -- analysis data will be created for all tables which are not flagged.");
                todoDatei.Add("2. COMPUTE_SUBSET –- analysis data will be created for all tables which are flagged.");
                todoDatei.Add("3. NONE           -- no analysis data will be created - DEFAULT");
                todoDatei.Add("Information about COMPUTE and COMPUTE_SUBSET");
                todoDatei.Add("--------------------------------------------");
                todoDatei.Add("Sometimes there is not enough time to analyse ALL tables for example during a migration. Therefore the administrator can set a flag in the key data of the table maintenance dialog to decide whether a table shell be analysed or not.");
                todoDatei.Add("With this feature the administrator can analyse single tables or build packages which will be processed at a preferred time.");
                todoDatei.Add("");
                todoDatei.Add("");
                todoDatei.Add("Execute the following command:");
                todoDatei.Add("exec ef_utilities.analyze_schema('&METHOD')");
                todoDatei.Add("");
            }
            else
            {
                todoDatei.Add("");
            }

            textToPrint = todoDatei.ToArray<string>();
            System.IO.File.WriteAllLines(as_targetDir + "\\" + dateiName, textToPrint, Encoding.Default);
            return;
        }
        private static void PruefeFreigabeSchluessel(string as_targetDir, string as_versionsPrefix, string as_kundenKuerzel)
        {
            string as_filetocheck = Path.Combine(Data.KundenFreigabe + "\\freigabeschluessel_k_" + as_kundenKuerzel.ToLower() + ".txt");
            if (File.Exists(as_filetocheck)) { }
            else
            {
                throw new Exception("Freigabeschlüssel Datei konnte nicht gefunden werden: " + as_filetocheck);
            }
            //string as_filetocheck = Path.Combine(Data.GrossMigration3 + Data.GrossMigration1 + as_versionsPrefix + "\\Freigabe\\freigabeschluessel_k_" + as_kundenKuerzel.ToLower() + ".txt");
            //if (File.Exists(as_filetocheck)){}
            //else
            //{
            //    as_filetocheck = Path.Combine(Data.GrossMigration4 + Data.GrossMigration1 + as_versionsPrefix + "\\Freigabe\\freigabeschluessel_k_" + as_kundenKuerzel.ToLower() + ".txt");
            //    if (File.Exists(as_filetocheck)){}
            //    else
            //    {
            //        throw new Exception("Freigabeschlüssel Datei konnte nicht gefunden werden: " + as_filetocheck);
            //    }
            //}
        }
        private static void CopyFreigabeSchluessel(string as_targetDir, string as_versionsPrefix, string as_kundenKuerzel)
        {
            //alte variante
            //string as_filetocopy = Path.Combine(Data.GrossMigration3 + Data.GrossMigration1 + as_versionsPrefix + "\\Freigabe\\freigabeschluessel_k_" + as_kundenKuerzel.ToLower() + ".txt");
            //string as_filePath = as_targetDir + "\\freigabeschluessel_k_" + as_kundenKuerzel.ToLower() + ".txt";
            //if (File.Exists(as_filetocopy))
            //{
            //    File.Copy(as_filetocopy, as_filePath, false);
            //}
            //else
            //{
            //    as_filetocopy = Path.Combine(Data.GrossMigration4 + Data.GrossMigration1 + as_versionsPrefix + "\\Freigabe\\freigabeschluessel_k_" + as_kundenKuerzel.ToLower() + ".txt");
            //    if (File.Exists(as_filetocopy))
            //    {
            //        File.Copy(as_filetocopy, as_filePath, false);
            //    }
            //    else
            //    {
            //        throw new Exception("Freigabeschlüssel Datei konnte nicht gefunden werden: " + as_filetocopy);
            //    }
            string as_filetocopy = Path.Combine(Data.KundenFreigabe + "\\freigabeschluessel_k_" + as_kundenKuerzel.ToLower() + ".txt");
            string as_filePath = as_targetDir + "\\freigabeschluessel_k_" + as_kundenKuerzel.ToLower() + ".txt";
            File.Copy(as_filetocopy, as_filePath, false);
        }
        private static void CopyPdfDruck(string as_targetDir, string as_versionPrefix)
        {
            string as_pdfDruck = as_targetDir + "\\PDF-Druck", 
            as_pdfDir = Data.PdfDruck + "\\" + as_versionPrefix;
            Directory.CreateDirectory(as_pdfDruck);
            if (Directory.Exists(as_pdfDir))
            {
                CopyFiles(as_pdfDir, as_pdfDruck);
            }
            else
            {
                throw new Exception("PDF-Druck Ordner für das Release " + as_versionPrefix + " konnte nicht gefunden werden.");
            }
            
        }
        private static void CopySpezifischenMsi(string as_targetSpezifMsi, string as_targetDir)
        {
            if (Directory.Exists(as_targetSpezifMsi))
            {
                var dir_fix_ef3_to_copy = new DirectoryInfo(as_targetSpezifMsi);
                foreach (var file_msi_ef3 in dir_fix_ef3_to_copy.EnumerateFiles("*.msi"))
                {
                    string as_filePath = as_targetDir + "\\" + file_msi_ef3.Name;
                    file_msi_ef3.CopyTo(as_filePath, false);
                }
            }
        }
        private static void IrrlevantSkriptenEntfernen(string serverpfad, string as_versionBis, string as_FixBis)
        {
            bool check = false;
            string line;
            string line2;
            string todoSql= "todo_" + as_versionBis + "." + as_FixBis + ".sql";
            string ziel = serverpfad + "\\" + todoSql;
            System.IO.StreamReader sr = new System.IO.StreamReader(ziel, Encoding.Default);
            try
            {
                while((line= sr.ReadLine()) != null)
                {
                    if(line.Contains("@Migration"))
                    {
                        string[] treffer = line.Split('@');
                        string migSkript = treffer[1];
                        string ziel2 = serverpfad + "\\" + migSkript;
                        System.IO.StreamReader sr2 = new System.IO.StreamReader(ziel2, Encoding.Default);
                        line2 = sr2.ReadToEnd();
                        if (line2.Contains("@init_application.sql"))
                        {
                            check = true;
                        }
                        else
                        {

                        }
                        sr2.Close();
                    }
                    
                }
                
                sr.Close();
                if (!check)
                {
                    // hier muss die (init_application.sql un die Update Skripten löschen (Falls sie vorhanden sind ))
                    DirectoryInfo migrationSkripten = new DirectoryInfo(serverpfad);
                    foreach (var server in migrationSkripten.GetFiles())
                    {
                        string irrelavant = server.ToString();
                        if(irrelavant.Contains("init_application") || irrelavant.Contains("update"))
                        {
                            if (File.Exists(Path.Combine(serverpfad + "\\" + irrelavant)))
                            {
                                File.Delete(Path.Combine(serverpfad + "\\" + irrelavant));
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

        }
        private static void MigrationCmdErzeugen(string as_targetDir)
        {
            string dateiName = "migration.cmd";
            string[] textToPrint;
            List<string> todoDatei = new List<string>()
            {
                "@ECHO OFF",
                "",
                "IF NOT EXIST \"Server\" GOTO :FalscherAufrufOrt",
                "",
                "CD Server",
                "",
                "set logfile=install.log",
                "IF EXIST %logfile% GOTO :overwrite",
                "GOTO :install",
                "",
                ":overwrite",
                "",
                "SET /p aw=Logfile exists already (Overwrite - y=yes) ?",
                "",
                "IF %aw%==y GOTO :overwrite",
                "ECHO ---Start--- >> %logfile%",
                "GOTO :install",
                ":overwrite",
                "ECHO ---Start--- > %logfile%",
                ":install",
                "",
                "set nls_lang=AMERICAN_AMERICA.WE8MSWIN1252",
                "ECHO nls_lang=AMERICAN_AMERICA.WE8MSWIN1252 >> %logfile%",
                "",
                ":VorbereitungMigration",
                "",
                "CLS",
                "",
                "ECHO --------------------------",
                "ECHO -------------------------- >> %logfile%",
                "ECHO Preparation migration...",
                "ECHO Preparation migration... >> %logfile%",
                "ECHO --------------------------",
                "ECHO -------------------------- >> %logfile%",
                "ECHO Please enter the database login information to login via sqlplus",
                "ECHO Please enter the database login information to login via sqlplus >> %logfile%",
                "SET /p us=ORA_User:",
                "ECHO User:%us% >> %logfile%",
                "SET /p pw=Password:",
                "ECHO Password:****** >> %logfile%",
                "SET /p db=Instance-Databasename:",
                "ECHO Instance-Databasename:%db% >> %logfile%",
                "CLS",
                "",
                "ECHO --------------------------",
                "ECHO -------------------------- >> %logfile%",
                "ECHO Check migration data...",
                "ECHO Check migration data... >> %logfile%",
                "ECHO --------------------------",
                "ECHO -------------------------- >> %logfile%",
                "SET aw=n",
                "ECHO The migration was performed on the following database:",
                "ECHO.",
                "ECHO %us%@%db%",
                "ECHO.",
                "ECHO.",
                "ECHO The migration was performed on the following database:%us%@%db% >> %logfile%",
                "SET /p aw=Do you really want to perform the migration (y=yes / n=no):",
                "ECHO Do you really want to perform the migration (y=yes / n=no):%aw% >> %logfile%",
                "IF %aw%==y GOTO :Start",
                "GOTO :Ende",
                ":Start",
                "ECHO sqlplus %us%/%pw%@%db% @migration.sql >> %logfile%",
                "sqlplus %us%/%pw%@%db% @migration.sql",
                "CLS",
                "",
                "ECHO --------------------------",
                "ECHO -------------------------- >> %logfile%",
                "ECHO Migration performed...",
                "ECHO Migration performed... >> %logfile%",
                "ECHO --------------------------",
                "ECHO -------------------------- >> %logfile%",
                "pause",
                "Goto :Ende",
                ":FalscherAufrufOrt",
                "CLS",
                "ECHO --------------------------",
                "ECHO -------------------------- >> %logfile%",
                "ECHO Fehler...",
                "ECHO Fehler... >> %logfile%",
                "ECHO --------------------------",
                "ECHO -------------------------- >> %logfile%",
                "ECHO File was started out of a wrong folder...",
                "ECHO File was started out of a wrong folder... >> %logfile%",
                "ECHO.",
                "ECHO No server folder was found...",
                "ECHO No server folder was found... >> %logfile%",
                "Pause",
                ":Ende",
                "CLS",
                "ECHO ---End--- > %logfile%",
                "ECHO. > %logfile%",
                "ECHO. > %logfile%",
                ""
            };
            textToPrint = todoDatei.ToArray<string>();
            System.IO.File.WriteAllLines(as_targetDir + "\\" + dateiName, textToPrint, Encoding.Default);
            return;
        }
        private static void MigrationSqlErzeugen(string as_targetDir, string as_versionBis, string as_highestFix, bool hasFix) {
            string dateiName = "migration.sql";
            string[] textToPrint;
            string versionName;

            if (hasFix) {
                versionName = as_versionBis + "." + as_highestFix + ".sql";
            } else {
                versionName = as_versionBis + ".sql";
            }

            List<string> todoDatei = new List<string>()
            {
                "col file_name new_value sqlplus_filename;",
                "SELECT TO_CHAR(SYSDATE, 'yyyymmddhh24miss')||'_ef3_spool'||'.log' AS file_name FROM dual;",
                "spool &sqlplus_filename",
                "",
                "SELECT TO_CHAR(SYSDATE, 'dd.mm.yyyy hh24:mi:ss') AS Startzeit FROM dual;",
                "",
                "@todo_" + versionName,
                "",
                "SELECT TO_CHAR(SYSDATE, 'dd.mm.yyyy hh24:mi:ss') AS Endzeit FROM dual;",
                "",
                "spool off",
                "exit"
            };
            textToPrint = todoDatei.ToArray<string>();
            System.IO.File.WriteAllLines(as_targetDir + "\\" + dateiName, textToPrint, Encoding.Default);
            return;
        }
        private static void InstTemplateverarbeiten(string as_targetDir, string as_clientDir, string as_versionBis, string as_highestFix, bool hasFix)
        {
            string instRoutinePfad = Data.InstTemplate;
            string instTemplatePfad = instRoutinePfad + "\\Installation_template.iss";
            string[] las_versionClient = as_versionBis.Split('.');
            string as_versionMain = las_versionClient[0] + '.' + las_versionClient[1];
            string as_exePath = Data.Auslieferung1 + as_versionMain + "\\" + as_versionBis + Data.Auslieferung2;
            string as_exeSource = as_exePath + as_versionMain + "\\Source";
            int li_abbruch = 1;
            List<string> installationsdatei = new List<string>() {
                "#define ef3Version \"" + as_versionBis + "\"",
                "",
                //"#define ef3Fix \"" + as_highestFix + "\"",
                //"",
                //"#define logfile \"ef3_install\"",
                //"",
                //"#define app_path \"" + as_clientDir + "\"",
                //""
            };
            if (hasFix)
            {
                installationsdatei.Add("#define ef3Fix \"" + as_highestFix + "\"");
            }
            else {
                installationsdatei.Add("#define ef3Fix \"\"");
            }
            

            installationsdatei.Add("");
            installationsdatei.Add("#define logfile \"ef3_install\"");
            installationsdatei.Add("");
            installationsdatei.Add("#define app_path \"" + as_clientDir + "\"");
            installationsdatei.Add("");
            if (File.Exists(instTemplatePfad))
            {
                string line;
                System.IO.StreamReader alter = new System.IO.StreamReader(instTemplatePfad, Encoding.Default);
                for (int i = 0; (line = alter.ReadLine()) != null; i++)
                {
                    if (i >= 12)
                    {

                        if (line == "Source: \"{#app_path}\\__version_main__\\__exe_variant__\"; DestDir: \"{app}\"; DestName: \"ef.exe\"; Flags: ignoreversion" ||
                            line == "Source: \"{#app_path}\\__version_main__\\Source\\*.pbd\"; DestDir: \"{app}\"; Flags: ignoreversion" ||
                            line == "Source: \"{#app_path}\\__version_main__ \\Source\\*.pbd\"; DestDir: \"{app}\"; Flags: ignoreversion" ||
                            line == "Source: \"{#app_path}\\__version_main__\\Source\\Infomaker\\*.pbl\"; DestDir: \"{app}\"; Flags: ignoreversion" ||
                            line == "Source: \"{#app_path}\\help\\*.*\"; DestDir: \"{app}\"; Flags: ignoreversion" ||
                            line == "__client_entries__" ||
                            line == "__module_entries__" ||
                            line == "__fix_entries__")
                        {
                            //ignorieren
                        }
                        else if (line == "Source: \"{#app_path}\\shared\\*.*\"; DestDir: \"{app}\"; Flags: ignoreversion")
                        {
                            installationsdatei.Add("Source: \"{#app_path}\\*.*\"; DestDir: \"{app}\"; Flags: ignoreversion skipifsourcedoesntexist");
                            //installationsdatei.Add("Source: \"{#app_path}\\*.*\"; DestDir: \"{app}\\*.*\"; Flags: ignoreversion recursesubdirs createallsubdirs");

                            if (hasFix)
                            {
                                while (li_abbruch <= int.Parse(as_highestFix))
                                {
                                    string as_exeSourceFix = as_exeSource + "\\Fix_" + li_abbruch.ToString() + "\\Shared";

                                    if (Directory.Exists(as_exeSourceFix))
                                    {
                                        if (Directory.GetDirectories(as_exeSourceFix).Length > 0)
                                        {
                                            foreach (var dir in Directory.GetDirectories(as_exeSourceFix))
                                            {
                                                string d = Path.GetFileName(dir);
                                                installationsdatei.Add("Source: \"{#app_path}\\" + d + "\\*.*\"; DestDir: \"{app}\\" + d + "\"; Flags: ignoreversion recursesubdirs createallsubdirs");
                                            }
                                        }

                                    }
                                    else
                                    {

                                    }
                                    li_abbruch++;

                                }



                            }
                        }
                        else
                        {
                            installationsdatei.Add(line);
                        }
                    }
                }
                alter.Close();
            }

            string[] textToPrint = installationsdatei.ToArray<string>();
            System.IO.File.WriteAllLines(instRoutinePfad + "\\Installation.iss", textToPrint, Encoding.Default);
            string cmdDir = @"C:\TEMP\Installationsroutine\Installation_compile.cmd";

            try
            {
                string _cmdDir = @"C:\TEMP\Installationsroutine\";
                Process proc = new Process();
                proc.StartInfo.WorkingDirectory = _cmdDir;
                proc.StartInfo.FileName = "Installation_compile.cmd";
                proc.Start();
                proc.WaitForExit();
                proc.Close();
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Fehler: " + ex.Message);
            }

            //nicht in output erzeugen
            //prozess in unterordner verlegen
            //falls 2 auslieferungen gleichzeitig
            //unterordner mit kundenname + version

            System.IO.File.Delete(instRoutinePfad + "\\Installation.iss");

            string as_filetomove = instRoutinePfad + "\\Output\\setup.exe";
            string as_filePath = as_targetDir + "\\setup.exe";
            File.Move(as_filetomove, as_filePath);

            SchreibschutzEntfernen(as_clientDir, true);
            Directory.Delete(as_clientDir, true);
            return;
        }
        private static void FixTemplateverarbeiten(string as_targetDir, string as_clientDir, string as_versionBis, string as_fixVon, string as_fixBis, bool hasFix)
        {
            string instRoutinePfad = Data.InstTemplate;
            string instTemplatePfad = instRoutinePfad + "\\Fix_template.iss";
            string [] las_versionClient = as_versionBis.Split('.');
            string as_versionMain = las_versionClient[0] + '.' + las_versionClient[1];
            string as_exePath = Data.Auslieferung1 + as_versionMain + "\\" + as_versionBis + Data.Auslieferung2;
            string as_exeSource = as_exePath + as_versionMain + "\\Source";
            int li_abbruch = int.Parse(as_fixVon); ;
            List<string> installationsdatei = new List<string>() {
                "#define ef3Version \"" + as_versionBis + "\"",
                "",
                "#define ef3FixFrom \"" + as_fixVon + "\"",
                "#define ef3FixTo \"" + as_fixBis + "\"",
                "",
                "#define logfile \"ef3_fix\"",
                "",
                "#define app_path \"" + as_clientDir + "\"",
                ""
            };
            if (File.Exists(instTemplatePfad))
            {
                string line;
                System.IO.StreamReader alter = new System.IO.StreamReader(instTemplatePfad, Encoding.Default);
                for (int i = 0; (line = alter.ReadLine()) != null; i++)
                {
                    if (i >= 12)
                    {

                        if (line == "Source: \"{#app_path}\\__version_main__\\__exe_variant__\"; DestDir: \"{app}\"; DestName: \"ef.exe\"; Flags: ignoreversion" ||
                            line == "Source: \"{#app_path}\\__version_main__\\Source\\*.pbd\"; DestDir: \"{app}\"; Flags: ignoreversion" ||
                            line == "Source: \"{#app_path}\\__version_main__ \\Source\\*.pbd\"; DestDir: \"{app}\"; Flags: ignoreversion" ||
                            line == "Source: \"{#app_path}\\__version_main__\\Source\\Infomaker\\*.pbl\"; DestDir: \"{app}\"; Flags: ignoreversion" ||
                            line == "Source: \"{#app_path}\\help\\*.*\"; DestDir: \"{app}\"; Flags: ignoreversion" ||
                            line == "__client_entries__" ||
                            line == "__module_entries__" ||
                            line == "__fix_entries__")
                        {
                            //ignorieren
                        }
                        //else if (line == "Source: \"{#app_path}\\shared\\*.*\"; DestDir: \"{app}\"; Flags: ignoreversion")
                        //{
                        //    installationsdatei.Add("Source: \"{#app_path}\\*.*\"; DestDir: \"{app}\"; Flags: ignoreversion");
                        //}
                        else if (line == "[Files]")
                        {
                            installationsdatei.Add(line);
                            installationsdatei.Add("// -------------------------------------------------------------------------------------------");
                            installationsdatei.Add("Source: \"{#app_path}\\*.*\"; DestDir: \"{app}\"; Flags: ignoreversion skipifsourcedoesntexist");

                            if(hasFix)
                            {
                                while(li_abbruch <= int.Parse(as_fixBis))
                                {
                                    string as_exeSourceFix = as_exeSource + "\\Fix_" + li_abbruch.ToString("00") +"\\Shared";
                                    
                                    if(Directory.Exists(as_exeSourceFix))
                                    {
                                        if (Directory.GetDirectories(as_exeSourceFix).Length > 0)
                                        {
                                            foreach (var dir in Directory.GetDirectories(as_exeSourceFix))
                                            {
                                                string d = Path.GetFileName(dir);
                                                installationsdatei.Add("Source: \"{#app_path}\\"+ d +"\\*.*\"; DestDir: \"{app}\\" + d + "\"; Flags: ignoreversion recursesubdirs createallsubdirs");
                                            }
                                        }
                                        
                                    }
                                    else
                                    {

                                    }
                                    li_abbruch++;

                                }

                                

                            }
                            
                            i = i++;
                        }
                        else
                        {
                            installationsdatei.Add(line);
                        }
                    }
                }
                alter.Close();
            }

            string[] textToPrint = installationsdatei.ToArray<string>();
            System.IO.File.WriteAllLines(instRoutinePfad + "\\Fix.iss", textToPrint, Encoding.Default);
            string cmdDir = @"C:\TEMP\Installationsroutine\Fix_compile.cmd";
            //try
            //{
            //    Process proc = new Process();
            //    ProcessStartInfo processStartInfo = new ProcessStartInfo
            //    {
            //        WorkingDirectory = Path.GetDirectoryName(cmdDir),
            //        FileName = Path.GetFileName(cmdDir),
            //        UseShellExecute = false,
            //        RedirectStandardOutput = true
            //    };
            //    proc.StartInfo = processStartInfo;
            //    proc = Process.Start(processStartInfo);
            //    proc.WaitForExit();
            //    proc.Close();

            //}
            //catch (Exception ex)
            //{
            //    throw new ArgumentException("Test: " + ex.Message);
            //}

            try
            {
                string _cmdDir = @"C:\TEMP\Installationsroutine\";
                Process proc = new Process();
                proc.StartInfo.WorkingDirectory = _cmdDir;
                proc.StartInfo.FileName = "Fix_compile.cmd";
                proc.Start();
                proc.WaitForExit();
                proc.Close();
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Fehler: " + ex.Message);
            }

            //nicht in output erzeugen
            //prozess in unterordner verlegen
            //falls 2 auslieferungen gleichzeitig
            //unterordner mit kundenname + version

            System.IO.File.Delete(instRoutinePfad + "\\Fix.iss");

            string as_filetomove = instRoutinePfad + "\\Output\\setup.exe";
            string as_filePath = as_targetDir + "\\setup.exe";
            File.Move(as_filetomove, as_filePath);

            SchreibschutzEntfernen(as_clientDir, true);
            Directory.Delete(as_clientDir, true);
            return;
        }
        private static void VerzeichnisKomprimieren(string as_targetDir, string as_zipDir, string as_dateiName, string as_version, string as_32bit, string auslieferungsart) {
            string startPath = as_targetDir;
            string zipPath;
            int abversion64;
            string [] version = as_version.Split('.');
            abversion64 = int.Parse(version[0] + version[1] + version[2]);

            if(abversion64 >= 42116 && as_32bit ==null && auslieferungsart =="1")
            {
                 zipPath = as_zipDir + "\\" + as_dateiName + "x64.zip";
            }
            else
            {
                 zipPath = as_zipDir + "\\" + as_dateiName + ".zip";
            }
            ZipFile.CreateFromDirectory(startPath, zipPath);
            string as_filetomove = zipPath;
            string as_filePath = as_targetDir + "\\" + as_dateiName + ".zip";
            System.IO.DirectoryInfo di = new DirectoryInfo(startPath);
            SchreibschutzEntfernen(startPath, true);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                SchreibschutzEntfernen(dir.FullName, true);
                dir.Delete(true);
            }
            File.Move(as_filetomove, as_filePath);

            string freigabeName = "FREIGABE.TXT";
            string[] textToPrint;
            List<string> todoDatei = new List<string>() {
                ""
            };
            textToPrint = todoDatei.ToArray<string>();
            System.IO.File.WriteAllLines(as_targetDir + "\\" + freigabeName, textToPrint, Encoding.Default);
        }
        private static bool ContainsLetter(string fileName) {
            foreach (char c in fileName) {
                if (char.IsLetter(c)) 
                { 
                    return true;
                }
            }
            return false;
        }

        private static bool IsDirectoryEmpty(string dirpath) {
            return Directory.GetFiles(dirpath).Length == 0 && Directory.GetDirectories(dirpath).Length == 0;
        }

        class NaturalSortComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return StringLogicalComparer.Compare(x.ToString(), y.ToString());
            }
        }

        public static class StringLogicalComparer
        {
            public static int Compare(string str1, string str2)
            {
                return StringComparer.OrdinalIgnoreCase.Compare(NaturalKey(str1), NaturalKey(str2));
            }

            private static string NaturalKey(string input)
            {
                return Regex.Replace(input, @"\d+", match => match.Value.PadLeft(10, '0'));
            }
        }

    }
}