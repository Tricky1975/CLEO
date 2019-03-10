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

#undef KeyOnExit
#define ColorStrict  // if set the program will crash when an unknown color has been set!
#undef Log
#undef FlagChat


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TrickyUnits;



namespace CLEO {

    class CLEO{
        static CLEO[] CLEOs;
        readonly string FileName;
        //readonly byte[] EOLN; // How to separate EOLNS (if system cannot detect this the Unix way will be default).
        readonly string EOLN;
        readonly FlagParse flags;
        int TabSize => flags.GetInt("tabsize");
        static int docn = 0;
        int curx = 0;
        int cury = 0;
        int scrx = 0;
        int scry = 0;
        bool modified = true; // must be false in release!
        string[] Doc = new string[0];


        CLEO(string filename,FlagParse aflags) {
            FileName = filename;
            flags = aflags;
            if (!File.Exists(FileName)) {
                Console.Write($"File {FileName} does not exist. Create it ? <Y/N> ");
                var x = Console.ReadKey(true);
                if (x.Key==ConsoleKey.Y) {
                    Console.WriteLine("Yes");
                    QuickStream.SaveString(filename, "");
                    if (flags.GetBool("wineoln")) EOLN = "\r\n"; else EOLN = "\n";
                } else {
                    Console.WriteLine("No");
                    Environment.Exit(1);
                }
            }
            try {
                var win = false;
                var SDoc = QuickStream.LoadLines(FileName,ref win);
                EOLN = "\n";
                if (win) EOLN = "\r\n";
            } catch (Exception e) {
                Crash(e);
            }
        }

        static int lx = 0;
        static int ly = 0;
        static void Locate(int x,int y, bool force=false) { // Named after the "LOCATE" command in GWBASIC,although the original command had syntax LOCATE Y,X but as that would only confuse me... :P
            if ((!force) && x == lx && y == ly) return;
            lx = x; ly = y;
            var rx = x;
            var ry = y;
            if (x < 0) rx = Console.WindowWidth + x;
            if (y < 0) ry = Console.WindowHeight + y;
            LOG($"Locate({x},{y}) => ({rx},{ry})   -- Winsize: {Console.WindowWidth}x{Console.WindowHeight}");
            Console.SetCursorPosition(rx, ry);
        }

        static bool caps = false;
        static bool NumL = false;
        void UCaps(bool force = false) {
            if (force || caps != Console.CapsLock || NumL!=Console.NumberLock) {
                caps = Console.CapsLock;
                NumL = Console.NumberLock;
                QColor("Foot");
                Locate(5, -1);
                switch (caps){
                    case true:  Console.Write("|CAPS|"); break;
                    case false: Console.Write("|    |"); break;
                }
                switch (NumL) {
                    case true:  Console.Write("|NUM|"); break;
                    case false: Console.Write("|   |"); break;
                }
            }
        }

        // In C or C++ I would set this INSIDE the void function
        // but C# doesn't support that (I tried), so too bad!
        static int ox = 0;
        static int oy = 0;
        void UpdateCursorPos(bool force=false) {
            if (force || curx!=ox || cury!=oy) {
                var spos = $"Doc {docn+1}/{CLEOs.Length} Line {cury + 1} Pos {curx + 1} ";
                QColor("Foot");
                Locate(-(spos.Length + 2), -1);
                Console.Write(spos);
                ox = curx;
                oy = cury;
            }
        }

        void CursorLocate() => Locate( curx - scrx,(cury-scry)+1);
        
        void DrawText() {
            
        }

        void Redraw() {
            // Clear screen from all junk still living there
            Cls();

            // Top and bottom bar!
            QColor("Head"); for (int i = 0; i < Console.WindowWidth - 1; i++) Console.Write(" ");  Locate(0, -1,true);
            QColor("Foot"); for (int i = 0; i < Console.WindowWidth - 1; i++) Console.Write(" ");

            // Bar Content
            QColor("Head");
            Locate(0, 0);
            Console.Write(FileName);
            Locate(-11, 0);
            Console.Write("F1 = Help");
            UCaps(true);
            UpdateCursorPos(true);


            // Text Content

            // Position Cursor            
            CursorLocate();
            QColor("Text");
        }

        void Save() {
            // Code comes later!
        }

