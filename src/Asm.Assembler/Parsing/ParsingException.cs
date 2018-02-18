using System;

namespace Asm.Assembler.Parsing
{
    public class ParsingException : Exception
    {
        public ParsingException(string filePath, int lineNumber, string error) : base($"{filePath}, line: {lineNumber}, {error}")
        {

        }

        public ParsingException(int lineNumber, string error) : base($"line: {lineNumber}, {error}")
        {

        }
    }
}
