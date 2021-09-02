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
            bool configFromFile = false;
            conf.Read("config.txt");
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
            Program.AddPatchesLog("   Name", "      Actions", ConsoleColor.DarkCyan, ConsoleColor.Black, ConsoleColor.DarkCyan, ConsoleColor.Black);
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
                SetStatusText("Fix Invalid MD...");
                InvalidMD.Fix();
            }
            if (conf.RemoveUselessJumps)
            {
                SetStatusText("Removing Useless Jumps...");
                CFlow.RemoveUselessBrs();
            }
            if (conf.FixCallis)
            {
                SetStatusText("Fix Callis...");
                CFlow.FixCallis();
            }
            if (conf.RemoveInvalidCalls)
            {
                SetStatusText("Cleaning up invalid calls...");
                Antis.RemoveJunkCalls(Program.asm.GlobalType);
            }
            if (conf.FixProxyConst || conf.FixProxyString)
            {
                SetStatusText("Fix Proxies...");
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
                SetStatusText("Fix Int Confusion...");
                IntConfusion.Fix();
            }
            if (conf.FixL2F)
            {
                SetStatusText("Fix L2F...");
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
                savedFileName = asm.Name + "-LostMyMind_IL.exe";
                asm.Write(savedFileName, moduleWriterOptions);
            }
            else
            {
                savedFileName = asm.Name + "-LostMyMind_Native.exe";
                asm.NativeWrite(savedFileName, nativeModuleWriterOptions);
            }
            SetStatusText("Done! (saved as: '" + savedFileName + "')", ConsoleColor.White, ConsoleColor.DarkGreen);
            Console.WriteLine("done");
            Console.ReadKey();
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

        private static int patchCount = 0;
        public static void AddPatchesLog(string name, string action, ConsoleColor nameBackColor, ConsoleColor nameForeColor, ConsoleColor actionBackColor, ConsoleColor actionForeColor)
        {
            int yPos = Console.CursorTop;
            Console.CursorTop = patchCount;
            Console.CursorLeft = 44;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.Write(" ");
            Console.ForegroundColor = nameForeColor;
            Console.BackgroundColor = nameBackColor;
            Console.Write(" " + name);
            if (name.Length < 10) { Console.Write("\t");}
            Console.Write("│");
            Console.ForegroundColor = actionForeColor;
            Console.BackgroundColor = actionBackColor;
            Console.Write("  " + action);
            for (int x = Console.CursorLeft; x < Console.WindowWidth - 1; x++)
            {
                Console.Write(" ");
            }
            patchCount++;
            Console.WriteLine();
            Console.ResetColor();
            Console.CursorTop = yPos;
        }

        public static void PrintLog(string MethodName, string text)
        {
            Console.WriteLine("[" + MethodName + "]: " + text);
        }
    }
}
