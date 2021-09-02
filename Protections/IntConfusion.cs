using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace LMM_Rewritten.Protections
{
    class IntConfusion
    {
        /*class ConfusionMethod
        {
            public ConfusionMethod(int id, int pos, int instcount)
            {
                ID = id;
                index = pos;
                InstructionCount = instcount;
            }
        }*/

        public static List<RngSeed> RngSeeds = new List<RngSeed>();

        public static void FixStackUnf()
        {
            int count = 0;
            foreach (TypeDef type in Program.asm.Types)
            {
                foreach (MethodDef methods in type.Methods)
                {
                    if (methods == Program.asm.EntryPoint) {int.Parse("0");}
                    if (!methods.HasBody) { continue; }
                    if (methods.Body.Instructions[0].OpCode == OpCodes.Ldc_I8 &&
                        methods.Body.Instructions[1].OpCode == OpCodes.Pop &&
                        (methods.Body.Instructions[2].OpCode == OpCodes.Br ||
                        methods.Body.Instructions[2].OpCode == OpCodes.Br_S) &&
                        (Instruction)methods.Body.Instructions[2].Operand == methods.Body.Instructions[3])
                    {
                        string methodName = methods.FullName;
                        if (methodName.Length > 35) { methodName = methodName.Substring(0, 34) + "..."; }
                        Program.SetStatusText("Fixing StackUnfConfusion - " + methodName, ConsoleColor.White, ConsoleColor.DarkYellow);
                        count++;
                        Flags.StackUnfConf = true;
                        for (int x = 0; x < 3; x++) { methods.Body.Instructions.RemoveAt(0); }
                    }
                }
            }
            ConsoleColor fore = ConsoleColor.White;
            ConsoleColor back = ConsoleColor.DarkGreen;
            if (count == 0 || !Flags.StackUnfConf) { fore = ConsoleColor.Black; back = ConsoleColor.DarkGray; }
            Program.AddPatchesLog("StackUnf", "Fixed " + count + " Methods!", ConsoleColor.DarkBlue, ConsoleColor.White, back, fore);
        }
        public static void RemoveUselessLocals()
        {
            int count = 0;
            foreach (TypeDef type in Program.asm.Types)
            {
                foreach (MethodDef methods in type.Methods)
                {
                    List<Local> ReferencedLocals = new List<Local>();
                    if (!methods.HasBody) { continue; }
                    foreach (Instruction inst in methods.Body.Instructions)
                    {
                        if (methods == Program.asm.EntryPoint) { int.Parse("0"); }
                        switch (inst.OpCode.Code)
                        {
                            case Code.Stloc_S:
                            case Code.Ldloc_S:
                            case Code.Stloc:
                            case Code.Ldloc:
                                if (!ReferencedLocals.Contains((Local)inst.Operand)) { ReferencedLocals.Add((Local)inst.Operand); }
                                break;
                            case Code.Ldloc_0:
                            case Code.Ldloc_1:
                            case Code.Ldloc_2:
                            case Code.Ldloc_3:
                            case Code.Stloc_0:
                            case Code.Stloc_1:
                            case Code.Stloc_2:
                            case Code.Stloc_3:
                                int localIndex = int.Parse(inst.OpCode.Name.Split('.')[1]);
                                if (!ReferencedLocals.Contains(methods.Body.Variables[localIndex])) { ReferencedLocals.Add(methods.Body.Variables[localIndex]); }
                                break;
                        }
                    }
                    for (int x = 0; x < methods.Body.Variables.Count(); x++)
                    {
                        Local var = methods.Body.Variables[x];
                        if (!ReferencedLocals.Contains(var))
                        {
                            methods.Body.Variables.RemoveAt(x);
                            x--;
                            count++;
                        }
                    }
                }
            }
            ConsoleColor fore = ConsoleColor.White;
            ConsoleColor back = ConsoleColor.DarkRed;
            if (count == 0) { fore = ConsoleColor.Black; back = ConsoleColor.DarkGray; }
            Program.AddPatchesLog("Usl.Locals", "Removed " + count + " Locals!", ConsoleColor.DarkBlue, ConsoleColor.White, back, fore);
        }

        public static void Fix()
        {
            int count = 0;
            foreach (TypeDef type in Program.asm.Types)
            {
                if (type.IsGlobalModuleType) { continue; }
                foreach (MethodDef methods in type.Methods)
                {
                    if (!methods.HasBody) { continue; }
                    for (int x = 0; x < methods.Body.Instructions.Count(); x++)
                    {
                        Instruction inst = methods.Body.Instructions[x];
                        switch (inst.OpCode.Code)
                        {
                            case Code.Stloc:
                                Local currentVar = (Local)inst.Operand;
                                if (methods.Body.Instructions[x + 4].OpCode == OpCodes.Xor &&
                                    methods.Body.Instructions[x + 6].OpCode == OpCodes.Bne_Un &&
                                    (Instruction)methods.Body.Instructions[x + 6].Operand == methods.Body.Instructions[x + 11] &&
                                    (Local)methods.Body.Instructions[x + 8].Operand == currentVar)
                                {
                                    int ogint = methods.Body.Instructions[x + 1].GetLdcI4Value() + 4;
                                    int curPos = methods.Body.Instructions.IndexOf(inst);
                                    for (int x_remInst = 0; x_remInst < 12; x_remInst++) { methods.Body.Instructions.RemoveAt(x-1); }
                                    if (x - 12 < 0) { x = 0; } else { x = x - 12; }
                                    methods.Body.Instructions.Insert(curPos, Instruction.CreateLdcI4(ogint));
                                    count++;
                                }
                                Flags.IntConf = true;
                                break;
                            case Code.Ldc_R8:
                                if (double.Parse(inst.Operand.ToString()) == Program.Preset.IntConf_Default)
                                {
                                    if (methods.Body.Instructions[x - 2].OpCode == OpCodes.Sizeof &&
                                        methods.Body.Instructions[x - 1].OpCode == OpCodes.Add &&
                                        (Antis.isNamespace(methods.Body.Instructions[x + 1], Program.Preset.Math_Sin.Namespace) &&
                                             Antis.isClass(methods.Body.Instructions[x + 1], Program.Preset.Math_Sin.Class) &&
                                            Antis.isMethod(methods.Body.Instructions[x + 1], Program.Preset.Math_Sin.Method)) &&
                                        (Antis.isNamespace(methods.Body.Instructions[x + 7], Program.Preset.Math_Cos.Namespace) &&
                                             Antis.isClass(methods.Body.Instructions[x + 7], Program.Preset.Math_Cos.Class) &&
                                            Antis.isMethod(methods.Body.Instructions[x + 7], Program.Preset.Math_Cos.Method)))
                                    {
                                        RngSeeds.Add(new RngSeed(methods, Convert.ToInt32(Math.PI / Convert.ToDouble(methods.Body.Instructions[x + 6].Operand.ToString())), methods.Body.Instructions[x + 6]));
                                        for (int x_remInst_ = 0; x_remInst_ < 12; x_remInst_++) { methods.Body.Instructions.RemoveAt(x-2); }
                                        if (x - 12 < 0) { x = 0; } else { x = x - 12; }
                                    }
                                }
                                Flags.IntConf = true;
                                break;
                        }
                    }
                }
            }
            ConsoleColor fore = ConsoleColor.White;
            ConsoleColor back = ConsoleColor.DarkGreen;
            if (count == 0 || !Flags.IntConf) { fore = ConsoleColor.Black; back = ConsoleColor.DarkGray; }
            Program.AddPatchesLog("IntConf", "Fixed " + count + " Integers!", ConsoleColor.DarkBlue, ConsoleColor.White, back, fore);
        }

        /*public static int ComputeConfusion(MethodDef method)
        {
            switch (ConfMethodsID[ConfMethodsID.Count()-1])
            {
                case 1:
                    int RNG_1 = method.Body.Instructions[ConfMethodsIndexes[ConfMethodsIndexes.Count() - 1] + 5].GetLdcI4Value();
                    int RNG_2 = method.Body.Instructions[ConfMethodsIndexes[ConfMethodsIndexes.Count() - 1] + 3].GetLdcI4Value();
                    if (((RNG_1 ^ RNG_2) ^ RNG_2) == RNG_1)
                    {
                        return sizeof(float)+2;
                    }
                    break;
            }
            return int.MinValue;
        }*/
    }
}
