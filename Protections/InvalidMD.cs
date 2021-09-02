using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace LMM_Rewritten.Protections
{
    class InvalidMD
    {
        public static void Fix()
        {
            int count = 0;
            foreach (TypeDef type in Program.asm.Types)
            {
                foreach (MethodDef methods in type.Methods)
                {
                    if (!methods.HasBody) { continue; }
                    if (methods == Program.asm.EntryPoint) { int.Parse("0"); }
                    try
                    {
                        if (methods.Body.Instructions[5].OpCode == OpCodes.Ceq &&
                        methods.Body.Instructions[7].OpCode == OpCodes.Ceq &&
                        methods.Body.Instructions[10].OpCode == OpCodes.Brtrue_S &&
                        methods.Body.Instructions[11].OpCode == OpCodes.Ret)
                        {
                            int startIndex = methods.Body.Instructions.IndexOf((Instruction)methods.Body.Instructions[10].Operand);
                            for (int x = 0; x < startIndex; x++) { methods.Body.Instructions.RemoveAt(0); }
                            Flags.InvalidMD = true;
                        }
                    }
                    catch { }
                }
            }
            ConsoleColor fore = ConsoleColor.White;
            ConsoleColor back = ConsoleColor.DarkGreen;
            if (count == 0 || !Flags.InvalidMD) { fore = ConsoleColor.Black; back = ConsoleColor.DarkGray; }
            Program.AddPatchesLog("InvalidMD", "Fixed " + count + " Methods!", ConsoleColor.DarkBlue, ConsoleColor.White, back, fore);
        }
    }
}
