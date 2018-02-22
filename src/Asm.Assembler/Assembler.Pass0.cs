using Asm.Assembler.Parsing;
using System;

namespace Asm.Assembler
{
    public partial class Assembler
    {
		private void Pass0(Parser parser)
        {
            while (true)
            {
                var token = parser.NextToken();
                if (token.Type == TokenType.EOF)
                {
                    break;
                }

                if (token.Type == TokenType.Literal)
                {
                    if ("org".Equals(token.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        ParseOrg(parser);
                    }
                    else
                    {
                        var nextToken = parser.NextToken();
                        if (nextToken.Type == TokenType.Colon)
                        {
                            ParseLabel(token);
                        }
                        else
                        {
                            var opToken = parser.ExpectTokenType(TokenType.Literal);
                            // TODO:    equ, rep.
                            if (_opTable.TryGetValue(nextToken.Text, out var opCode))
                            {
                                _locationCounter += opCode.Size(parser);
                            }
                            else 
                            {
                                throw new Exception($"Undefined op code {opToken.Text}");
                            }
                        }
                    }
                }
            }
        }

        private void ParseLabel(Token symbolToken)
        {
            if (_symbolTable.TryGetValue(symbolToken.Text, out var existingInfo))
            {
                throw new Exception($"Symbol {symbolToken.Text} on line {symbolToken.LineNumber} is already defined at line {existingInfo.lineNumber}");
            }

            _symbolTable.Add(symbolToken.Text, (address: _locationCounter, lineNumber: symbolToken.LineNumber));
        }

        private void ParseOrg(Parser parser)
        {
            var numberToken = parser.NextToken();
            var org = numberToken.ToNumber();
            if (org < 0 || org > ushort.MaxValue)
            {
                throw new Exception($"org is out of range, line: {numberToken.LineNumber}");
            }

            _locationCounter = org;
        }
    }
}
