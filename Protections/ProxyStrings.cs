using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace LMM_Rewritten.Protections
{
    class Proxy
    {
        class ProxyStrings
        {
            public ProxyStrings(string str, MethodDef methods)
            {
                data = str;
                method = methods;
            }

            public string data;
            public MethodDef method;
        }

        class ProxyConstants
        {
            public ProxyConstants(int cnst, MethodDef methods)
            {
                constant = cnst;
                method = methods;
            }

            public int constant;
            public MethodDef method;
        }

        enum ProxyType
        {
            ProxyStrings,
            ProxyConstants,
            ProxyMethods
        }

        private static MethodDef[] GetProxyMethods()
        {
            List<ProxyStrings> ProxyStringMethods = new List<ProxyStrings>();
            List<ProxyConstants> ProxyConstantMethods = new List<ProxyConstants>();
            foreach (MethodDef methods in Program.asm.GlobalType.Methods)
            {
                if (!methods.HasBody) { continue; }
                if (methods.Name.Contains("ProxyMeth"))
                {
                    if (methods.Body.Instructions[0].OpCode == OpCodes.Ldstr &&
                        methods.Body.Instructions[1].OpCode == OpCodes.Ret)
                    {
                        ProxyStringMethods.Add(new ProxyStrings(methods.Body.Instructions[0].Operand.ToString(), methods));
                        Flags.ProxyStrings = true;
                        continue;
                    }
                    else if (methods.Body.Instructions[0].OpCode == OpCodes.Ldc_I4 &&
                             methods.Body.Instructions[1].OpCode == OpCodes.Ret)
                    {
                        ProxyConstantMethods.Add(new ProxyConstants(int.Parse(methods.Body.Instructions[0].Operand.ToString()), methods));
                        Flags.ProxyConst = true;
                        continue;
                    }
                    else
                    {

                        Flags.ProxyMethods = true;
                        continue;
                    }
                }
            }
            List<MethodDef> method = new List<MethodDef>();
            foreach (ProxyStrings prStrings in ProxyStringMethods)
            {
                method.Add(prStrings.method);
            }
            foreach (ProxyConstants prConst in ProxyConstantMethods)
            {
                method.Add(prConst.method);
            }
            return method.ToArray();
        }

        public static void Fix()
        {
            int pStringCount = 0, pConstCount = 0;
            MethodDef[] proxyMethods = GetProxyMethods();
            foreach (TypeDef type in Program.asm.Types)
            {
                foreach (MethodDef methods in type.Methods)
                {
                    if (!methods.HasBody) { continue; }
                    string methodName = methods.FullName;
                    if (methodName.Length > 35) { methodName = methodName.Substring(0, 34) + "..."; }
                    Program.SetStatusText("Fixing Proxies - " + methodName, ConsoleColor.White, ConsoleColor.DarkYellow);
                    for (int x = 0; x < methods.Body.Instructions.Count(); x++)
                    {
                        Instruction inst = methods.Body.Instructions[x];
                        if (inst.OpCode == OpCodes.Call)
                        {
                            if (inst.Operand is MethodDef)
                            {
                                if (proxyMethods.Contains((MethodDef)inst.Operand))
                                {
                                    MethodDef method = (MethodDef)inst.Operand;
                                    switch (method.Body.Instructions[0].OpCode.Code)
                                    {
                                        case Code.Ldstr:
                                            inst.OpCode = OpCodes.Ldstr;
                                            inst.Operand = method.Body.Instructions[0].Operand.ToString();
                                            pStringCount++;
                                            Flags.ProxyStrings = true;
                                            break;
                                        case Code.Ldc_I4:
                                            inst.OpCode = OpCodes.Ldc_I4;
                                            inst.Operand = int.Parse(method.Body.Instructions[0].Operand.ToString());
                                            pConstCount++;
                                            Flags.ProxyConst = true;
                                            break;
                                    }
                                    method.DeclaringType.Methods.Remove(method);
                                }
                            }
                        }
                    }
                }
            }
            ConsoleColor fore = ConsoleColor.White;
            ConsoleColor back = ConsoleColor.DarkGreen;
            if (pConstCount == 0) { fore = ConsoleColor.Black; back = ConsoleColor.DarkGray; }
            Program.AddPatchesLog("Pr.Const", "Fixed " + pConstCount + "!", ConsoleColor.DarkBlue, ConsoleColor.White, back, fore);
            if (pStringCount != 0) { fore = ConsoleColor.White; back = ConsoleColor.DarkGreen; } else { fore = ConsoleColor.Black; back = ConsoleColor.DarkGray; }
            Program.AddPatchesLog("Pr.String", "Fixed " + pStringCount + "!", ConsoleColor.DarkBlue, ConsoleColor.White, back, fore);
        }
    }
}
