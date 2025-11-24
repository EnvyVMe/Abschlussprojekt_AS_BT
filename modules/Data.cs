using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Web;
using System.IO;


namespace BuildToolService
{
    public class Data
    {
        private const string ss_configIni = "config.ini";

        private static string ss_pathMIGRATIONDBIni, ss_pathDateiregelIni, ss_pathEfvorlageIni, ss_instTemp, ss_aktuellsteVersion;
        private static string ss_pathGrossmigration1, ss_pathGrossmigration2, ss_pathGrossmigration3, ss_pathGrossmigration4, ss_pathGrossmigration5;
        private static string ss_pathauslieferung1, ss_pathauslieferung2, ss_pathauslieferung3, ss_pathauslieferungsordner, ss_pathauslieferungsordnerlokal;
        private static string ss_webservice1, ss_webservice2, ss_webservice3, ss_webserviceInstallation, ss_webserviceAktualisierung, ss_kundenKennung, ss_kundenKennung1;
        private static string ss_kundenKennung2, ss_kundenFreigabe, ss_kundenFreigabe1, ss_kundenFreigabe2, ss_pdfDruck, ss_lokalMigDurchfuehren, ss_verteilerPfad;
        private static string ss_sharedpfad64, ss_sharedpfad32, ss_pathauslieferungsordnerIntern;
        public static string GrossMigration1 { get { return ss_pathGrossmigration1; } }
        public static string GrossMigration2 { get { return ss_pathGrossmigration2; } }
        public static string GrossMigration3 { get { return ss_pathGrossmigration3; } }
        public static string GrossMigration4 { get { return ss_pathGrossmigration4; } }
        public static string Auslieferung1 { get { return ss_pathauslieferung1; } }
        public static string Auslieferung2 { get { return ss_pathauslieferung2; } }
        public static string Auslieferung3 { get { return ss_pathauslieferung3; } }
        public static string AuslieferungsOrdner { get { return ss_pathauslieferungsordner; } }
        public static string AuslieferungsOrdnerLokal { get { return ss_pathauslieferungsordnerlokal; } }
        public static string Webservice1 { get { return ss_webservice1; } }
        public static string Webservice2 { get { return ss_webservice2; } }
        public static string Webservice3 { get { return ss_webservice3; } }
        public static string WebserviceInstallation { get { return ss_webserviceInstallation; } }
        public static string WebserviceAktualisierung { get { return ss_webserviceAktualisierung; } }
        public static string VorlagenIni { get { return ss_pathEfvorlageIni; } }
        public static string InstTemplate { get { return ss_instTemp; } }
        public static string KundenKennung { get { return ss_kundenKennung; } }
        public static string KundenFreigabe { get { return ss_kundenFreigabe; } }
        public static string PdfDruck { get { return ss_pdfDruck; } }
        public static string lokalMigDurchfuehren { get { return ss_lokalMigDurchfuehren; } }
        public static string verteilerPfad { get { return ss_verteilerPfad; } }
        public static string sharedpfad64 { get { return ss_sharedpfad64; } }
        public static string sharedpfad32 { get { return ss_sharedpfad32; } }
        public static string AuslieferungsOrdnerIntern { get { return ss_pathauslieferungsordnerIntern; } }
        public static string GrossMigration5 { get { return ss_pathGrossmigration5; } }
        public static void Init()
        {
            StringBuilder sb = new StringBuilder(2048);

            if (GetPrivateProfileString("paths", "aktuellsteVersion", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_aktuellsteVersion = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "migrationDBIniPfad", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_pathMIGRATIONDBIni = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "vorlageEfIniPfad", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_pathEfvorlageIni = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "dateiregelIniPfad", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_pathDateiregelIni = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "grossMigration1", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_pathGrossmigration1 = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "grossMigration2", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_pathGrossmigration2 = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "grossMigration3", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_pathGrossmigration3 = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "grossMigration4", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_pathGrossmigration4 = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "auslieferung1", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_pathauslieferung1 = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "auslieferung2", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_pathauslieferung2 = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "auslieferung3", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_pathauslieferung3 = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "auslieferungsordner", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_pathauslieferungsordner = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "auslieferungsordnerLOKAL", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_pathauslieferungsordnerlokal = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "webservice1", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_webservice1 = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "webservice2", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_webservice2 = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "webservice3", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_webservice3 = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "webserviceAktualisierung", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_webserviceAktualisierung = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "webserviceInstallation", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_webserviceInstallation = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "instTemplatePfad", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_instTemp = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "kundenKennung1", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_kundenKennung1 = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "kundenKennung2", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_kundenKennung2 = sb.ToString();
            }
            ss_kundenKennung = ss_kundenKennung1 + ss_aktuellsteVersion + ss_kundenKennung2;
            if (GetPrivateProfileString("paths", "kundenFreigabe1", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_kundenFreigabe1 = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "kundenFreigabe2", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_kundenFreigabe2 = sb.ToString();
            }
            ss_kundenFreigabe = ss_kundenFreigabe1 + ss_aktuellsteVersion + ss_kundenFreigabe2;
            if (GetPrivateProfileString("paths", "pdfDruck", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_pdfDruck = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "lokalMigDurchfuehren", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_lokalMigDurchfuehren = sb.ToString();
            }
            if (GetPrivateProfileString("paths", "verteilerPfad", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_verteilerPfad = sb.ToString();
            }

            if (GetPrivateProfileString("paths", "sharedpfad64", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_sharedpfad64 = sb.ToString();
            }

            if (GetPrivateProfileString("paths", "sharedpfad32", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_sharedpfad32 = sb.ToString();
            }

            if (GetPrivateProfileString("paths", "auslieferungsordnerIntern", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_pathauslieferungsordnerIntern = sb.ToString();
            }

            if (GetPrivateProfileString("paths", "grossMigration5", "", sb, 2048, System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ss_configIni) > 0)
            {
                ss_pathGrossmigration5 = sb.ToString();
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern UInt32 GetPrivateProfileSection([In][MarshalAs(UnmanagedType.LPStr)] string strSectionName, [In] IntPtr pReturnedString, [In] UInt32 nSize, [In][MarshalAs(UnmanagedType.LPStr)] string strFileName);

        public static string[] GetClients()
        {
            StringBuilder sb = new StringBuilder(2048);

            if (GetPrivateProfileString("global", "Kunde", "", sb, 2048, ss_pathMIGRATIONDBIni) > 0)
            {

                return sb.ToString().ToLower().Split(',');
            }

            return new string[] { };
        }

        public static string[] GetAusnahmen()
        {
            StringBuilder sb = new StringBuilder(2048);


            if (GetPrivateProfileString("ENTFERNDATEI", "ausnahmen", "", sb, 2048, ss_pathDateiregelIni) > 0)
            {
                return sb.ToString().Split(',');
            }

            return new string[] { };
        }

        public static string[] GetEinzeldateien()
        {
            StringBuilder sb = new StringBuilder(2048);


            if (GetPrivateProfileString("ENTFERNDATEI", "einzeldatei", "", sb, 2048, ss_pathDateiregelIni) > 0)
            {
                return sb.ToString().Split(',');
            }

            return new string[] { };
        }

        public static string[] GetDateitypen()
        {
            StringBuilder sb = new StringBuilder(2048);


            if (GetPrivateProfileString("ENTFERNDATEI", "dateityp", "", sb, 2048, ss_pathDateiregelIni) > 0)
            {
                return sb.ToString().Split(',');
            }

            return new string[] { };
        }

        public static string[] GetReihenfolge()
        {
            StringBuilder sb = new StringBuilder(2048);


            if (GetPrivateProfileString("MIGRATIONSDATEI", "reihenfolge", "", sb, 2048, ss_pathDateiregelIni) > 0)
            {
                return sb.ToString().Split(',');
            }

            return new string[] { };
        }

        public static string[] GetZusatzdateien()
        {
            StringBuilder sb = new StringBuilder(2048);


            if (GetPrivateProfileString("MIGRATIONSDATEI", "zusatzdateien", "", sb, 2048, ss_pathDateiregelIni) > 0)
            {
                return sb.ToString().Split(',');
            }

            return new string[] { };
        }

        public static string[] GetAuslieferunglöschen()
        {
            StringBuilder sb = new StringBuilder(2048);


            if (GetPrivateProfileString("AUSLIEFERUNG_ERSTELLEN_LOESCHEN", "delete", "", sb, 2048, ss_pathDateiregelIni) > 0)
            {
                return sb.ToString().Split(',');
            }

            return new string[] { };
        }

        public static string[] GetFixWhitelist()
        {
            StringBuilder sb = new StringBuilder(2048);


            if (GetPrivateProfileString("AUSLIEFERUNG_ERSTELLEN_LOESCHEN", "fixwhitelist", "", sb, 2048, ss_pathDateiregelIni) > 0)
            {
                return sb.ToString().Split(',');
            }

            return new string[] { };
        }

        public static string[] GetFixBlacklist()
        {

            StringBuilder sb = new StringBuilder(2048);

            if (GetPrivateProfileString("AUSLIEFERUNG_ERSTELLEN_LOESCHEN", "fixblacklist", "", sb, 2048, ss_pathDateiregelIni) > 0)
            {
                return sb.ToString().Split(',');
            }

            return new string[] { };
        }


        //Achtung Funktion ist irreführend benannt 
        //Sucht mit Kunde nach dem entsprechenden Kuerzel migdb.ini 
        public static string GetKundenKennung(string s_globalKunde)
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString(s_globalKunde, "kennung", "", sb, 2048, ss_pathMIGRATIONDBIni) > 0)
            {
                return sb.ToString();
            }
            return "";
        }

        public static string GetKundenKuerzel(string s_globalKunde)
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString(s_globalKunde, "kuerzel", "", sb, 2048, ss_pathMIGRATIONDBIni) > 0)
            {
                return sb.ToString().ToUpper();
            }
            return "";
        }

        public static string GetWeitereSkripte(string s_globalKunde)
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString(s_globalKunde, "weitereScripte", "", sb, 2048, ss_pathMIGRATIONDBIni) > 0)
            {
                return sb.ToString();
            }
            return "";
        }

        public static string[] GetKundenPBD(string s_globalKunde)
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString(s_globalKunde, "kunde_pbd", "", sb, 2048, ss_pathMIGRATIONDBIni) > 0)
            {
                return sb.ToString().Split(',');
            }
            return new string[] { };
        }

        #region Webservicepruefung
        public static string[] GetWebserviceOrdner()
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString("WEBSERVICE_PRUEFUNG", "ordner", "", sb, 2048, ss_pathDateiregelIni) > 0)
            {
                return sb.ToString().Split(',');
            }
            return new string[] { };
        }

