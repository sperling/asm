using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Asm.Assembler.Parsing
{
    public class Token
    {
        public string Text { get; }

        public TokenType Type { get; }

        public int LineNumber { get; }

        public Token(string text, TokenType type, int lineNumber)
        {
            Text = text;
            Type = type;
            LineNumber = lineNumber;
        }

        public int ToNumber()
        {
            if (Type != TokenType.Number)
            {
                throw new ParsingException(LineNumber, "Expected number");
            }

            if (Text.IndexOf("h", StringComparison.OrdinalIgnoreCase) != -1)
            {
                if (!int.TryParse(Text.Replace("h", "").Replace("H", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hex))
                {
                    throw new ParsingException(LineNumber, "Can't parse hex number");
                }

                return hex;
            }

            if (Text.IndexOf("b", StringComparison.OrdinalIgnoreCase) != -1)
            {
                try
                {
                    var bin = Convert.ToInt32(Text.Replace("b", "").Replace("B", ""), 2);
                    return bin;
                }
                catch
                {
                    throw new ParsingException(LineNumber, "Can't parse binary number");
                }                
            }

            if (!int.TryParse(Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
            {
                throw new ParsingException(LineNumber, "Can't parse decimal number");
            }

            return dec;
        }

        public NumberType GetNumber(out int number)
        {
            number = ToNumber();
            if (number >= 0 && number <= 15)
            {
                return NumberType.Nibble;
            }
            else if (number >= byte.MinValue && number <= byte.MaxValue)
            {
                return NumberType.Byte;
            }
            else if (number >= ushort.MinValue && number <= ushort.MaxValue)
            {
                return NumberType.Short;
            }

            return NumberType.Int;
        }
    }

    public enum TokenType
    {
        None = 0,
        Literal,
        Number,
        Quote,
        LessThan,
        GreaterThan,
        Comma,
        Colon,
        LeftBracket,
        RightBracket,
        EOF
    }

    public enum NumberType
    {
        Nibble = 0,
        Byte,
        Short,
        Int
    }
}
