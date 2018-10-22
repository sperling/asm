using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Asm.Assembler.Parsing
{
    public class Parser
    {
        private const string IncludeToken = "#include";
        private const string SingleLineComment = "//";
        private const string MultiLinesCommentStart = "/*";
        private const string MultiLinesCommentEnd = "*/";

        private string _buffer;
        private int _currentPosition;
        private int _currentLineNumber;
        private string _filePath;

        private readonly Dictionary<char, TokenType> _punctuation = new Dictionary<char, TokenType>
        {
            { '"', TokenType.Quote },
            { '<', TokenType.LessThan },
            { '>', TokenType.GreaterThan },
            { ',', TokenType.Comma },
            { ':', TokenType.Colon },
            { '[', TokenType.LeftBracket },
            { ']', TokenType.RightBracket }
        };

        public void Init(StreamReader reader, string filePath)
        {
            var visitedIncludedFilePaths = new Dictionary<string, (string fromFilePath, int fromLineNumber)>();
            filePath = Path.GetFullPath(filePath);
            var buffer = PreProcess(reader, filePath, visitedIncludedFilePaths);
            Reset(buffer, filePath, 1);            
        }

        public void Reset()
        {
            Reset(_buffer, _filePath, _currentLineNumber);
        }

        public Token NextToken()
        {
            var sb = new StringBuilder();
            var type = TokenType.None;
            while (true)
            {
                if (ConsumeWhitespace())
                {
                    if (_currentPosition >= _buffer.Length)
                    {
                        return new Token(null, TokenType.EOF, _currentLineNumber);
                    }
                    break;
                }

                var c = _buffer[_currentPosition];
                if (_punctuation.TryGetValue(c, out var punctuationType))
                {
                    if (sb.Length > 0)
                    {
                        // don't include single punctuation chars in token.
                        break;
                    }

                    // single punctuation chars as token.
                    type = punctuationType;
                    sb.Append(c);
                    _currentPosition++;
                    break;
                }

                // intel like syntax, http://www.imada.sdu.dk/Courses/DM18/Litteratur/IntelnATT.htm
                // if hex number starts with a char, then need to prefix with 0.
                // ex. 123h
                //     0ffh
                //     111001b
                if (type == TokenType.None)
                {
                    if (char.IsDigit(c))
                    {
                        type = TokenType.Number;
                    }
                    else
                    {
                        type = TokenType.Literal;
                    }
                }

                sb.Append(c);
                _currentPosition++;                
            }

            return new Token(sb.ToString(), type, _currentLineNumber);
        }

        public void ExpectToken(string expectedToken, TokenType expectedType)
        {
            var token = NextToken();
            if (token.Type != expectedType || token.Text != expectedToken)
            {
                throw new ParsingException(_filePath, _currentLineNumber, $"Expected {expectedToken}");
            }
        }

        public Token ExpectTokenType(TokenType expectedType)
        {
            var token = NextToken();
            if (token.Type != expectedType)
            {
                throw new ParsingException(_filePath, _currentLineNumber, $"Expected {expectedType}");
            }

            return token;
        }

        private string PreProcess(StreamReader reader, string filePath, Dictionary<string, (string fromFilePath, int fromLineNumber)> visitedIncludedFilePaths)
        {
            var sb = new StringBuilder((int)reader.BaseStream.Length);
            var lineNumber = 1;
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                {                    
                    break;
                }

                var trimmedLine = line.TrimStart();
                if (trimmedLine.StartsWith(IncludeToken))
                {
                    string includeContent = LoadInclude(trimmedLine, lineNumber, filePath, visitedIncludedFilePaths);
                    sb.Append(includeContent);
                    lineNumber++;
                    continue;
                }
                if (trimmedLine.StartsWith(SingleLineComment))
                {
                    lineNumber++;
                    continue;
                }
                if (trimmedLine.StartsWith(MultiLinesCommentStart))
                {
                    if (trimmedLine.EndsWith(MultiLinesCommentEnd))
                    {
                        lineNumber++;
                        continue;
                    }

                    var lastLineHit = false;
                    var multiCommentStartLine = lineNumber;
                    while (true)
                    {
                        line = reader.ReadLine();
                        if (line == null)
                        {
                            throw new ParsingException(filePath, multiCommentStartLine, "Missing end of multi lines comment");
                        }
                        lineNumber++;
                        if (line.EndsWith(MultiLinesCommentEnd))
                        {
                            line = reader.ReadLine();
                            if (line == null)
                            {
                                lastLineHit = true;
                            }
                            break;
                        }
                    }
                    if (lastLineHit)
                    {
                        // compensate for that lineNumber++ is not executed in main loop body.
                        lineNumber++;
                        break;
                    }
                }
                
                sb.AppendLine(line);
                lineNumber++;
            }

            return sb.ToString();
        }

        private string LoadInclude(string line, int lineNumber, string filePath, Dictionary<string, (string fromFilePath, int fromLineNumber)> visitedIncludedFilePaths)
        {
            Reset(line, filePath, lineNumber);
            ExpectToken(IncludeToken, TokenType.Literal);
            var searchToken = ExpectEitherToken("<", "\"");
            var includedFileNameToken = NextToken();
            if (includedFileNameToken.Type != TokenType.Literal)
            {
                // TODO:    error
                throw new ParsingException(filePath, lineNumber, $"");
            }
            ExpectToken(searchToken.Text, searchToken.Type == TokenType.LessThan ? TokenType.GreaterThan : TokenType.Quote);

            var basePath = Environment.CurrentDirectory;
            var includedFilePath = "";
            if (searchToken.Type == TokenType.Quote)
            {
                // first check if includeFileName exists in same directory as fileName.
                // if not there, fallback to "standard" search paths.
                includedFilePath = Path.Combine(Path.GetDirectoryName(filePath), includedFileNameToken.Text);
                if (!File.Exists(includedFilePath))
                {
                    includedFilePath = FileExistsInStandardSearchPaths(includedFileNameToken.Text, basePath);
                }
            }
            else
            {
                includedFilePath = FileExistsInStandardSearchPaths(includedFileNameToken.Text, basePath);
            }

            var fullPath = Path.GetFullPath(includedFilePath);
            if (visitedIncludedFilePaths.TryGetValue(fullPath, out var fromFilePathAndLineNumber))
            {
                throw new ParsingException(filePath, lineNumber, $"{fullPath} already included from {fromFilePathAndLineNumber.fromFilePath}, {fromFilePathAndLineNumber.fromLineNumber}");
            }

            visitedIncludedFilePaths.Add(fullPath, (fromFilePath: filePath, fromLineNumber: lineNumber));
            using (var f = File.OpenText(includedFilePath))
            {
                var parser = new Parser();
                return parser.PreProcess(f, includedFilePath, visitedIncludedFilePaths);                
            }
        }

        private string FileExistsInStandardSearchPaths(string includedFileName, string basePath)
        {
            string includedFilePath = Path.Combine(basePath, includedFileName);
            if (!File.Exists(includedFilePath))
            {
                throw new ParsingException(_filePath, _currentLineNumber, $"Can't locate file {includedFileName}");
            }

            return includedFilePath;
        }
        
        private Token ExpectEitherToken(params string[] expectedTokens)
        {
            var token = NextToken();
            foreach (var expectedToken in expectedTokens)
            {
                if (token.Text == expectedToken)
                {
                    return token;
                }
            }

            throw new ParsingException(_filePath, _currentLineNumber, $"Expected one of the following {string.Join(" ", expectedTokens)}");
        }

        private bool ConsumeWhitespace()
        {
            bool anyWhitespace = false;

            while (_currentPosition < _buffer.Length)
            {
                var c = _buffer[_currentPosition];
                if (c == '\n')
                {
                    _currentLineNumber++;
                    _currentPosition++;
                    anyWhitespace |= true;
                    continue;
                }

                if (char.IsWhiteSpace(c))
                {
                    _currentPosition++;
                    anyWhitespace |= true;
                    continue;
                }

                break;
            }

            return anyWhitespace;
        }
        
        private void Reset(string buffer, string filePath, int lineNumber)
        {
            _buffer = buffer;
            _currentPosition = 0;
            _filePath = filePath;
            _currentLineNumber = lineNumber;
        }
    }
}