        static void Quit() {
            foreach (var c in CLEOs) {
                Locate(0, -1,true);
                QColor("Quit");
                for (int i = 0; i < Console.WindowWidth - 1; i++) Console.Write(" ");
                Locate(0, -1,true);
                if (c.modified) {
                    Console.Write($"Save modified file '{c.FileName}' ? <Y/N> ");
                    var waiting = true;
                    while (waiting)
                        switch (Console.ReadKey(true).Key) {
                            case ConsoleKey.Y:
                                Console.Write("Yes");
                                System.Threading.Thread.Sleep(250);
                                c.Save();
                                waiting = false;
                                break;
                            case ConsoleKey.N:
                                Console.Write("No");
                                System.Threading.Thread.Sleep(500);
                                waiting = false;
                                break;
                        }
                }
            }
            Locate(0, -1,true);
            QColor("Quit");
            for (int i = 0; i < Console.WindowWidth - 1; i++) Console.Write(" ");
            Locate(0, -1,true);
            Console.Write("Do you really want to quit CLEO ? <Y/N> ");
            if (Console.ReadKey(true).Key == ConsoleKey.Y) {
                Console.Write("Yes");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
                Cls();
                Environment.Exit(0);
            }
            CLEOs[docn].Redraw(); // If "no" let's continue like nothing happened ;)
        }
        
        static void ShowHelp()
        {
            QColor("Text"); Cls();
            Console.WriteLine("\tCLEO Quick key overview\n");
            Console.WriteLine("\t\tF1  = Show this help");
            Console.WriteLine("\t\tF2  = Save");
            Console.WriteLine("\t\tF6  = Quickly remove a line");
            Console.WriteLine("\t\tF10 = Quit");
            Console.WriteLine("\t\tF11 = Prev document");
            Console.WriteLine("\t\tF12 = Next document");
            Console.WriteLine("\n\tFor copy and pasting, your normal features your OS provides you inside a terminal should work");
            Console.WriteLine("\n\tHit any key to go back to your editing");
            Console.ReadKey();
            CLEOs[docn].Redraw();
        }

        void Flow() {
            UCaps();
            UpdateCursorPos();
            CursorLocate();
            if (Console.KeyAvailable) {
                var k = Console.ReadKey(true);
                switch (k.Key) {
                    case ConsoleKey.F1:
                        ShowHelp();
                        break;
                    case ConsoleKey.F10:
                        Quit();
                        break;
                }
            }
        }






        /*********************************************************
         *********************************************************
         **                                                     **
         **                  START UP STUFF                     **
         **                                                     **
         *********************************************************
         *********************************************************/

        static string ConfigFile => Dirry.C("$AppSupport$/CLEO/Config.gini");
        static TGINI config = new TGINI();
        static void Cls() => Console.Clear();

        static void Crash(string e) {
            Cls();
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("FATAL ERROR!");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(e);
            Environment.Exit(2);
        }
        static void Crash(Exception e) => Crash(e.Message);

        static void Assert(bool cond,string e) {
            if (!cond) Crash(e);
        }
        
        
        static void ShowVersionInfo() {
            Console.WriteLine(MKL.All(true));
        }

        static ConsoleColor GQColor(string n)
        {
            switch (n) {
                case "Black":
                    return ConsoleColor.Black;
                case "DBlue":
                case "DarkBlue":
                    return ConsoleColor.DarkBlue;
                case "DGreen":
                case "DarkGreen":
                    return ConsoleColor.DarkGreen;
                case "DRed":
                case "DarkRed":
                    return ConsoleColor.DarkRed;
                case "DMagenta":
                case "DarkMagenta":
                    return ConsoleColor.DarkMagenta;
                case "DCyan":
                case "DarkCyan":
                    return ConsoleColor.DarkCyan;
                case "DYellow":
                case "DarkYellow":
                case "Brown":
                    return ConsoleColor.DarkYellow;
                case "Gray":
                case "LGray":
                case "LightGray":
                    return ConsoleColor.Gray;
                case "DarkGray":
                case "DGray":
                    return ConsoleColor.DarkGray;
                case "LBlue":
                case "LightBlue":
                case "Blue":
                    return ConsoleColor.Blue;
                case "LGreen":
                case "Green":
                case "LightGreen":
                    return ConsoleColor.Green;
                case "LRed":
                case "Red":
                case "LightRed":
                    return ConsoleColor.Red;
                case "LMagenta":
                case "Magenta":
                case "LightMagenta":
                    return ConsoleColor.Magenta;
                case "LCyan":
                case "Cyan":
                case "LightCyan":
                    return ConsoleColor.Cyan;
                case "Yellow":
                    return ConsoleColor.Yellow;
                case "White":
                    return ConsoleColor.White;
                default:
#if ColorStrict
                    Crash($"I do not know the color {n}");
                    // The return won't happen anymore, but the C# compiler is not smart enough to realize that, 
                    // so it has to be present even when the "Crash" routine does take place!
#endif
                    return ConsoleColor.Black;
            }
        }

