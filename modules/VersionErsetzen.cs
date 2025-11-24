using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BuildToolService
{
    public class VersionErsetzen
    {
        public static string EfVersionErsetzen(string as_version, string as_kunde)
        {
            string[] las_Version = as_version.Split('.');
            string preVersion1 = las_Version[0];
            string preVersion2 = las_Version[1];
            string versionprefix = las_Version[0] + "." + las_Version[1];
            #region Eingabe Version prüfen 

            Regex regex = new Regex(@"\A[0-9]\.[0-9]\.[0-9]{3}\z");
            if (!regex.IsMatch(as_version))
            {
                throw new ArgumentException("Version: \"" + as_version + "\" ist ungültig!", "version");
            }

            string versionOrdner = Data.Auslieferung1 + preVersion1 + "." + preVersion2 + "\\" + as_version;

            if (!Directory.Exists(versionOrdner))
            {
                throw new ArgumentException("Version: \"" + as_version + "\" ist nicht Vorhanden!", "version");
            }

            if (as_kunde != "efcom")
            {
                throw new ArgumentException("Version: \"" + as_version + "\" kann nur für Kunde efcom erstezt!", "Kunde");
            }

            #endregion
            #region schreibschutz entfernen & ZielOrdner leeren 
            string kundenOrdner = Data.GetKundenOrdner(as_kunde);
            string zielPfad32 = Data.verteilerPfad + kundenOrdner + "\\TEST(32bit)";
            string zielPfad64 = Data.verteilerPfad + kundenOrdner + "\\Test(64bit)";

            if (Directory.Exists(zielPfad32))
            {
                SchreibschutzEntfernen(zielPfad32);
                DeleteFiles(zielPfad32);
            }
            else
            {
                throw new ArgumentException("Pfad: \"" + zielPfad32 + "\" ist nicht vorhanden!");
            }

            if (Directory.Exists(zielPfad64))
            {
                SchreibschutzEntfernen(zielPfad64);
                DeleteFiles(zielPfad64);
            }
            else
            {
                throw new ArgumentException("Pfad: \"" + zielPfad64 + "\" ist nicht vorhanden!");
            }
            #endregion

            #region Version Verteilen 
            // Shared Ordner
            //32Bit
            string sharedPfad32 = Data.Auslieferung1 + versionprefix + Data.sharedpfad32;
            CopySourceFiles(sharedPfad32, zielPfad32);
            //64Bit
            string sharedPfad64 = Data.Auslieferung1 + versionprefix + Data.sharedpfad64;
            CopySourceFiles(sharedPfad64, zielPfad64);

            //ef.exe
            //32Bit
            string efxPfad32 = Data.Auslieferung1 + preVersion1 + "." + preVersion2 + "\\" + as_version + Data.Auslieferung2 + preVersion1 + "." + preVersion2;
            File.Copy(Path.Combine(efxPfad32, "efr2.exe"), Path.Combine(zielPfad32, "ef.exe"));
            //64Bit
            string efxPfad64 = Data.Auslieferung1 + preVersion1 + "." + preVersion2 + "\\" + as_version + Data.Auslieferung2 + preVersion1 + "." + preVersion2;
            File.Copy(Path.Combine(efxPfad64, "ef64.exe"), Path.Combine(zielPfad64, "ef.exe"));

            //Source
            string sourcePfad = Data.Auslieferung1 + preVersion1 + "." + preVersion2 + "\\" + as_version + Data.Auslieferung2 + preVersion1 + "." + preVersion2 + Data.Auslieferung3;
            //32Bit
            CopySourceFiles(sourcePfad, zielPfad32);
            //64Bit
            CopySourceFiles(sourcePfad, zielPfad64);

            // Infomaker
            string infomakerPfad = Data.Auslieferung1 + preVersion1 + "." + preVersion2 + "\\" + as_version + Data.Auslieferung2 + preVersion1 + "." + preVersion2 + Data.Auslieferung3 + "\\" + Data.GetInfomakerOrnder();
            //32Bit
            CopySourceFiles(infomakerPfad, zielPfad32);
            //64Bit
            CopySourceFiles(infomakerPfad, zielPfad64);

            // Kunden 

            string kundePfad = Data.Auslieferung1 + preVersion1 + "." + preVersion2 + "\\" + as_version + Data.Auslieferung2 + preVersion1 + "." + preVersion2 + Data.Auslieferung3 + "\\" + Data.GetKundeOrnder();
            //32Bit
            CopySourceFiles(kundePfad, zielPfad32);
            //64Bit
            CopySourceFiles(kundePfad, zielPfad64);

            //Module
            string modulePfad = Data.Auslieferung1 + preVersion1 + "." + preVersion2 + "\\" + as_version + Data.Auslieferung2 + preVersion1 + "." + preVersion2 + Data.Auslieferung3 + "\\" + Data.GetModulOrnder();
            //32Bit
            CopySourceFiles(modulePfad, zielPfad32);
            //64Bit
            CopySourceFiles(modulePfad, zielPfad64);

            #endregion

            return "Version: \"" + as_version + "\" wurde erfolgreich ersetzt." + Environment.NewLine +"Ziel1: " + zielPfad32 + Environment.NewLine +"Ziel2: " + zielPfad64;
        }

        public static void DeleteFiles(string target)
        {
            DirectoryInfo ziel = new DirectoryInfo(target);
            foreach (var file in ziel.GetFiles())
            {
                try
                {
                    File.Delete(target + "\\" + file);
                }
                catch (Exception ex)
                {

                    throw new ArgumentException("Fehler: " + ex.Message);
                }

            }
        }

        public static void CopySourceFiles(string sourceDir, string targetDir)
        {
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string filePath = Path.Combine(targetDir, Path.GetFileName(file));
                if (!File.Exists(filePath))
                {
                    File.Copy(file, filePath, false);
                }
            }

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
    }
}