using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Asm.Assembler.Parsing;

namespace Asm.Assembler
{
    public partial class Assembler
    {
		/*
            #define HLT 0b10000000000000000000000000000000  // Halt clock
            #define MI  0b01000000000000000000000000000000  // Memory address register in
            #define RI  0b00100000000000000000000000000000  // RAM data in
            #define RO  0b00010000000000000000000000000000  // RAM data out
            #define IO  0b00001000000000000000000000000000  // Instruction register out
            #define II  0b00000100000000000000000000000000  // Instruction register in
            #define AI  0b00000010000000000000000000000000  // A register in
            #define AO  0b00000001000000000000000000000000  // A register out
            #define EO  0b00000000100000000000000000000000  // ALU out
            #define SU  0b00000000010000000000000000000000  // ALU subtract
            #define BI  0b00000000001000000000000000000000  // B register in
            #define OI  0b00000000000100000000000000000000  // Output register in
            #define CE  0b00000000000010000000000000000000  // Program counter enable
            #define CO  0b00000000000001000000000000000000  // Program counter out
            #define J   0b00000000000000100000000000000000  // Jump (program counter in)
            #define JC  0b00000000000000010000000000000000  // Jump if carry (program counter in)
            #define IIO 0b00000000000000001000000000000000  // Intermediate out
            #define III 0b00000000000000000100000000000000  // Intermediate in
            #define ORS 0b00000000000000000010000000000000  // Output register select
            #define OE  0b00000000000000000001000000000000  // Output enable
            
            #define FETCH0 MI|CO
            #define FETCH1 RO|II|CE
            
            unsigned long data[] = {
              FETCH0,  FETCH1,  0,      0,          0,         0, 0, 0,   // 0000 - NOP
              FETCH0,  FETCH1,  IO|MI,  RO|AI,      0,         0, 0, 0,   // 0001 - LDA
              FETCH0,  FETCH1,  IO|MI,  RO|BI,      EO|AI,     0, 0, 0,   // 0010 - ADD
              FETCH0,  FETCH1,  IO|MI,  RO|BI,      EO|AI|SU,  0, 0, 0,   // 0011 - SUB
              FETCH0,  FETCH1,  IO|MI,  AO|RI,      0,         0, 0, 0,   // 0100 - STA
              FETCH0,  FETCH1,  IO|AI,  0,          0,         0, 0, 0,   // 0101 - LDI
              FETCH0,  FETCH1,  IO|J,   0,          0,         0, 0, 0,   // 0110 - JMP
              FETCH0,  FETCH1,  IO|JC,  0,          0,         0, 0, 0,   // 0111 - JC
              FETCH0,  FETCH1,  AO|MI,  RO|BI,      0,         0, 0, 0,   // 1000 - LDAB
              FETCH0,  FETCH1,  EO|AI,  0,          0,         0, 0, 0,   // 1001 - ADDA
              FETCH0,  FETCH1,  FETCH0, RO|III|CE,  IIO|AI,    0, 0, 0,   // 1010 - LDII
              FETCH0,  FETCH1,  FETCH0, RO|III|CE,  IIO|ORS|OE,0, 0, 0,   // 1011 - OUT COMMAND
              FETCH0,  FETCH1,  FETCH0, RO|III|CE,  IIO|OE,    0, 0, 0,   // 1100 - OUT DATA
              FETCH0,  FETCH1,  0,      0,          0,         0, 0, 0,   // 1101
              FETCH0,  FETCH1,  AO|OI,  0,          0,         0, 0, 0,   // 1110 - OUT
              FETCH0,  FETCH1,  HLT,    0,          0,         0, 0, 0,   // 1111 - HLT
            };
        */

