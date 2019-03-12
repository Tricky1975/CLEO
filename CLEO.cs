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
// Version: 19.03.12
// EndLic


#undef KeyOnExit
#define ColorStrict  // if set the program will crash when an unknown color has been set!
#undef Log
#undef FlagChat

#if DEBUG
#define debugdoc
#endif 


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TrickyUnits;



namespace CLEO
{

    class CLEO
    {
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
        bool insert = true;
        bool modified = false; // must be false in release!
        string[] Doc = new string[0];


        CLEO(string filename, FlagParse aflags)
        {
            FileName = filename;
            flags = aflags;
            if (!File.Exists(FileName)) {
                Console.Write($"File {FileName} does not exist. Create it ? <Y/N> ");
                var x = Console.ReadKey(true);
                if (x.Key == ConsoleKey.Y) {
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
                Doc = QuickStream.LoadLines(FileName, ref win);
                EOLN = "\n";
                if (win) EOLN = "\r\n";
            } catch (Exception e) {
                Crash(e);
            }
        }

        static int lx = 0;
        static int ly = 0;
        static void Locate(int x, int y, bool force = false)
        { // Named after the "LOCATE" command in GWBASIC,although the original command had syntax LOCATE Y,X but as that would only confuse me... :P
            if ((!force) && x == lx && y == ly) return;
            lx = x; ly = y;
            var rx = x;
            var ry = y;
            if (x < 0) rx = Console.WindowWidth + x;
            if (y < 0) ry = Console.WindowHeight + y;
            //LOG($"Locate({x},{y}) => ({rx},{ry})   -- Winsize: {Console.WindowWidth}x{Console.WindowHeight}");
            if (rx>=0 && ry>=0 && rx<Console.WindowWidth && ry<Console.WindowHeight)
                Console.SetCursorPosition(rx, ry);
        }

        static bool caps = false;
        static bool NumL = false;
        static bool sins = false;
        static bool smod = false;
        void UCaps(bool force = false)
        {
            if (force || caps != Console.CapsLock || NumL != Console.NumberLock || sins != insert || smod != modified) {
                caps = Console.CapsLock;
                NumL = Console.NumberLock;
                sins = insert;
                smod = modified;
                QColor("Foot");
                Locate(0, -1);
                if (modified)
                    Console.Write("  *  ");
                else
                    Console.Write("     ");
                switch (caps) {
                    case true: Console.Write("|CAPS|"); break;
                    case false: Console.Write("|    |"); break;
                }
                switch (NumL) {
                    case true: Console.Write("|NUM|"); break;
                    case false: Console.Write("|   |"); break;
                }
                switch (insert) {
                    case true: Console.Write("|INSERT|"); break;
                    case false: Console.Write("|      |"); break;
                }
                Console.Write("  ");
                switch (EOLN) {
                    case "\r\n":
                        Console.Write("Windows");
                        break;
                    case "\n":
                        Console.Write("Unix");
                        break;
                    default:
                        Console.Write("Unknown");
                        break;
                }
            }
        }

        // In C or C++ I would set this INSIDE the void function
        // but C# doesn't support that (I tried), so too bad!
        static int ox = 0;
        static int oy = 0;
        void UpdateCursorPos(bool force = false)
        {
            if (force || curx != ox || cury != oy) {
                var ll = "*"; if (cury < Doc.Length) ll = $"{Doc[cury].Length + 1}";
                var spos = $"    Doc {docn + 1}/{CLEOs.Length} Line {cury + 1}/{Doc.Length} Pos {curx + 1}/{ll} ";
                QColor("Foot");
                Locate(-(spos.Length + 2), -1);
                Console.Write(spos);
                ox = curx;
                oy = cury;
            }
        }

        void CursorLocate() => Locate(curx - scrx, (cury - scry) + 1);

        void DrawLine(int linnum, bool force = false)
        {
            // This routine is SLOW! I hope I can produce something faster in the future but for now this'll have to do!
            if (force || (linnum - scry + 1) < Console.WindowHeight - 1) {
                var w = Console.WindowWidth - 1;
                // Auto left-right scroll
                if (curx < scrx) scrx = scrx - 10;
                if (curx > scrx + w) scrx = curx - 10;
                if (scrx < 0) scrx = 0;
                // draw stuff
                for (int col = scrx; col < scrx + w; col++) {
                    Locate(col - scrx, (linnum - scry) + 1);
                    //LOG($"DL {col}/{linnum}/{scrx}/{scry}/{Doc.Length}");
                    if (linnum < Doc.Length && col < Doc[linnum].Length) { // Remember, once the first is true, the second is no longer checked, so this is safe!
                        var a = Doc[linnum][col];
                        if (a < 30 || a > 126) {
                            QColor("Text", true);
                            Console.Write("?");
                        } else {
                            QColor("Text");
                            Console.Write(a);
                        }
                    } else if (col == 80) {
                        QColor("p80l");
                        Console.Write("|");
                    } else {
                        QColor("Text");
                        Console.Write(" ");
                    }
                }
            }
        }

        void DrawText()
        {
            var h = Console.WindowHeight - 2;
            for (int l = scry; l < scry + h; l++) DrawLine(l);
        }

        void Redraw()
        {
            ww = Console.WindowWidth;
            wh = Console.WindowHeight;

            // Clear screen from all junk still living there
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Black;
            Cls();

            // Top and bottom bar!
            QColor("Head"); for (int i = 0; i < Console.WindowWidth - 1; i++) Console.Write(" "); Locate(0, -1, true);
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
            DrawText();

            // Position Cursor            
            CursorLocate();
            QColor("Text");
        }

        static void FootMessage(string m) {

            QColor("FOOT");

            Locate(0, -1);

            for (int i = 0; i < Console.WindowWidth - 2; ++i) Console.Write(" ");

            Locate(0, -1,true);

            Console.Write(m);

        }

        void Save()
        {
            try {

                var bout = QuickStream.WriteFile(FileName);

                FootMessage($"Saving {FileName}");

                for(int i = 0; i < Doc.Length; i++) {

                    if (i > 0) bout.WriteString(EOLN, true);

                    bout.WriteString(Doc[i],true);

                }

                bout.Close();

                modified = false;

            } catch (Exception e) {

                FootMessage($"ERROR! Saving failed -- {e.Message}");

                Console.Beep();

                Console.ReadKey();

            } finally {

                Redraw();

            }
        }

        static void Quit()
        {
            foreach (var c in CLEOs) {
                Locate(0, -1, true);
                QColor("Quit");
                for (int i = 0; i < Console.WindowWidth - 1; i++) Console.Write(" ");
                Locate(0, -1, true);
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
            Locate(0, -1, true);
            QColor("Quit");
            for (int i = 0; i < Console.WindowWidth - 1; i++) Console.Write(" ");
            Locate(0, -1, true);
            Console.Write("Do you really want to quit CLEO ? <Y/N> ");
            if (Console.ReadKey(true).Key == ConsoleKey.Y) {
                Console.Write("Yes");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Clear();
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
            Console.WriteLine("\t\tF8  = Prev document");
            Console.WriteLine("\t\tF9  = Next document");
            Console.WriteLine("\t\tF10 = Quit");
            Console.WriteLine("\n\tFor copy and pasting, your normal features your OS provides you inside a terminal should work");
            Console.WriteLine("\n\tHit any key to go back to your editing");
            Console.ReadKey();
            CLEOs[docn].Redraw();
        }

        void TypeChar(char ch)
        {
            modified = true;
            if (cury == Doc.Length) {
                Array.Resize<string>(ref Doc, Doc.Length + 1);
                Doc[cury] = "";
            }
            if (curx == Doc[cury].Length) {
                Doc[cury] += ch;
                curx++;
                DrawLine(cury);
            } else if (curx == 0) {

                Doc[cury] = $"{ch}{Doc[cury]}";
                curx++;

                DrawLine(cury);
            } else if (curx > 0) {

                Doc[cury] = $"{qstr.Left(Doc[cury], curx)}{ch}{qstr.Right(Doc[cury],Doc[cury].Length-curx)}";

                curx++;

                DrawLine(cury);
            }
        }

        void BackSpace()
        {
            if (curx == 0) {
                if (cury == 0) return;
                modified = true;
                if (cury == Doc.Length) {
                    cury--;
                    //Array.Resize(ref Doc, Doc.Length - 1);
                    curx = Doc[cury].Length;
                    DrawText();
                    CursorLocate();
                    return;
                }
                var ol = Doc[cury].Length;
                Doc[cury - 1] += Doc[cury];
                for (int i = cury; i < Doc.Length - 2; i++) {
                    Doc[i] = Doc[i + 1];
                }
                Array.Resize(ref Doc, Doc.Length - 1);
                //DrawLine(i);
                DrawText();
                CursorLocate();
                /*
                for (int i = Doc.Length; i < Console.WindowHeight; i++) {
                    DrawLine(i);
                }
                */
                cury--;
                if (cury < Doc.Length) curx = Doc[cury].Length-ol;
                if (curx < 0) curx = 0; // Safety measure. curx<0 should NEVER be possible but if it happens, no crash will happen and CLEO will "fix itself".
                CursorLocate();
                return;
            }
            if (curx == Doc[cury].Length) {
                Doc[cury] = qstr.Left(Doc[cury], Doc[cury].Length - 1);
                DrawLine(cury);
                CursorLocate();
                curx--;
                return;
            }
            Doc[cury] = $"{qstr.Left(Doc[cury], curx - 1)}{qstr.Right(Doc[cury], Doc[cury].Length - (curx))}";
            DrawLine(cury);
            curx--;
            CursorLocate();
        }

        void KeyReturn()
        {
            modified = true;
            if (cury == Doc.Length) {
                Array.Resize(ref Doc, Doc.Length + 1);
                Doc[Doc.Length - 1] = "";
                cury++;
                CursorLocate();
                return;
            }
            if (curx == Doc[cury].Length) {
                Array.Resize(ref Doc, Doc.Length + 1);
                for (int i = Doc.Length - 1; i > cury + 1; i--) Doc[i] = Doc[i - 1];
                Doc[cury + 1] = "";
                cury++;
                curx = 0;
                DrawText();
                CursorLocate();
                return;
            }

            Array.Resize(ref Doc, Doc.Length + 1);

            for (int i = Doc.Length - 1; i > cury + 1; i--) Doc[i] = Doc[i - 1];

            Doc[cury + 1] = qstr.Right(Doc[cury], Doc[cury].Length - curx);

            Doc[cury] = qstr.Left(Doc[cury], curx);

            cury++;

            curx = 0;

            DrawText();
            CursorLocate();

        }

        void LeftArrow()
        {
            if (curx == 0) {
                if (cury == 0) return;
                cury--;
                curx = Doc[cury].Length;
            } else curx--;
            CursorLocate();
        }

        void RightArrow()
        {
            if (cury == Doc.Length) return;
            if (curx == Doc[cury].Length) {
                curx = 0;
                cury++;
            } else curx++;
            CursorLocate();
        }

        void DelLine() {
            if (cury == Doc.Length) return;
            for (int l = cury; l < Doc.Length - 1; l++) Doc[l] = Doc[l + 1];
            Array.Resize(ref Doc, Doc.Length - 1);
            curx = 0;
            DrawText();
            modified = true;
        }


        static int ww;
        static int wh;
        void AutoResize() {
            if (ww != Console.WindowWidth || wh != Console.WindowHeight) Redraw();
            if (cury < scry) {
                scry = cury - 5;
                if (scry < 0) scry = 0;
                Redraw();
            } else if (cury-scry > Console.WindowHeight - 3) {
                scry += cury+5;
                Redraw();
            }
        }

        void Flow()
        {
            AutoResize();
            UCaps();
            UpdateCursorPos();
            CursorLocate();
            if (Console.KeyAvailable) {
                var k = Console.ReadKey(true);
                LOG($"{k.ToString()} => {k.Key} => {k.KeyChar}");
                switch (k.Key) {
                    case ConsoleKey.F1:
                        ShowHelp();
                        break;

                    case ConsoleKey.F2:
                        Save();
                        break;
                    case ConsoleKey.F10:
                        Quit();
                        break;
                    case ConsoleKey.Insert:
                        insert = !insert;
                        break;
                    case ConsoleKey.F6:
                        DelLine();
                        break;
                    case ConsoleKey.F8:
                        docn--;
                        if (docn < 0) docn = CLEOs.Length - 1;
                        CLEOs[docn].Redraw();
                        return;
                    case ConsoleKey.F9:
                        docn++;
                        if (docn >= CLEOs.Length) docn = 0;
                        CLEOs[docn].Redraw();
                        return;
                    case ConsoleKey.Q:
                    case ConsoleKey.W:
                    case ConsoleKey.E:
                    case ConsoleKey.R:
                    case ConsoleKey.T:
                    case ConsoleKey.Y:
                    case ConsoleKey.U:
                    case ConsoleKey.I:
                    case ConsoleKey.O:
                    case ConsoleKey.P:
                    case ConsoleKey.A:
                    case ConsoleKey.S:
                    case ConsoleKey.D:
                    case ConsoleKey.F:
                    case ConsoleKey.G:
                    case ConsoleKey.H:
                    case ConsoleKey.J:
                    case ConsoleKey.K:
                    case ConsoleKey.L:
                    case ConsoleKey.Z:
                    case ConsoleKey.X:
                    case ConsoleKey.C:
                    case ConsoleKey.V:
                    case ConsoleKey.B:
                    case ConsoleKey.N:
                    case ConsoleKey.M:
                    case ConsoleKey.D0:
                    case ConsoleKey.D1:
                    case ConsoleKey.D2:
                    case ConsoleKey.D3:
                    case ConsoleKey.D4:
                    case ConsoleKey.D5:
                    case ConsoleKey.D6:
                    case ConsoleKey.D7:
                    case ConsoleKey.D8:
                    case ConsoleKey.D9:
                    case ConsoleKey.Spacebar:
                    case ConsoleKey.OemComma:
                    case ConsoleKey.OemPeriod:
                    case ConsoleKey.OemMinus:
                    case ConsoleKey.OemPlus:
                    case ConsoleKey.Oem1:
                    case ConsoleKey.Oem2:
                    case ConsoleKey.Oem3:
                    case ConsoleKey.Oem4:
                    case ConsoleKey.Oem5:
                    case ConsoleKey.Oem6:
                    case ConsoleKey.Oem7:
                    case ConsoleKey.Oem8:
                    case ConsoleKey.Separator:
                        TypeChar(k.KeyChar);
                        break;
                    case ConsoleKey.Backspace:
                        BackSpace();
                        break;
                    case ConsoleKey.Enter:
                        KeyReturn();
                        break;
                    case ConsoleKey.Home:
                        curx = 0;
                        break;
                    case ConsoleKey.End:
                        if (cury < Doc.Length) curx = Doc[cury].Length;
                        break;
                    case ConsoleKey.LeftArrow:
                        LeftArrow();
                        break;
                    case ConsoleKey.RightArrow:
                        RightArrow();
                        break;
                    case ConsoleKey.UpArrow:
                        if (cury > 0) cury--;
                        if (curx > Doc[cury].Length) curx = Doc[cury].Length;
                        CursorLocate();
                        break;
                    case ConsoleKey.DownArrow:
                        if (cury < Doc.Length) cury++;
                        if (cury == Doc.Length) curx = 0;
                        else if (curx > Doc[cury].Length) curx = Doc[cury].Length;
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
        static void Cls() {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
        }

        static void Crash(string e)
        {
            Cls();
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("FATAL ERROR!");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(e);
            Environment.Exit(2);
        }
        static void Crash(Exception e) => Crash(e.Message);

        static void Assert(bool cond, string e)
        {
            if (!cond) Crash(e);
        }


        static void ShowVersionInfo()
        {
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

                case "Pink":
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

        static void QColor(string n, bool reverse = false)
        {
            Assert(config.C($"{n}Color") != "", $"No foreground known for {n}");
            Assert(config.C($"{n}Back") != "", $"No background known for {n}");
            if (reverse) {
                Console.BackgroundColor = GQColor(config.C($"{n}Color"));
                Console.ForegroundColor = GQColor(config.C($"{n}Back"));
            } else {
                Console.ForegroundColor = GQColor(config.C($"{n}Color"));
                Console.BackgroundColor = GQColor(config.C($"{n}Back"));
            }
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
            config.D("p80lColor", "DGray");
            config.D("p80lBack", "Black");
            if (File.Exists(ConfigFile)) {
                var confl = QuickStream.LoadLines(ConfigFile);
                if (confl == null) {
                    Crash("Invalid config file");
                }
                config.ParseLines(confl);
            }
        }

#if Log
        static QuickStream btlog = QuickStream.WriteFile(Dirry.C("$AppSupport$/CLEO/CLEO_DEBUG_LOG.TXT"));
#endif
        static void LOG(string l)
        {
#if Log
            btlog.WriteString($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}\t {l}\n", true);
#endif
        }

        static void Main(string[] args)
        {
            LOG("Session Started! Log Created");
#if Log
            Console.WriteLine($"PLEASE NOTE! All progress will be logged this time in {Dirry.C("$AppSupport$/ CLEO / CLEO_DEBUG_LOG.TXT")}");
            Console.Beep();
#endif
#if debugdoc
            var fp = new FlagParse(new string[] { "../../test/test.txt" });
#else
            var fp = new FlagParse(args);
#endif
            LoadConfig();
            fp.CrBool("version", false);
            fp.CrBool("wineoln", false); // When set new files will always have the Windows EOLN otherwise EOLN will be set to the unix way.
            fp.CrBool("configure", false);
            fp.CrInt("tabsize", 8);
#if FlagChat
            Console.WriteLine("Parsing cli input");
            var fpgood = fp.Parse(true);
            Console.WriteLine();
#else
            var fpgood = fp.Parse();
#endif
            MKL.Version("CLEO - CLEO.cs","19.03.12");
            MKL.Lic    ("CLEO - CLEO.cs","GNU General Public License 3");
            Console.WriteLine($"CLEO v{MKL.Newest}");
            Console.WriteLine("Coded by: Jeroen P. Broks");
            Console.WriteLine($"(c) Copyright {MKL.CYear(2019)}, Released under the terms of the General Public License v3\n");
            if (!fpgood) { Console.WriteLine("Invalid cli input!"); return; }
            if (fp.GetBool("version")) ShowVersionInfo();
            if (fp.GetInt("tabsize") < 3 || fp.GetInt("tabsize") > 12) { Console.WriteLine("TabSize must be between 3 and 12"); return; }
            if (fp.GetBool("configure")) {
                if (!File.Exists(ConfigFile)) {
                    Directory.CreateDirectory(qstr.ExtractDir(ConfigFile));
                    config.SaveSource(ConfigFile);
                }
                CLEOs = new CLEO[] { new CLEO(ConfigFile.Replace("\\", "/"), fp) };
            } else {
                if (fp.Args.Length == 0) {
                    Console.WriteLine("Usage: CLEO <file to edit>");
                    Console.WriteLine("\tEdits a file");
                    Console.WriteLine("Usage: CLEO -version");
                    Console.WriteLine("\tDetailed version information");
                    return;
                }
                CLEOs = new CLEO[fp.Args.Length];
                for (int i = 0; i < fp.Args.Length; i++) CLEOs[i] = new CLEO(fp.Args[i], fp);
            }
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

