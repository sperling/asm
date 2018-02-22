using System;
using System.Collections.Generic;
using System.Text;
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
        {
            public abstract byte Op { get; }
            
            public abstract int Size(Parser parser);

            protected void RegNameIsValid(Token regNameToken)
            {
                if (_regNames.Contains(regNameToken.Text))
                {
                    return;
                }

                throw new Exception($"Bad register name {regNameToken.Text} on line {regNameToken.LineNumber}");
            }
        }

        private class Nop : OpCode
        {
            public override byte Op => 0x00;
            
            public override int Size(Parser parser)
            {
                return 1;
            }
        }

        private class Mov : OpCode
        {
            private byte _op = 0x01;

            public override byte Op => _op;

            public override int Size(Parser parser)
            {
                var dstRegToken = parser.ExpectTokenType(TokenType.Literal);
                RegNameIsValid(dstRegToken);

                parser.ExpectTokenType(TokenType.Comma);

                var srcRegOrNumberToken = parser.NextToken();
                
            }
        }

        private static Dictionary<string, OpCode> _opTable = new Dictionary<string, OpCode>(StringComparer.OrdinalIgnoreCase)
        {
            { nameof(Nop), new Nop() },
            { nameof(Mov), new Mov() }
        };

        private static HashSet<string> _regNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a"
        };
    }
}
