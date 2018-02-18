using Asm.Assembler.Parsing;
using System;
using System.IO;

namespace Asm.Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var f = File.OpenText(args[0]))
            {
                var parser = new Parser();
                parser.Init(f, args[0]);
            }
        }
    }
}
