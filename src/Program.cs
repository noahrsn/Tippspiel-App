using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TippspielApp.Models;

namespace TippspielApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Eine Liste implementiert im Hintergrund IEnumerable<string>
            IEnumerable<string> namen = new List<string> { "Anna", "Ben", "Clara" };

            // Die foreach-Schleife nutzt IEnumerable, um alle Namen auszugeben
            foreach (string name in namen)
            {
                Console.WriteLine(name);
            }
        }
    }
}