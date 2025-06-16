using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallE
{
    public class Error
    {
        private static List<string> errores = new List<string>();
        private static bool errorActivo = false;

        public static void SetError(string tipo, string msg)
        {
            if (!errorActivo)
            {
                errorActivo = true;
                errores.Add($"Tipo: {tipo} - Mensaje: {msg}");
            }
        }

        public static void ImprimirErrores()
        {
            if (errores.Count > 0)
            {
                Console.WriteLine("Errores encontrados:");
                foreach (var error in errores)
                {
                    Console.WriteLine(error);
                }
                Console.WriteLine($"Total de errores: {errores.Count}");
            }
            else
            {
                Console.WriteLine("No se encontraron errores.");
            }
            errorActivo = false;
            errores.Clear();
        }
    }
}