        static void QColor(string n) {
            Assert(config.C($"{n}Color") != "", $"No foreground known for {n}");
            Assert(config.C($"{n}Back")  != "", $"No background known for {n}");
            Console.ForegroundColor = GQColor(config.C($"{n}Color"));
            Console.BackgroundColor = GQColor(config.C($"{n}Back"));
        }

        static void LoadConfig()
        {
            config.D("TextColor", "DGreen");
            config.D("TextBack", "Black");
            config.D("HeadColor", "Black");
            config.D("HeadBack", "DGreen");
            config.D("FootColor", "Black");
            config.D("FootBack", "DGreen");
            config.D("QuitColor", "LGreen");
            config.D("QuitBack", "Black");
            if (File.Exists(ConfigFile)) {
                var confl = QuickStream.LoadLines(ConfigFile);
                if (confl==null) {
                    Crash("Invalid config file");
                }
                config.ParseLines(confl);
            }
        }

#if Log
        static QuickStream btlog = QuickStream.WriteFile(Dirry.C("$AppSupport$/CLEO/CLEO_DEBUG_LOG.TXT"));
#endif
        static void LOG(string l) {
#if Log
            btlog.WriteString($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}\t {l}\n", true);
#endif
        }

        static void Main(string[] args) {
            LOG("Session Started! Log Created");
#if Log
            Console.WriteLine($"PLEASE NOTE! All progress will be logged this time in {Dirry.C("$AppSupport$/ CLEO / CLEO_DEBUG_LOG.TXT")}");
            Console.Beep();
#endif
            var fp = new FlagParse(args);
            LoadConfig();
            fp.CrBool("version", false);
            fp.CrBool("wineoln", false); // When set new files will always have the Windows EOLN otherwise EOLN will be set to the unix way.
            fp.CrInt("tabsize", 8);
#if FlagChat
            Console.WriteLine("Parsing cli input");
            var fpgood = fp.Parse(true);
            Console.WriteLine();
#else
            var fpgood = fp.Parse();
#endif
            MKL.Version("CLEO - CLEO.cs","19.03.09");
            MKL.Lic    ("CLEO - CLEO.cs","GNU General Public License 3");
            Console.WriteLine($"CLEO v{MKL.Newest}");
            Console.WriteLine("Coded by: Jeroen P. Broks");
            Console.WriteLine($"(c) Copyright {MKL.CYear(2019)}, Released under the terms of the General Public License v3\n");
            if (!fpgood) { Console.WriteLine("Invalid cli input!"); return; }
            if (fp.GetBool("version")) ShowVersionInfo();
            if (fp.GetInt("tabsize")<3 || fp.GetInt("tabsize")>12) { Console.WriteLine("TabSize must be between 3 and 12"); return; }
            if (fp.Args.Length == 0) {
                Console.WriteLine("Usage: CLEO <file to edit>");
                Console.WriteLine("\tEdits a file");
                Console.WriteLine("Usage: CLEO -version");
                Console.WriteLine("\tDetailed version information");
                return;
            }
            CLEOs = new CLEO[fp.Args.Length];
            for (int i = 0; i < fp.Args.Length; i++) CLEOs[i] = new CLEO(fp.Args[i],fp);
            CLEOs[0].Redraw(); // Only logic to start with the first document, no?
            while (true) CLEOs[docn].Flow();
#if KeyOnExit
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ReadKey();
#endif
        }
    }

}

