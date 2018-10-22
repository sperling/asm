using Asm.Assembler.Parsing;
using System;
using System.Collections.Generic;

namespace Asm.Assembler
{
    public partial class Assembler
    {
		private List<OpCode> Pass0(Parser parser)
        {
            var opCodes = new List<OpCode>();
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
                            if (_opTableFactory.TryGetValue(nextToken.Text, out var opCodeFactory))
                            {
                                var opCode = opCodeFactory();
<<<<<<< HEAD
                                opCodes.Add(opCode);
                                _locationCounter += opCode.Pass0(parser);
=======
                                _locationCounter += opCode.Pass0(parser);
                                _opCodes.Add(opCode);
>>>>>>> dfb58aa3c35eb44148f0677e083d3704225f04e2
                            }
                            else 
                            {
                                throw new Exception($"Undefined op code {opToken.Text}");
                            }
                        }
                    }
                }
            }

            return opCodes;
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
