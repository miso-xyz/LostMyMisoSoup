using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using LMM_Rewritten.Protections;

namespace LMM_Rewritten
{
    class Program
    {
        static public ModuleDefMD asm;
        static public Assembly RefASM;
        static public string path;
        static public Presets Preset = new Presets();
        static public Config conf = new Config();

        static void Main(string[] args)
        {
            //bool forceDefaultPreset = false, forceCustomPreset = false, presetdev = false;
            //if (args.Contains("--forcedefaultpreset")) { forceDefaultPreset = true; }
            //if (args.Contains("--forcecustompreset"))  {  forceCustomPreset = true; }
            //if (args.Contains("--presetdev"))          {          presetdev = true; }
            SetStatusText("Initializing...", ConsoleColor.White, ConsoleColor.Blue);
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("\tLostMyMisoSoup - ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("https://github.com/miso-xyz/LostMyMisoSoup");
            for (int x = 0; x < Console.WindowWidth; x++) { if (Console.CursorLeft == Console.WindowWidth - 1) { break; } Console.Write(" "); }
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();
            InitializeSideContainer(pages.Count() + 1,1);
            bool configFromFile = false;
            try { conf.Read("config.txt"); configFromFile = true; }
            catch {}
            int yPos = Console.CursorTop;
            Console.CursorTop = Console.WindowHeight - 4;
            Console.Write(" - Config Source: ");
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.ForegroundColor = ConsoleColor.White;
            if (configFromFile) { Console.WriteLine(" From File (config.txt) "); }
            else { Console.WriteLine(" App Default "); }
            Console.ResetColor();
            Console.CursorTop = yPos;
            if (!conf.PresetDevMode)
            {
                path = Path.GetFullPath(args[0]);
                RefASM = Assembly.LoadFile(path);
                asm = ModuleDefMD.Load(path);
            }
            if (!conf.ForceDefaultPreset)
            {
                if (File.Exists(conf.PresetFilePath))
                {
                    try { Preset.Read(conf.PresetFilePath); }
                    catch (Exception ex) { if (conf.PresetDevMode) { throw ex; } }
                    yPos = Console.CursorTop;
                    Console.CursorTop = Console.WindowHeight - 5;
                    Console.Write(" - Preset Source: ");
                    Console.BackgroundColor = ConsoleColor.Magenta;
                    Console.ForegroundColor = ConsoleColor.White;
                    if (configFromFile) { Console.WriteLine(" From File (" + conf.PresetFilePath + ") "); } else { Console.WriteLine(" App Default "); }
                    Console.CursorTop = yPos;
                }
                else { if (conf.PresetDevMode) { PrintLog("Presets", "Invalid File Path in Config File, using default preset."); } }
            }
            if (conf.PresetDevMode) { Console.ReadKey(); Environment.Exit(0); }
            Console.ResetColor();
            Console.WriteLine("  While LostMyMisoSoup is deobfuscating");
            Console.WriteLine("  your application, go read the ReadME ");
            Console.Write("  at: ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("https://youtu.be/AjypZ_-vwZ4");
            Console.ResetColor();
            Console.WriteLine();
            for (int x = 0; x < 43; x++) { Console.Write("─"); }
            Console.WriteLine();
            Console.WriteLine("  Credits to:");
            Console.WriteLine("  - Sato-Isolated  (Author of MindLated)");
            Console.WriteLine("  - 0xd4d / wtfsck (Author of dnlib)");
            Console.WriteLine("  - your mother    (fucking gottem dude)");
            for (int x = 0; x < 43; x++) { Console.Write("─"); }
            Console.WriteLine();
            Console.WriteLine("  oni's prolly getting cucked thats funny");
            Console.WriteLine(" cant wait to play mkwii as mayonaise lmao");
            Console.WriteLine();
            Console.WriteLine("  fuck sl, golden age is over cry about it");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("           pizza king forever o7");
            Console.ResetColor();
            for (int x = 0; x < 43; x++) { Console.Write("─"); }
            Console.WriteLine();
            Program.AddPatchesLog("   Name", "      Actions", ConsoleColor.Blue, ConsoleColor.White, ConsoleColor.Blue, ConsoleColor.White);
            if (conf.FixAntiDe4Dot)
            {
                SetStatusText("Searching for AntiDe4Dots...");
                Antis.FixAntiDe4dot();
            }
            if (conf.FixAntiDump)
            {
                SetStatusText("Searching for AntiDump...");
                Antis.FixAntiDump();
            }
            if (conf.FixAntiTamper)
            {
                SetStatusText("Searching for AntiTamper...");
                Antis.FixAntiTamper();
            }
            if (conf.FixAntiDebug)
            {
                SetStatusText("Searching for AntiDebug...");
                Antis.FixAntiDebug();
            }
            if (conf.FixWatermarks)
            {
                SetStatusText("Removing Watermarks...");
                CFlow.RemoveWatermarks();
            }
            if (conf.FixInvalidMD)
            {
                SetStatusText("Removing Invalid Metadata...");
                InvalidMD.Fix();
            }
            if (conf.RemoveUselessJumps)
            {
                SetStatusText("Removing Useless Jumps...");
                CFlow.RemoveUselessBrs();
            }
            if (conf.FixCallis)
            {
                SetStatusText("Convering Callis...");
                CFlow.FixCallis();
            }
            if (conf.RemoveInvalidCalls)
            {
                SetStatusText("Removing Invalid Calls...");
                Antis.RemoveJunkCalls(Program.asm.GlobalType);
            }
            if (conf.FixProxyConst || conf.FixProxyString)
            {
                SetStatusText("Repairing Proxy Calls...");
                Proxy.Fix();
            }
            if (conf.FixStackUnfConf)
            {
                SetStatusText("Removing StackUnfConfusion...");
                IntConfusion.FixStackUnf();
            }
            if (conf.RemoveUselessNOPs)
            {
                SetStatusText("Removing Useless NOPs...");
                CFlow.RemoveUselessNops(); // Taken from DevT02's Junk Remover
            }
            if (conf.FixIntConf)
            {
                SetStatusText("Processing Int Confusion...");
                IntConfusion.Fix();
            }
            if (conf.FixL2F)
            {
                SetStatusText("Repairing Local2Field (v1 & v2)...");
                L2F.FixL2F();
            }
            if (conf.RemoveUnusedLocals)
            {
                SetStatusText("Removing Useless Locals...");
                IntConfusion.RemoveUselessLocals();
            }
            if (conf.RemoveUnusedVariables)
            {
                SetStatusText("Removing Useless Variables...");
                L2F.RemoveUselessFields();
            }
            if (conf.FixArithmatic)
            {
                SetStatusText("Calculating Arithmetic...");
                Arithmatic.Fix();
            }
            if (conf.FixOnlineStringEncr)
            {
                SetStatusText("Decrypting Strings (SOD)...");
                StringEnc.FixSOD();
            }
            if (conf.FixStringsEncr)
            {
                if (RefASM.GetManifestResourceNames().ToList().Contains(Program.Preset.EncString_ResourceName) && RefASM.GetManifestResourceStream(Program.Preset.EncString_ResourceName).Length > 0)
                {
                    SetStatusText("Decrypting Strings...");
                    StringEnc.Fix();
                }
            }
            if (conf.ExportRNGSeeds)
            {
                SetStatusText("Decrypting Strings...", ConsoleColor.White, ConsoleColor.DarkMagenta);
                List<string> text = new List<string>();
                foreach (RngSeed seed in IntConfusion.RngSeeds) { text.Add(seed.method.DeclaringType.Name + "." + seed.method.Name + " - " + seed.inst.Offset + "," + seed.inst.OpCode.ToString() + "'" + seed.inst.Operand.ToString() + "' - '" + seed.seed + "'"); }
                File.WriteAllLines(asm.Name + "-LostMyMisoSoup_RNGSeeds.txt", text.ToArray());
            }
            pages.Add(currentPage.ToArray());
            ModuleWriterOptions moduleWriterOptions = new ModuleWriterOptions(asm);
            moduleWriterOptions.MetadataOptions.Flags |= MetadataFlags.PreserveAll;
            moduleWriterOptions.Logger = DummyLogger.NoThrowInstance;
            NativeModuleWriterOptions nativeModuleWriterOptions = new NativeModuleWriterOptions(asm, true);
            nativeModuleWriterOptions.MetadataOptions.Flags |= MetadataFlags.PreserveAll;
            nativeModuleWriterOptions.Logger = DummyLogger.NoThrowInstance;
            SetStatusText("Saving File...", ConsoleColor.White, ConsoleColor.DarkYellow);
            Console.WriteLine("Saving file...");
            string savedFileName;
            if (asm.IsILOnly)
            {
                savedFileName = asm.Name + "-LostMyMisoSoup_IL.exe";
                asm.Write(savedFileName, moduleWriterOptions);
            }
            else
            {
                savedFileName = asm.Name + "-LostMyMisoSoup_Native.exe";
                asm.NativeWrite(savedFileName, nativeModuleWriterOptions);
            }
            SetStatusText("Done! (saved as: '" + savedFileName + "')", ConsoleColor.White, ConsoleColor.DarkGreen);
            int pageIndex = pages.Count() - 1;
            //Console.WriteLine("done");
            wait:
            ConsoleKey key = Console.ReadKey().Key;
            /*
             * if im bothered ill make a page system for the action log
             * switch (key)
            {
                case ConsoleKey.LeftArrow:
                    if (pageIndex - 1 < 0) { goto wait; }
                    ReadPage(pages[pageIndex - 1], pages.Count(), pageIndex - 1);
                    pageIndex--;
                    goto wait;
                case ConsoleKey.RightArrow:
                    if (pageIndex + 1 >= pages.Count()) { goto wait; }
                    ReadPage(pages[pageIndex + 1], pages.Count(), pageIndex + 1);
                    pageIndex++;
                    goto wait;
            }*/
        }

        private static void InitializeSideContainer(int pageNum, int index)
        {
            int yPos = Console.CursorTop;
            for (int x = patchCount; x < Console.WindowHeight - 6; x++)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.CursorTop = x;
                Console.CursorLeft = 43;
                if (x == Console.WindowHeight - 7) { Console.BackgroundColor = ConsoleColor.Blue; Console.Write("  " + index + "/" + pageNum); }
                for (int x_ = Console.CursorLeft; x_ < Console.WindowWidth - 1; x_++)
                {
                    if (Console.CursorLeft == 56)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write("│");
                        Console.ForegroundColor = ConsoleColor.White;
                        if (x != Console.WindowHeight - 7) { Console.BackgroundColor = ConsoleColor.DarkBlue; }
                    }
                    else { Console.Write(" "); }
                }
                Console.WriteLine();
            }
            Console.CursorTop = yPos;
            Console.ResetColor();
        }

