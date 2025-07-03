using System;
using System.Collections.Generic;
using System.Linq;

namespace WallE
{
    public class ErrorEntry
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public int Line { get; set; }

        public ErrorEntry(string type, string message, int line)
        {
            Type = type;
            Message = message;
            Line = line;
        }
    }

    public static class Error
    {
        private static List<ErrorEntry> errores = new List<ErrorEntry>();
        private static bool errorActivo = false;

        public static void SetError(string tipo, string msg, int line)
        {
            if (!errorActivo)
            {
                errorActivo = true;
                errores.Add(new ErrorEntry(tipo, msg, line));
            }
        }

        public static List<ErrorEntry> GetErrors()
        {
            return errores.ToList(); 
        }

        public static void ErrorsClear()
        {
            errores.Clear();
            errorActivo = false;
        }

        public static void ImprimirErrores()
        {
            if (errores.Count > 0)
            {
                Console.WriteLine("Errores encontrados:");
                foreach (var error in errores)
                {
                    Console.WriteLine($"[{error.Line}] Tipo: {error.Type} - Mensaje: {error.Message}");
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
