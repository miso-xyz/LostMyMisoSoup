using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LMM_Rewritten
{
    class Config
    {
        public bool ForceDefaultPreset = false;
        public string PresetFilePath = "preset_file.txt";
        public bool ExportRNGSeeds = true;
        public bool PresetDevMode = false;

        public bool RemoveInvalidCalls = true;
        public bool RemoveUselessJumps = true;
        public bool RemoveUselessNOPs = true;
        public bool RemoveUnusedLocals = true;
        public bool RemoveUnusedVariables = true;

        public bool FixAntiTamper = true;
        public bool FixAntiDump = true;
        public bool FixAntiDebug = true;
        public bool FixAntiDe4Dot = true;
        public bool FixWatermarks = true;
        public bool FixL2F = true;
        public bool FixJCF = true;
        public bool FixProxyConst = true;
        public bool FixProxyString = true;
        public bool FixIntConf = true;
        public bool FixArithmatic = true;
        public bool FixStringsEncr = true;
        public bool FixOnlineStringEncr = true;
        public bool FixResourceEncr = true;
        public bool FixStackUnfConf = true;
        public bool FixCallis = true;
        public bool FixInvalidMD = true;

        public void Read(string path)
        {
            string[] fileData = File.ReadAllLines(path);
            foreach (string line in fileData)
            {
                if (line.StartsWith("[") || line.StartsWith("//") || line == "") { continue; }
                string p1 = line.Split("=".ToCharArray())[0].Replace("\t", null);
                string p2 = line.Split("=".ToCharArray())[1].Remove(0,1);
                switch (p1)
                {
                    case "ForceDefault":
                        ForceDefaultPreset = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "PresetFile":
                        PresetFilePath = p2;
                        break;
                    case "ExportRNGSeeds":
                        ExportRNGSeeds = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "AntiTamper":
                        FixAntiTamper = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "AntiDump":
                        FixAntiDump = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "AntiDebug":
                        FixAntiDebug = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "AntiDe4Dot":
                        FixAntiDe4Dot = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "Watermark":
                        FixWatermarks = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "JumpCFlow":
                        FixJCF = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "ProxyConstants":
                        FixProxyConst = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "ProxyStrings":
                        FixProxyString = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "IntConfusion":
                        FixIntConf = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "Arithmatic":
                        FixArithmatic = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "StringsEncryption":
                        FixStringsEncr = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "OnlineStringDecryption":
                        FixOnlineStringEncr = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "ResourceEncryption":
                        FixResourceEncr = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "StackUnfConfusion":
                        FixStackUnfConf = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "Calli":
                        FixCallis = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "InvalidMD":
                        FixInvalidMD = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "PresetDevMode":
                        PresetDevMode = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "Local2Field":
                        FixL2F = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "InvalidCalls":
                        RemoveInvalidCalls = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "UselessJumps":
                        RemoveUselessJumps = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "UselessNOPs":
                        RemoveUselessNOPs = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "UnusedLocals":
                        RemoveUnusedLocals = Convert.ToBoolean(int.Parse(p2));
                        break;
                    case "UnusedVariables":
                        RemoveUnusedVariables = Convert.ToBoolean(int.Parse(p2));
                        break;
                }
            }
        }
    }
}
