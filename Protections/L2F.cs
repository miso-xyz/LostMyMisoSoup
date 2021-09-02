using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace LMM_Rewritten.Protections
{
    class L2F
    {
        public static void FixL2F()
        {
            int count = 0;
            foreach (TypeDef type in Program.asm.Types)
            {
                foreach (MethodDef methods in type.Methods)
                {
                    if (!methods.HasBody) { continue; }
                    for (int x = 0; x < methods.Body.Instructions.Count(); x++)
                    {
                        Instruction inst = methods.Body.Instructions[x];
                        if (inst.Operand is FieldDef)
                        {
                            FieldDef field = ((FieldDef)inst.Operand);
                            Local newLoc = new Local(field.FieldType);
                            if (field.DeclaringType == Program.asm.GlobalType)
                            {
                                switch (inst.OpCode.Code)
                                {
                                    case Code.Stsfld:
                                        switch (methods.Body.Instructions[x + 1].OpCode.Code)
                                        {
                                            case Code.Ldsfld:
                                                methods.Body.Instructions[x + 1].OpCode = OpCodes.Ldloc;
                                                methods.Body.Instructions[x + 1].Operand = newLoc;
                                                break;
                                            case Code.Ldsflda:
                                                methods.Body.Instructions[x + 1].OpCode = OpCodes.Ldloca;
                                                methods.Body.Instructions[x + 1].Operand = newLoc;
                                                break;
                                        }
                                        inst.OpCode = OpCodes.Stloc;
                                        break;
                                    case Code.Ldsfld:
                                        inst.OpCode = OpCodes.Ldloc;
                                        break;
                                    case Code.Ldsflda:
                                        inst.OpCode = OpCodes.Ldloca;
                                        break;
                                }
                                methods.Body.Variables.Add(newLoc);
                                inst.Operand = newLoc;
                                count++;
                                Flags.LocalField = true;
                            }
                        }
                    }
                }
            }
            ConsoleColor fore = ConsoleColor.White;
            ConsoleColor back = ConsoleColor.DarkGreen;
            if (count == 0 || !Flags.LocalField) { fore = ConsoleColor.Black; back = ConsoleColor.DarkGray; }
            Program.AddPatchesLog("L2F", count + " Converted!", ConsoleColor.DarkBlue, ConsoleColor.White, back, fore);
        }

        public static void RemoveUselessFields()
        {
            int count = 0;
            List<FieldDef> ReferencedFields = new List<FieldDef>();
            foreach (TypeDef type in Program.asm.Types)
            {
                foreach (MethodDef methods in type.Methods)
                {
                    if (!methods.HasBody) { continue; }
                    foreach (Instruction inst in methods.Body.Instructions)
                    {
                        switch (inst.OpCode.Code)
                        {
                            case Code.Stsfld:
                            case Code.Ldsfld:
                            case Code.Ldsflda:
                                if (!ReferencedFields.Contains((FieldDef)inst.Operand)) { ReferencedFields.Add((FieldDef)inst.Operand); }
                                break;
                        }
                    }
                }
            }
            foreach (TypeDef type in Program.asm.Types)
            {
                for (int x = 0; x < type.Fields.Count(); x++)
                {
                    FieldDef field = type.Fields[x];
                    if (!ReferencedFields.Contains(field)) { type.Fields.RemoveAt(x); count++; x--; }
                }
            }
            ConsoleColor fore = ConsoleColor.White;
            ConsoleColor back = ConsoleColor.DarkRed;
            if (count == 0) { fore = ConsoleColor.Black; back = ConsoleColor.DarkGray; }
            Program.AddPatchesLog("Usl.Fields", "Removed " + count + " Fields!", ConsoleColor.DarkBlue, ConsoleColor.White, back, fore);
        }
    }
}
