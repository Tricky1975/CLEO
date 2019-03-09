// Lic:
// CLEO - Command Line Editor Oversimplefied
// Just a simple editor to replace EDIT.COM
// 
// 
// 
// (c) Jeroen P. Broks, 
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
// Please note that some references to data like pictures or audio, do not automatically
// fall under this licenses. Mostly this is noted in the respective files.
// 
// Version: 19.03.09
// EndLic





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrickyUnits;



namespace CLEO {

    class CLEO{

        static void ShowVersionInfo() {
            Console.WriteLine(MKL.All(true));
        }

        static void Main(string[] args) {
            var fp = new FlagParse(args);
            fp.CrBool("version", false);
            var fpgood = fp.Parse();
            MKL.Version("CLEO - CLEO.cs","19.03.09");
            MKL.Lic    ("CLEO - CLEO.cs","GNU General Public License 3");
            Console.WriteLine($"CLEO v{MKL.Newest}");
            Console.WriteLine("Coded by: Jeroen P. Broks");
            Console.WriteLine($"(c) Copyright {MKL.CYear(2019)}, Released under the terms of the General Public License v3\n");
            if (!fpgood) { Console.WriteLine("Invalid cli input!"); return; }
            if (fp.GetBool("version")) ShowVersionInfo();
            if (fp.Args.Length == 0) {
                Console.WriteLine("Usage: CLEO <file to edit>");
                Console.WriteLine("\tEdits a file");
                Console.WriteLine("Usage: CLEO -version");
                Console.WriteLine("\tDetailed version information");
                return;

            }

        }
    }

}

