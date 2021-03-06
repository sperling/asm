﻿using Asm.Assembler.Parsing;
using System.Collections.Generic;
using System.IO;

namespace Asm.Assembler
{
    // https://www.slideshare.net/ShubhamShah001/two-pass-assembler
    public partial class Assembler
    {
        private int _locationCounter;

        private readonly Dictionary<string, (int address, int lineNumber)> _symbolTable = new Dictionary<string, (int address, int lineNumber)>();

        public void Generate(string sourcePath, string destinationPath)
        {
            List<OpCode> opCodes = null;
            
            using (var source = File.OpenText(sourcePath))
            {
                _locationCounter = 0;
                _opCodes.Clear();

                var parser = new Parser();
                parser.Init(source, sourcePath);

                opCodes = Pass0(parser);
                //parser.Reset();

                if (_locationCounter > ushort.MaxValue)
                {
                    throw new System.Exception("Program size larger than max size");
                }
            }

            Pass1(destinationPath, opCodes);
        }

        
    }
}
