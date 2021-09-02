using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace LMM_Rewritten.Protections
{
    class Antis
    {
        public static void FixAntiDe4dot()
        {
            //Program.PrintLog("AntiDe4dot", "Searching...");
            for (int x = 0; x < Program.asm.Types.Count(); x++)
            {
                TypeDef type = Program.asm.Types[x];
                Program.SetStatusText("Searching AntiDe4Dots - " + type.Name, ConsoleColor.White, ConsoleColor.DarkYellow);
                if (type.HasInterfaces)
                {
                    foreach (InterfaceImpl intface in type.Interfaces)
                    {
                        if ((TypeDef)intface.Interface == type)
                        {
                            string tName = type.Name;
                            Program.asm.Types.Remove(type);
                            x--;
                            Flags.AntiDe4Dot = true;
                            Program.AddPatchesLog("AntiDe4Dot", "Removed '" + tName + "'!", ConsoleColor.DarkBlue, ConsoleColor.White, ConsoleColor.DarkRed, ConsoleColor.White);
                        }
                    }
                }
            }
            if (!Flags.AntiDe4Dot) { Program.AddPatchesLog("AntiDe4Dot", "None Found!", ConsoleColor.DarkBlue, ConsoleColor.White, ConsoleColor.Gray, ConsoleColor.Black); }
        }

        public static void FixAntiDump()
        {
            //Program.PrintLog("AntiDump", "Searching...");
            foreach (MethodDef methods in Program.asm.GlobalType.Methods)
            {
                if (!methods.HasBody) { continue; }
                string methodName = methods.DeclaringType.Name + "." + methods.Name;
                if (methodName.Length > 35) { methodName = methodName.Substring(0, 34) + "..."; }
                Program.SetStatusText("Searching AntiDump - " + methodName, ConsoleColor.White, ConsoleColor.DarkYellow);
                for (int x = 0; x < methods.Body.Instructions.Count(); x++)
                {
                    Instruction inst = methods.Body.Instructions[x];
                    if (inst.OpCode == OpCodes.Call
                        && isMethod(inst, Program.Preset.AntiDump_Marshal.Method)
                        && isClass(inst, Program.Preset.AntiDump_Marshal.Class)
                        && isNamespace(inst, Program.Preset.AntiDump_Marshal.Namespace))
                    {
                        Program.asm.GlobalType.Methods.Remove(methods);
                        Flags.AntiDump = true;
                        Program.AddPatchesLog("AntiDump", "Removed!", ConsoleColor.DarkBlue, ConsoleColor.White, ConsoleColor.DarkRed, ConsoleColor.White);
                        return;
                    }
                }
            }
            if (!Flags.AntiDump) { Program.AddPatchesLog("AntiDump", "Not Found!", ConsoleColor.DarkBlue, ConsoleColor.White, ConsoleColor.Gray, ConsoleColor.Black); }
        }

        public static void FixAntiDebug()
        {
            //Program.PrintLog("AntiDebug", "Searching...");
            foreach (MethodDef methods in Program.asm.GlobalType.Methods)
            {
                bool isLoggingCheck = false, isAttachedCheck = false, ExitEnvCheck = false, GetEnvVarCheck = false, GetOSCheck = false, GetPlatformCheck = false;
                if (!methods.HasBody) { continue; }
                string methodName = methods.DeclaringType.Name + "." + methods.Name;
                if (methodName.Length > 35) { methodName = methodName.Substring(0, 34) + "..."; }
                Program.SetStatusText("Searching AntiDebug - " + methodName, ConsoleColor.White, ConsoleColor.DarkYellow);
                for (int x = 0; x < methods.Body.Instructions.Count(); x++)
                {
                    Instruction inst = methods.Body.Instructions[x];
                    switch (inst.OpCode.Code)
                    {
                        case Code.Call:
                            if (isNamespace(inst, Program.Preset.AntiDebug_DebugHooked.Namespace) &&
                                isClass(inst, Program.Preset.AntiDebug_DebugHooked.Class) &&
                                isMethod(inst, Program.Preset.AntiDebug_DebugHooked.Method)) { isAttachedCheck = true; }
                            if (isNamespace(inst, Program.Preset.AntiDebug_DebugLog.Namespace) &&
                                isClass(inst, Program.Preset.AntiDebug_DebugLog.Class) &&
                                isMethod(inst, Program.Preset.AntiDebug_DebugLog.Method)) { isLoggingCheck = true; }
                            if (isNamespace(inst, Program.Preset.AntiDebug_EnvExit.Namespace) &&
                                isClass(inst, Program.Preset.AntiDebug_EnvExit.Class) &&
                                isMethod(inst, Program.Preset.AntiDebug_EnvExit.Method)) { ExitEnvCheck = true; }
                            if (isNamespace(inst, Program.Preset.AntiDebug_EnvVar.Namespace) &&
                                isClass(inst, Program.Preset.AntiDebug_EnvVar.Class) &&
                                isMethod(inst, Program.Preset.AntiDebug_EnvVar.Method)) { GetEnvVarCheck = true; }
                            if (isNamespace(inst, Program.Preset.AntiDebug_EnvOSVer.Namespace) &&
                                isClass(inst, Program.Preset.AntiDebug_EnvOSVer.Class) &&
                                isMethod(inst, Program.Preset.AntiDebug_EnvOSVer.Method)) { GetOSCheck = true; }
                            break;
                        case Code.Callvirt:
                            if (isNamespace(inst, Program.Preset.AntiDebug_OSPlatform.Namespace) &&
                                isClass(inst, Program.Preset.AntiDebug_OSPlatform.Class) &&
                                isMethod(inst, Program.Preset.AntiDebug_OSPlatform.Method)) { GetPlatformCheck = true; }
                            break;
                    }
                }
                if (isLoggingCheck && isAttachedCheck && ExitEnvCheck && GetEnvVarCheck && GetOSCheck && GetPlatformCheck)
                {
                    Program.asm.GlobalType.Methods.Remove(methods);
                    Flags.AntiDebug = true;
                    Program.AddPatchesLog("AntiDebug", "Removed!", ConsoleColor.DarkBlue, ConsoleColor.White, ConsoleColor.DarkRed, ConsoleColor.White);
                    return;
                }
            }
            if (!Flags.AntiDebug) { Program.AddPatchesLog("AntiDebug", "Not Found!", ConsoleColor.DarkBlue, ConsoleColor.White, ConsoleColor.Gray, ConsoleColor.Black); }
        }

        public static bool isClass(Instruction inst, string ClassName)
        {
            try { return ((MemberRef)inst.Operand).Class.Name == ClassName; }
            catch { return false; }
        }

        public static bool isMethod(Instruction inst, string MethodName)
        {
            try { return ((MemberRef)inst.Operand).Name == MethodName; }
            catch { return false; }
        }

        public static bool isNamespace(Instruction inst, string Namespace)
        {
            try { return (((TypeRef)((MemberRef)inst.Operand).Class)).Namespace == Namespace; }
            catch { return false; }
        }

        public static void RemoveJunkCalls(TypeDef type)
        {
            foreach (MethodDef methods in type.Methods)
            {
                if (!methods.HasBody) { continue; }
                if (methods.IsConstructor) { int z = 0; }
                for (int x = 0; x < methods.Body.Instructions.Count(); x++)
                {
                    Instruction inst = methods.Body.Instructions[x];
                    if (inst.Operand is MethodDef)
                    {
                        if (inst.OpCode.Equals(OpCodes.Call) && ((MethodDef)inst.Operand).DeclaringType == null)
                        {
                            methods.Body.Instructions.RemoveAt(x);
                            x--;
                        }
                    }
                }
            }
        }

        public static void FixAntiTamper()
        {
            //Program.PrintLog("AntiTamper", "Searching...");
            for (int x = 0; x < Program.asm.GlobalType.Methods.Count(); x++)
            {
                MethodDef methods = Program.asm.GlobalType.Methods[x];
                if (!methods.HasBody) { continue; }
                string methodName = methods.DeclaringType.Name + "." + methods.Name;
                if (methodName.Length > 35) { methodName = methodName.Substring(0, 34) + "..."; }
                Program.SetStatusText("Searching AntiTamper - " + methodName, ConsoleColor.White, ConsoleColor.DarkYellow);
                for (int x_inst = 0; x_inst < methods.Body.Instructions.Count(); x_inst++)
                {
                    Instruction inst = methods.Body.Instructions[x_inst];
                    if (inst.OpCode == OpCodes.Throw)
                    {
                        if (inst.OpCode == OpCodes.Throw && isClass(methods.Body.Instructions[x_inst - 1], "BadImageFormatException") && isNamespace(methods.Body.Instructions[x_inst - 1], "System"))
                        {
                            Program.asm.GlobalType.Methods.Remove(methods);
                            Flags.AntiTamper = true;
                            Program.AddPatchesLog("AntiTamper", "Removed!", ConsoleColor.DarkBlue, ConsoleColor.White, ConsoleColor.DarkRed, ConsoleColor.White);
                            return;
                        }
                    }
                }
            }
            if (Flags.AntiTamper)
            {
                if (!Flags.AntiTamper) { Program.AddPatchesLog("AntiTamper", "Not Found!", ConsoleColor.DarkBlue, ConsoleColor.White, ConsoleColor.Gray, ConsoleColor.Black); }
            }
        }
    }
}
