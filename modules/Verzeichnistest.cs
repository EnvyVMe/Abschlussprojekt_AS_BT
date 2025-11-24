using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BuildToolService
{
    public class Verzeichnistest
    {
        public static string Testpath()
        {
            string[] as_kunde, as_kunde2;
            string pfad;
            bool a;

            as_kunde = Data.GetClients1();
            pfad = Data.GetClients3();
            a = Data.GetClients2();

            return "Clients = " + (as_kunde.Length).ToString() + pfad + "                  " + a.ToString();
        }
    }
}