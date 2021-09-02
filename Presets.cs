using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LMM_Rewritten
{
    class Presets
    {
        public class MethodRef
        {
            public MethodRef(string ns, string cl, string methods)
            {
                Namespace = ns;
                Class = cl;
                Method = methods;
            }

            public string Namespace;
            public string Class;
            public string Method;
        }
        public class ClassRef
        {
            public ClassRef(string ns, string cl)
            {
                Namespace = ns;
                Class = cl;
            }

            public string Namespace;
            public string Class;
        }

        public ClassRef AntiTamper_Throw = new ClassRef("System", "BadImageFormatException");
        public MethodRef AntiDump_Marshal = new MethodRef("System.Runtime.InteropServices", "Marshal", "GetHINSTANCE");
        public MethodRef AntiDebug_OSPlatform = new MethodRef("System", "OperatingSystem", "get_Platform");
        public MethodRef AntiDebug_EnvExit = new MethodRef("System", "Environment", "Exit");
        public MethodRef AntiDebug_EnvVar = new MethodRef("System", "Environment", "GetEnvironmentVariable");
        public MethodRef AntiDebug_EnvOSVer = new MethodRef("System", "Environment", "get_OSVersion");
        public MethodRef AntiDebug_DebugHooked = new MethodRef("System.Diagnostics", "Debugger", "get_IsAttached");
        public MethodRef AntiDebug_DebugLog = new MethodRef("System.Diagnostics", "Debugger", "IsLogging");
        public string JCF_Watermark = "MindLated.jpg";
        public ClassRef Math_Class = new ClassRef("System", "Math");
        public MethodRef Math_Truncate = new MethodRef("System", "Math", "Truncate");
        public MethodRef Math_Abs = new MethodRef("System", "Math", "Abs");
        public MethodRef Math_Log = new MethodRef("System", "Math", "Log");
        public MethodRef Math_Log10 = new MethodRef("System", "Math", "Log10");
        public MethodRef Math_Floor = new MethodRef("System", "Math", "Floor");
        public MethodRef Math_Round = new MethodRef("System", "Math", "Round");
        public MethodRef Math_Tan = new MethodRef("System", "Math", "Tan");
        public MethodRef Math_Tanh = new MethodRef("System", "Math", "Tanh");
        public MethodRef Math_Sqrt = new MethodRef("System", "Math", "Sqrt");
        public MethodRef Math_Ceiling = new MethodRef("System", "Math", "Ceiling");
        public MethodRef Math_Cos = new MethodRef("System", "Math", "Cos");
        public MethodRef Math_Sin = new MethodRef("System", "Math", "Sin");
        public double IntConf_Default = 1.5707963267949;
        public string Proxy_CommonName = "ProxyMeth";
        public string OnlineEncString_URL = "https://communitykeyv1.000webhostapp.com/Decoder4.php?string=";
        public ClassRef EncString_KeyAlgorithm = new ClassRef("System.Security.Cryptography", "Rfc2898DeriveBytes");
        public ClassRef EncString_AESAlgorithm = new ClassRef("System.Security.Cryptography", "RijndaelManaged");
        public ClassRef EncString_SymmetricAlg = new ClassRef("System.Security.Cryptography", "SymmetricAlgorithm");
        public string EncString_PasswordHash = "p7K95451qB88sZ7J";
        public byte[] EncString_Salt = Encoding.Default.GetBytes("2GM23j301t60Z96T");
        public byte[] EncString_VI = Encoding.Default.GetBytes("IzTdhG6S8uwg141S");
        public string EncString_ResourceName;

        private object FormatCall(string line)
        {
            string[] split = line.Split(',');
            switch (split.Count())
            {
                case 2:
                    return new ClassRef(split[0], split[1]);
                case 3:
                    return new MethodRef(split[0], split[1], split[2]);
            }
            return null;
        }
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
                    case "AntiTamper_Throw":
                        AntiTamper_Throw = (ClassRef)FormatCall(p2);
                        break;
                    case "AntiDump_Marshal":
                        AntiDump_Marshal = (MethodRef)FormatCall(p2);
                        break;
                    case "AntiDebug_OSPlatform":
                        AntiDebug_OSPlatform = (MethodRef)FormatCall(p2);
                        break;
                    case "AntiDebug_EnvExit":
                        AntiDebug_EnvExit = (MethodRef)FormatCall(p2);
                        break;
                    case "AntiDebug_EnvVar":
                        AntiDebug_EnvVar = (MethodRef)FormatCall(p2);
                        break;
                    case "AntiDebug_EnvOSVer":
                        AntiDebug_EnvOSVer = (MethodRef)FormatCall(p2);
                        break;
                    case "AntiDebug_DebugHooked":
                        AntiDebug_DebugHooked = (MethodRef)FormatCall(p2);
                        break;
                    case "AntiDebug_DebugLog":
                        AntiDebug_DebugLog = (MethodRef)FormatCall(p2);
                        break;
                    case "JCF_Watermark":
                        JCF_Watermark = p2;
                        break;
                    case "Math_Cos":
                        Math_Cos = (MethodRef)FormatCall(p2);
                        break;
                    case "Math_Sin":
                        Math_Sin = (MethodRef)FormatCall(p2);
                        break;
                    case "IntConf_Default":
                        IntConf_Default = Convert.ToDouble(p2.Replace(".", ","));
                        break;
                    case "Proxy_CommonName":
                        Proxy_CommonName = p2;
                        break;
                    case "EncString_PasswordHash":
                        EncString_PasswordHash = p2;
                        break;
                    case "EncString_Salt":
                        EncString_Salt = Convert.FromBase64String(p2.Replace("#", "="));
                        break;
                    case "EncString_VI":
                        EncString_VI = Convert.FromBase64String(p2.Replace("#", "="));
                        break;
                    case "EncString_ResourceName":
                        EncString_ResourceName = p2;
                        break;
                    case "Math_Truncate":
                        Math_Truncate = (MethodRef)FormatCall(p2);
                        break;
                    case "Math_Abs":
                        Math_Abs = (MethodRef)FormatCall(p2);
                        break;
                    case "Math_Log":
                        Math_Log = (MethodRef)FormatCall(p2);
                        break;
                    case "Math_Log10":
                        Math_Log10 = (MethodRef)FormatCall(p2);
                        break;
                    case "Math_Floor":
                        Math_Floor = (MethodRef)FormatCall(p2);
                        break;
                    case "Math_Round":
                        Math_Round = (MethodRef)FormatCall(p2);
                        break;
                    case "Math_Tan":
                        Math_Tan = (MethodRef)FormatCall(p2);
                        break;
                    case "Math_Tanh":
                        Math_Tanh = (MethodRef)FormatCall(p2);
                        break;
                    case "Math_Sqrt":
                        Math_Sqrt = (MethodRef)FormatCall(p2);
                        break;
                    case "Math_Ceiling":
                        Math_Ceiling = (MethodRef)FormatCall(p2);
                        break;
                    case "EncString_KeyAlgorithm":
                        EncString_KeyAlgorithm = (ClassRef)FormatCall(p2);
                        break;
                    case "EncString_AESAlgorithm":
                        EncString_AESAlgorithm = (ClassRef)FormatCall(p2);
                        break;
                    case "EncString_SymmetricAlg":
                        EncString_SymmetricAlg = (ClassRef)FormatCall(p2);
                        break;
                    case "OnlineEncString_URL":
                        OnlineEncString_URL = p2;
                        break;
                }
            }
        }
    }
}