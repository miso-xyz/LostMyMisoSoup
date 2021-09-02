using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace LMM_Rewritten.Protections
{
    class CFlow
    {
        public static void RemoveUselessBrs()
        {
            int RemovedBRsCount = 0;
            foreach (TypeDef type in Program.asm.Types)
            {
                foreach (MethodDef methods in type.Methods)
                {
                    if (!methods.HasBody) { continue; }
                    for (int x = 0; x < methods.Body.Instructions.Count(); x++)
                    {
                        Instruction inst = methods.Body.Instructions[x];
                        Instruction nextInst;
                        try { nextInst = methods.Body.Instructions[x + 1]; }
                        catch { break; }
                        if (inst.OpCode == OpCodes.Br_S || inst.OpCode == OpCodes.Br)
                        {
                            if ((Instruction)inst.Operand == nextInst)
                            {
                                methods.Body.Instructions.RemoveAt(x);
                                x--;
                                RemovedBRsCount++;
                            }
                        }
                    }
                }
            }
            ConsoleColor fore = ConsoleColor.White;
            ConsoleColor back = ConsoleColor.DarkRed;
            if (RemovedBRsCount == 0) { fore = ConsoleColor.Black; back = ConsoleColor.DarkGray; }
            Program.AddPatchesLog("CFlow BRs", "Removed " + RemovedBRsCount + "!", ConsoleColor.DarkBlue, ConsoleColor.White, back, fore);
        }

        #region Taken from DevT02's Junk Remover - Useless NOPs Remover
        public static void RemoveUselessNops()
        {
            int count = 0;
            foreach (var type in Program.asm.Types.Where(t => t.HasMethods))
            {
                foreach (var method in type.Methods.Where(m => m.HasBody && m.Body.HasInstructions))
                {
                    if (method.HasBody)
                    {
                        var instr = method.Body.Instructions;
                        for (int i = 0; i < instr.Count; i++)
                        {
                            if (instr[i].OpCode == OpCodes.Nop &&
                                !IsNopBranchTarget(method, instr[i]) &&
                                !IsNopSwitchTarget(method, instr[i]) &&
                                !IsNopExceptionHandlerTarget(method, instr[i]))
                            {
                                instr.RemoveAt(i);
                                count++;
                                i--;
                            }
                        }
                    }
                }
            }
            ConsoleColor fore = ConsoleColor.White;
            ConsoleColor back = ConsoleColor.DarkRed;
            if (count == 0) { fore = ConsoleColor.Black; back = ConsoleColor.DarkGray; }
            Program.AddPatchesLog("CFlow NOP", "Removed " + count + "!", ConsoleColor.DarkBlue, ConsoleColor.White, back, fore);
        }
        private static bool IsNopSwitchTarget(MethodDef method, Instruction nopInstr)
        {
            var instr = method.Body.Instructions;
            for (int i = 0; i < instr.Count; i++)
            {
                if (instr[i].OpCode.OperandType == OperandType.InlineSwitch && instr[i].Operand != null)
                {
                    Instruction[] source = (Instruction[])instr[i].Operand;
                    if (source.Contains(nopInstr))
                        return true;
                }
            }
            return false;
        }
        private static bool IsNopExceptionHandlerTarget(MethodDef method, Instruction nopInstr)
        {
            bool result;
            if (!method.Body.HasExceptionHandlers)
                result = false;
            else
            {
                var exceptionHandlers = method.Body.ExceptionHandlers;
                foreach (var exceptionHandler in exceptionHandlers)
                {
                    if (exceptionHandler.FilterStart == nopInstr ||
                        exceptionHandler.HandlerEnd == nopInstr ||
                        exceptionHandler.HandlerStart == nopInstr ||
                        exceptionHandler.TryEnd == nopInstr ||
                        exceptionHandler.TryStart == nopInstr)
                        return true;
                }
                result = false;
            }
            return result;
        }
        private static bool IsNopBranchTarget(MethodDef method, Instruction nopInstr)
        {
            var instr = method.Body.Instructions;
            for (int i = 0; i < instr.Count; i++)
            {
                if (instr[i].OpCode.OperandType == OperandType.InlineBrTarget || instr[i].OpCode.OperandType == OperandType.ShortInlineBrTarget && instr[i].Operand != null)
                {
                    Instruction instruction2 = (Instruction)instr[i].Operand;
                    if (instruction2 == nopInstr)
                        return true;
                }
            }
            return false;
        }
        #endregion

        public static void FixCallis()
        {
            int count = 0;
            foreach (TypeDef type in Program.asm.Types)
            {
                foreach (MethodDef methods in type.Methods)
                {
                    if (!methods.HasBody) { continue; }
                    string methodName = methods.FullName;
                    if (methodName.Length > 35) { methodName = methodName.Substring(0, 34) + "..."; }
                    Program.SetStatusText("Searching Callis - " + methodName, ConsoleColor.White, ConsoleColor.DarkYellow);
                    for (int x = 0; x < methods.Body.Instructions.Count(); x++)
                    {
                        Instruction inst = methods.Body.Instructions[x];
                        if (inst.OpCode.Equals(OpCodes.Ldftn) && methods.Body.Instructions[x + 1].OpCode.Equals(OpCodes.Calli))
                        {
                            inst.OpCode = OpCodes.Call;
                            methods.Body.Instructions.RemoveAt(x + 1);
                            Flags.Calli = true;
                            x--;
                            count++;
                        } 
                    }
                }
            }
            ConsoleColor fore = ConsoleColor.White;
            ConsoleColor back = ConsoleColor.DarkGreen;
            if (count == 0 || !Flags.Calli) { fore = ConsoleColor.Black; back = ConsoleColor.DarkGray; }
            Program.AddPatchesLog("Calli", "Fixed " + count + " Calls!", ConsoleColor.DarkBlue, ConsoleColor.White, back, fore);

        }

        public static void RemoveWatermarks()
        {
            int WatermarkCount = 0;
            foreach (TypeDef type in Program.asm.Types)
            {
                if (type.IsGlobalModuleType) { continue; }
                foreach (MethodDef methods in type.Methods)
                {
                    if (!methods.HasBody) { continue; }
                    string methodName = methods.FullName;
                    if (methodName.Length > 35) { methodName = methodName.Substring(0, 34) + "..."; }
                    Program.SetStatusText("Searching Watermarks - " + methodName, ConsoleColor.White, ConsoleColor.DarkYellow);
                    for (int x = 0; x < methods.Body.Instructions.Count(); x++)
                    {
                        Instruction inst = methods.Body.Instructions[x];
                        if (inst.OpCode == OpCodes.Ldstr)
                        {
                            if (inst.Operand.ToString() == Program.Preset.JCF_Watermark)
                            {
                                methods.Body.Instructions.RemoveAt(x);
                                Flags.JumpCFlow = true;
                                WatermarkCount++;
                                x--;
                            }
                        }
                    }
                }
            }
            ConsoleColor fore = ConsoleColor.White;
            ConsoleColor back = ConsoleColor.DarkRed;
            if (WatermarkCount == 0 || !Flags.JumpCFlow) { fore = ConsoleColor.Black; back = ConsoleColor.DarkGray; }
            Program.AddPatchesLog("Watermark", "Removed " + WatermarkCount + "!", ConsoleColor.DarkBlue, ConsoleColor.White, back, fore);
        }
    }
}
