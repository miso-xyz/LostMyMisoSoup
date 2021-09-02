using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace LMM_Rewritten
{
    class RngSeed
    {
        public RngSeed(MethodDef methods, int Seed, Instruction Inst)
        {
            method = methods;
            seed = Seed;
            inst = Inst;
        }

        public MethodDef method;
        public int seed;
        public Instruction inst;
    }
}
