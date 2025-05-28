using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallE
{
    public class Error
    {
        public static bool wrong = false;
        public static string message = "";
        public static string typeMessage = "";

        public static void SetError(string type, string msg)
        {
            if (wrong) return;
            wrong = true;
            message = msg;
            typeMessage = type;
        }
    }
}