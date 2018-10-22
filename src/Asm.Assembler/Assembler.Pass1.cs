using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Asm.Assembler
{
    public partial class Assembler
    {
        private void Pass1(string destionationPath, List<OpCode> opCodes)
        {
            using (var file = File.OpenWrite(destionationPath))
            {
                foreach (var opCode in opCodes)
                {
                    opCode.Pass1(file);
                }
            }
        }
    }
}