        public static void SetStatusText(string text, ConsoleColor foreColor = ConsoleColor.White, ConsoleColor backColor = ConsoleColor.DarkCyan)
        {
            int yPos = Console.CursorTop;
            Console.CursorTop = Console.WindowHeight - 2;
            Console.ForegroundColor = foreColor;
            Console.BackgroundColor = backColor;
            Console.WriteLine();
            Console.CursorTop--;
            Console.CursorLeft = 1;
            Console.Write(" " + text);
            for (int x = Console.CursorLeft; x < Console.WindowWidth - 2; x++) { Console.Write(" "); }
            Console.WriteLine();
            Console.ResetColor();
            Console.CursorTop = yPos;
        }

        private static int patchCount = 1;
        public static List<PatchLog[]> pages = new List<PatchLog[]>();
        public static List<PatchLog> currentPage = new List<PatchLog>();

        public static void AddPatchesLog(string name, string action, ConsoleColor nameBackColor, ConsoleColor nameForeColor, ConsoleColor actionBackColor, ConsoleColor actionForeColor)
        {
            int yPos = Console.CursorTop;
            Console.CursorTop = patchCount;
            if (Console.CursorTop == Console.WindowHeight - 8) { pages.Add(currentPage.ToArray()); currentPage.Clear(); patchCount = 2; Console.CursorTop = patchCount; InitializeSideContainer(pages.Count() + 1, pages.Count() + 1); }
            Console.CursorLeft = 43;
            Console.ForegroundColor = nameForeColor;
            Console.BackgroundColor = nameBackColor;
            Console.Write("  " + name + " ");
            if (name.Length < 10) { Console.Write("\t");}
            if (patchCount > 1)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.Write("│");
            }
            Console.BackgroundColor = nameBackColor;
            Console.ForegroundColor = actionForeColor;
            Console.BackgroundColor = actionBackColor;
            Console.Write("  " + action);
            for (int x = Console.CursorLeft; x < Console.WindowWidth - 1; x++) { Console.Write(" "); }
            patchCount++;
            Console.WriteLine();
            Console.ResetColor();
            Console.CursorTop = yPos;
            PatchLog ps = new PatchLog(action, name, actionForeColor, actionBackColor, nameForeColor, nameBackColor);
            currentPage.Add(ps);
        }

        public static void ReadPage(PatchLog[] plog, int pagenum, int index)
        {
            InitializeSideContainer(pagenum, index);
            foreach (PatchLog page in plog) { AddPatchesLog(page.name, page.text, page.nameBack, page.nameFore, page.back, page.fore); }
        }

        public class PatchLog
        {
            public PatchLog(string Text, string ActionName, ConsoleColor foreColor, ConsoleColor backColor, ConsoleColor namefore, ConsoleColor nameback)
            {
                text = Text;
                name = ActionName;
                fore = foreColor;
                back = backColor;
                nameBack = nameback;
                nameFore = namefore;
            }

            public string text;
            public string name;
            public ConsoleColor nameFore;
            public ConsoleColor nameBack;
            public ConsoleColor fore;
            public ConsoleColor back;
        }

        public static void PrintLog(string MethodName, string text)
        {
            Console.WriteLine(" [" + MethodName + "]:\t" + text);
        }
    }
}