        private abstract class OpCode
<<<<<<< HEAD
        {            
            public abstract int Pass0(Parser parser);

            public abstract void Pass1(Stream stream);
=======
        {
            public abstract List<byte> Ops { get; }
            
            public abstract int Pass0(Parser parser);
>>>>>>> dfb58aa3c35eb44148f0677e083d3704225f04e2

            protected void ExpectRegName(Token regNameToken)
            {
                if (_regNames.Contains(regNameToken.Text))
                {
                    return;
                }

                throw new Exception($"Bad register name {regNameToken.Text} on line {regNameToken.LineNumber}");
            }

            protected bool TokenIsRegName(Token regNameToken) => _regNames.Contains(regNameToken.Text);
        }

        // nop
        private class Nop : OpCode
<<<<<<< HEAD
        {            
=======
        {
            private static readonly List<byte> _ops = new List<byte> { 0x00 };

            public override List<byte> Ops => _ops;
            
>>>>>>> dfb58aa3c35eb44148f0677e083d3704225f04e2
            public override int Pass0(Parser parser)
            {
                return 1;
            }

            public override void Pass1(Stream stream)
            {
                stream.Emit8(0x00);
            }
        }

<<<<<<< HEAD
        // ldsp absolute
        // ldsp const symbol
        private class Ldsp : OpCode
        {
            private int _absolute;
            private Token _symbol;

            public override int Pass0(Parser parser)
            {
                var token = parser.NextToken();
                if (token.Type == TokenType.Number)
                {
                    _absolute = token.ToNumber();
                }
                /*else if (token.Type == const symbol) 
                {

                }*/
                else 
                {
                    throw new Exception("TODO: should be number or symbol");
                }
                return 3;
            }
=======
        // num256 can also be a symbol.
        // mov a, b                 -- move from b reg to a reg
        // mov a, [b]               -- move from memory address in b reg to a reg
        // mov a, num256            -- move constant value to a reg
        // mov a, [num256]          -- move memory address constant value to a reg
        // mov [a], b               -- move from b reg to memory address in a reg
        // mov [a], num256          -- move constant to memory address in a reg
        // mov [a], [num256]        -- move memory address constant value to memory address in a reg
        // mov [a], [b]             -- move memory address in b reg to memory address in a reg
        // mov [num256], b          -- move from b reg to memory address constant value
        // mov [num256], num256     -- move constant to memory address constant value
        // mov [num256], [num256]   -- move memory address constant value to memory address constant value
        // mov [num256], [b]        -- move memory address in b reg to memory address constant value
        private class Mov : OpCode
        {
            private readonly List<byte> _ops = new List<byte>();

            private Token _dstToken;
            private bool _dstTokenIsSymbol;
            private int? _dstConstant;
            private Token _srcToken;
            private bool _srcTokenIsSymbol;
            private int? _srcConstant;

            public override List<byte> Ops => _ops;

            public override int Pass0(Parser parser)
            {
                _dstToken = parser.NextToken();
                if (_dstToken.Type == TokenType.LeftBracket)
                {
                    _dstToken = parser.NextToken();
                    parser.ExpectTokenType(TokenType.RightBracket);
                }
                parser.ExpectTokenType(TokenType.Comma);

                _srcToken = parser.NextToken();

                if (_dstToken.Type == TokenType.Literal)
                {
                    // must be a reg.
                    ExpectRegName(_dstToken);

                    _dstTokenIsSymbol = true;

                    // mov reg, number256
                    // mov [reg], number256
                    // mov [number256], number256
                    if (_srcToken.Type == TokenType.Number)
                    {
                        if (_dstToken.Type != TokenType.Literal && _dstToken.Type != TokenType.Number)
                        {
                            throw new Exception($"Expected token {_dstToken.Text}")
                        }
                        var srcNumberType = _srcToken.GetNumber(out var srcConstant);
                        _srcConstant = srcConstant;
                        if (srcNumberType == NumberType.Nibble)
                        {
                            return 1;
                        }
                        else if (srcNumberType == NumberType.Byte)
                        {
                            return 2;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                }

                /*bool dstIsReg = false;
                var dstRegOrLeftBracketToken = parser.NextToken();
                if (dstRegOrLeftBracketToken.Type == TokenType.Literal)
                {
                    dstIsReg = true;
                    RegNameIsValid(dstRegOrLeftBracketToken);
                }
                else if (dstRegOrLeftBracketToken.Type == TokenType.LeftBracket)
                {
                    var dstRegOrNumberToken = parser.NextToken();
                    if (dstRegOrNumberToken.Type == TokenType.Literal)
                    {
                        throw new NotImplementedException();
                    }
                    else if (dstRegOrNumberToken.Type == TokenType.Number)
                    {
                        throw new NotImplementedException();                    
                    }
                    else
                    {
                        throw new Exception($"Unexpected token {dstRegOrLeftBracketToken.Text} at line {dstRegOrLeftBracketToken.LineNumber}");
                    }

                    parser.ExpectTokenType(TokenType.RightBracket);
                }
                else
                {
                    throw new Exception($"Unexpected token {dstRegOrLeftBracketToken.Text} at line {dstRegOrLeftBracketToken.LineNumber}");
                }
>>>>>>> dfb58aa3c35eb44148f0677e083d3704225f04e2

            public override void Pass1(Stream stream)
            {
                stream.Emit8(0x01);

<<<<<<< HEAD
                if (_symbol != null)
                {
                    // TODO:    
                }
                else 
                {
                    stream.Emit16(_absolute);
                }
            }
=======
                var srcRegOrNumberOrLeftBracketToken = parser.NextToken();
                if (srcRegOrNumberOrLeftBracketToken.Type == TokenType.Number)
                {
                    var srcConstant = srcRegOrNumberOrLeftBracketToken.ToNumber();
                    if (srcConstant >= 0 && srcConstant <= 15)
                    {
                        if (dstIsReg)
                        {
                            _ops.Add((byte)((0x05 << 4) | (srcConstant & 7)));
                            return 1;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else if ((srcConstant >= 0 && srcConstant <= 255) || (srcConstant >= -128 && srcConstant <= 127))
                    {
                        if (dstIsReg)
                        {
                            _ops.Add((byte)((0x0a << 4) | (srcConstant & 127)));
                            return 2;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }*/
            }            
>>>>>>> dfb58aa3c35eb44148f0677e083d3704225f04e2
        }

        private static Dictionary<string, Func<OpCode>> _opTableFactory = new Dictionary<string, Func<OpCode>>(StringComparer.OrdinalIgnoreCase)
        {
            { nameof(Nop), () => new Nop() },
<<<<<<< HEAD
            { nameof(Ldsp), () => new Ldsp() }
=======
            { nameof(Mov), () => new Mov() }
>>>>>>> dfb58aa3c35eb44148f0677e083d3704225f04e2
        };

        private static HashSet<string> _regNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a"
        };

        private readonly List<OpCode> _opCodes = new List<OpCode>();
    }
}
