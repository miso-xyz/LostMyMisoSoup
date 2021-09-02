using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace LMM_Rewritten.Protections
{
    class Arithmatic
    {
        public enum CalculationMethod
        {
            Abs,
            Cos,
            Sin,
            Floor,
            Log,
            Log10,
            Truncate,
            Round,
            Tan,
            Tanh,
            Sqrt,
            Ceiling
        }

        public static void Fix()
        {
            //Program.PrintLog("Arithmatic", "Computing Calculations...");
            int count = 0;
            foreach (TypeDef type in Program.asm.Types)
            {
                foreach (MethodDef methods in type.Methods)
                {
                    string methodName_ = methods.DeclaringType.Name + "." + methods.Name;
                    if (methodName_.Length > 35) { methodName_ = methodName_.Substring(0, 34) + "..."; }
                    Program.SetStatusText("Computing Arithmatic - " + methodName_ + " (Math Calls)", ConsoleColor.White, ConsoleColor.DarkYellow);
                    if (!methods.HasBody) { continue; }
                    for (int x = 0; x < methods.Body.Instructions.Count(); x++)
                    {
                        Instruction inst = methods.Body.Instructions[x];
                        switch (inst.OpCode.Code)
                        {
                            case Code.Call:
                                if (Antis.isNamespace(inst, Program.Preset.Math_Class.Namespace) &&
                                    Antis.isClass(inst, Program.Preset.Math_Class.Class))
                                {
                                    Instruction prevInst = methods.Body.Instructions[x - 1];
                                    string methodName = ((MemberRef)inst.Operand).Name;
                                    int returnValue = -1;
                                    if (Program.Preset.Math_Abs.Method == methodName) { returnValue = Compute(prevInst, CalculationMethod.Abs); }
                                    else if (Program.Preset.Math_Ceiling.Method == methodName) { returnValue = Compute(prevInst, CalculationMethod.Ceiling); }
                                    else if (Program.Preset.Math_Cos.Method == methodName) { returnValue = Compute(prevInst, CalculationMethod.Cos); }
                                    else if (Program.Preset.Math_Floor.Method == methodName) { returnValue = Compute(prevInst, CalculationMethod.Floor); }
                                    else if (Program.Preset.Math_Log.Method == methodName) { returnValue = Compute(prevInst, CalculationMethod.Log); }
                                    else if (Program.Preset.Math_Log10.Method == methodName) { returnValue = Compute(prevInst, CalculationMethod.Log10); }
                                    else if (Program.Preset.Math_Round.Method == methodName) { returnValue = Compute(prevInst, CalculationMethod.Round); }
                                    else if (Program.Preset.Math_Sin.Method == methodName) { returnValue = Compute(prevInst, CalculationMethod.Sin); }
                                    else if (Program.Preset.Math_Sqrt.Method == methodName) { returnValue = Compute(prevInst, CalculationMethod.Sqrt); }
                                    else if (Program.Preset.Math_Tan.Method == methodName) { returnValue = Compute(prevInst, CalculationMethod.Tan); }
                                    else if (Program.Preset.Math_Tanh.Method == methodName) { returnValue = Compute(prevInst, CalculationMethod.Tanh); }
                                    else if (Program.Preset.Math_Truncate.Method == methodName) { returnValue = Compute(prevInst, CalculationMethod.Truncate); }
                                    prevInst = Instruction.CreateLdcI4(returnValue);
                                    methods.Body.Instructions.RemoveAt(x);
                                    count++;
                                    x--;
                                    Flags.Arithmetic = true;
                                }
                                break;
                        }
                    }
                }

                foreach (MethodDef methods in type.Methods)
                {
                    if (!methods.HasBody) { continue; }
                    string methodName_ = methods.DeclaringType.Name + "." + methods.Name;
                    if (methodName_.Length > 35) { methodName_ = methodName_.Substring(0, 34) + "..."; }
                    Program.SetStatusText("Computing Arithmatic - " + methodName_ + " (Math Calls)", ConsoleColor.White, ConsoleColor.DarkYellow);
                    for (int x = 0; x < methods.Body.Instructions.Count(); x++)
                    {
                        Instruction inst = methods.Body.Instructions[x];
                        switch (inst.OpCode.Code)
                        {
                            case Code.Add:
                            case Code.Sub:
                            case Code.Mul:
                            case Code.Div:
                            case Code.Xor:
                                Instruction instVal1 = methods.Body.Instructions[x - 2];
                                Instruction instVal2 = methods.Body.Instructions[x - 1];
                                if ((instVal1.Operand is double || instVal1.Operand is int) && (instVal2.Operand is double || instVal2.Operand is int))
                                {
                                    object Value1 = instVal1.Operand;
                                    object Value2 = instVal2.Operand;
                                    double returnValue_ = -1;
                                    switch (inst.OpCode.Code)
                                    {
                                        case Code.Add:
                                            returnValue_ = Convert.ToDouble(Value1) + Convert.ToDouble(Value2);
                                            break;
                                        case Code.Sub:
                                            returnValue_ = Convert.ToDouble(Value1) - Convert.ToDouble(Value2);
                                            break;
                                        case Code.Mul:
                                            returnValue_ = Convert.ToDouble(Value1) * Convert.ToDouble(Value2);
                                            break;
                                        case Code.Div:
                                            returnValue_ = Convert.ToDouble(Value1) / Convert.ToDouble(Value2);
                                            break;
                                        case Code.Xor:
                                            returnValue_ = Convert.ToDouble(Convert.ToInt64(Value1) ^ Convert.ToInt64(Value2));
                                            break;
                                    }
                                    methods.Body.Instructions[x - 2] = Instruction.Create(OpCodes.Ldc_R8, returnValue_);
                                    methods.Body.Instructions.RemoveAt(x - 1);
                                    methods.Body.Instructions.RemoveAt(x - 1);
                                    methods.Body.Instructions.RemoveAt(x - 1);
                                    x = x - 2;
                                    count++;
                                }
                                break;
                        }
                    }
                }
            }
            ConsoleColor fore = ConsoleColor.White;
            ConsoleColor back = ConsoleColor.DarkRed;
            if (count == 0 || !Flags.Arithmetic) { fore = ConsoleColor.Black; back = ConsoleColor.DarkGray; }
            Program.AddPatchesLog("Arithmatic", "Solved " + count + " Calc.!", ConsoleColor.DarkBlue, ConsoleColor.White, back, fore);
        }

        private static int Compute(Instruction inst, CalculationMethod calcMethod)
        {
            switch (calcMethod)
            {
                case CalculationMethod.Abs:
                    return Convert.ToInt32(Math.Abs((double)inst.Operand));
                case CalculationMethod.Ceiling:
                    return Convert.ToInt32(Math.Ceiling((double)inst.Operand));
                case CalculationMethod.Cos:
                    return Convert.ToInt32(Math.Cos((double)inst.Operand));
                case CalculationMethod.Floor:
                    return Convert.ToInt32(Math.Floor((double)inst.Operand));
                case CalculationMethod.Log:
                    return Convert.ToInt32(Math.Log((double)inst.Operand));
                case CalculationMethod.Log10:
                    return Convert.ToInt32(Math.Log10((double)inst.Operand));
                case CalculationMethod.Round:
                    return Convert.ToInt32(Math.Round((double)inst.Operand));
                case CalculationMethod.Sin:
                    return Convert.ToInt32(Math.Sin((double)inst.Operand));
                case CalculationMethod.Sqrt:
                    return Convert.ToInt32(Math.Sqrt((double)inst.Operand));
                case CalculationMethod.Tan:
                    return Convert.ToInt32(Math.Tan((double)inst.Operand));
                case CalculationMethod.Tanh:
                    return Convert.ToInt32(Math.Tanh((double)inst.Operand));
                case CalculationMethod.Truncate:
                    return Convert.ToInt32(Math.Truncate((double)inst.Operand));
            }
            return -1;
        }
    }
}