        public static string[] GetWebserviceEintrag()
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString("WEBSERVICE_PRUEFUNG", "eintrag", "", sb, 2048, ss_pathDateiregelIni) > 0)
            {
                return sb.ToString().Split(',');
            }
            return new string[] { };
        }

        public static string[] GetWebserviceEintrag_ef3()
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString("WEBSERVICE_PRUEFUNG", "eintrag_ef3", "", sb, 2048, ss_pathDateiregelIni) > 0)
            {
                return sb.ToString().Split(',');
            }
            return new string[] { };
        }

        public static string[] GetWebserviceEnglisch()
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString("WEBSERVICE_PRUEFUNG", "dateiname_englisch", "", sb, 2048, ss_pathDateiregelIni) > 0)
            {
                return sb.ToString().Split(',');
            }
            return new string[] { };
        }

        public static string[] GetWebserviceSpanisch()
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString("WEBSERVICE_PRUEFUNG", "dateiname_spanisch", "", sb, 2048, ss_pathDateiregelIni) > 0)
            {
                return sb.ToString().Split(',');
            }
            return new string[] { };
        }

        public static string[] GetWebserviceKundenRelevanteFreigaben()
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString("WEBSERVICES", "relevante_freigaben", "", sb, 2048, ss_pathMIGRATIONDBIni) > 0)
            {
                return sb.ToString().Split(',');
            }
            return new string[] { };
        }

        public static string[] GetWebserviceFreigabenPfade(string s_freigabe)
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString("WEBSERVICES", s_freigabe, "", sb, 2048, ss_pathMIGRATIONDBIni) > 0)
            {
                return sb.ToString().Split(',');
            }
            return new string[] { };
        }
        #endregion

        public static string[] GetMussdateien()
        {
            StringBuilder sb = new StringBuilder(2048);


            if (GetPrivateProfileString("MUSSDATEI", "name", "", sb, 2048, ss_pathDateiregelIni) > 0)
            {
                return sb.ToString().Split(',');
            }

            return new string[] { };
        }

        public static string GetKundenDatabase(string s_globalKunde)
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString(s_globalKunde, "database", "", sb, 2048, ss_pathMIGRATIONDBIni) > 0)
            {
                return sb.ToString().ToUpper();
            }
            return "";
        }
        public static string GetKundenServer(string s_globalKunde)
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString(s_globalKunde, "server", "", sb, 2048, ss_pathMIGRATIONDBIni) > 0)
            {
                return sb.ToString().ToUpper();
            }
            return "";
        }

        public static string GetEntwDatabase()
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString("Testmigration", "database", "", sb, 2048, ss_pathMIGRATIONDBIni) > 0)
            {
                return sb.ToString();
            }
            return "";
        }

        public static string GetEntwServer()
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString("Testmigration", "server", "", sb, 2048, ss_pathMIGRATIONDBIni) > 0)
            {
                return sb.ToString();
            }
            return "";
        }

        public static string GetServerIPAdresse(string server)
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString("Servers", server, "", sb, 2048, ss_pathMIGRATIONDBIni) > 0)
            {
                return sb.ToString();
            }
            return "";
        }

        public static string GetKundenOrdner(string s_globalKunde)
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString(s_globalKunde, "ordner", "", sb, 2048, ss_pathMIGRATIONDBIni) > 0)
            {
                return sb.ToString();
            }
            return "";
        }

        public static string GetSharedOrnder()
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString("Source_Ordner", "sharedOrdner", "", sb, 2048, ss_pathMIGRATIONDBIni) > 0)
            {
                return sb.ToString();
            }
            return "";
        }
        public static string GetInfomakerOrnder()
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString("Source_Ordner", "infomakerOrdner", "", sb, 2048, ss_pathMIGRATIONDBIni) > 0)
            {
                return sb.ToString();
            }
            return "";
        }
        public static string GetKundeOrnder()
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString("Source_Ordner", "kundeOrdner", "", sb, 2048, ss_pathMIGRATIONDBIni) > 0)
            {
                return sb.ToString();
            }
            return "";
        }
        public static string GetModulOrnder()
        {
            StringBuilder sb = new StringBuilder(2048);
            if (GetPrivateProfileString("Source_Ordner", "moduleOrdner", "", sb, 2048, ss_pathMIGRATIONDBIni) > 0)
            {
                return sb.ToString();
            }
            return "";
        }
        public static string[] GetClientTestDatenbank()
        {
            StringBuilder sb = new StringBuilder(2048);

            if (GetPrivateProfileString("global", "KundeTestDatenbank", "", sb, 2048, ss_pathMIGRATIONDBIni) > 0)
            {

                return sb.ToString().ToUpper().Split(',');
            }

            return new string[] { };
        }
        public static string[] GetWebserviceStandart()
        {
            StringBuilder sb = new StringBuilder(2048);

            if (GetPrivateProfileString("WEBSERVICES_EF3_STANDART", "ordner", "", sb, 2048, ss_pathMIGRATIONDBIni) > 0)
            {

                return sb.ToString().ToLower().Split(',');
            }

            return new string[] { };
        }

        public static string[] GetWebserviceStandartEintrag()
        {
            StringBuilder sb = new StringBuilder(2048);

            if (GetPrivateProfileString("WEBSERVICE_PRUEFUNG", "eintrag_ef3_std", "", sb, 2048, ss_pathDateiregelIni) > 0)
            {

                return sb.ToString().ToLower().Split(',');
            }

            return new string[] { };
        }

    }
}
