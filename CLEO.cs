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

        static void Main(string[] args) {
            MKL.Version("CLEO (Command Line Editor Oversimplefied) - CLEO.cs","19.03.09");
            MKL.Lic    ("CLEO (Command Line Editor Oversimplefied) - CLEO.cs","GNU General Public License 3");
            Console.WriteLine($"CLEO v{MKL.Newest}");
        }

    }

}

