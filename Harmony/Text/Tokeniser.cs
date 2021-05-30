using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Harmony.Text.Tokens;

namespace Harmony.Text
{
    public static class Re
    {
        public static bool Match(string pat, char c)
            => new Regex(pat).IsMatch(c.ToString());

        public static bool Match(string pat, string c)
            => new Regex(pat).IsMatch(c);

        public static bool MatchC(string pat, char c)
            => new Regex(pat, RegexOptions.IgnoreCase).IsMatch(c.ToString());

        public static bool MatchC(string pat, string c)
            => new Regex(pat, RegexOptions.IgnoreCase).IsMatch(c);
    }

    public class Tokeniser
    {
        public StreamReaderWrapper Input;

        public string[] Keywords =
        {
            "if",
            "then",
            "else",
            "true",
            "false",
            "function",
            "do",
            "end",
            "extern",
            "object"
        };

        public Tokeniser(StreamReaderWrapper s)
        {
            Input = s;
        }

        bool IsIdStart(char c)
            => Re.MatchC(@"[a-z_]", c);
        bool IsId(char c)
            => IsIdStart(c) || "1234567890!?.".Contains(c);
        bool IsPunc(char c)
            => ",;${}()[]{}=<->!+-/*".Contains(c);
        bool IsNumberStart(char c)
            => "1234567890".Contains(c);
        bool IsNumber(char c)
            => IsNumberStart(c) || c == '.';

        public Exception Die(string msg) => Input.Die(msg);

        public bool Eof = false;

        void SkipLine()
        {
            Input.ReadUntil(c => c != '\n');
            Input.Next();
        }

        string ReadEscaped(char endchar)
        {
            var esc = false;
            var o = "";

            Input.Next();
            while (!Input.Eof)
            {
                var c = Input.Next();
                if (esc)
                {
                    o += c;
                    esc = false;
                } else if (c == '\\')
                {
                    esc = true;
                } else if (c == endchar)
                {
                    break;
                } else
                {
                    o += c;
                }
            }

            return o;
        }

        Token ReadString()
        {
            var o = ReadEscaped('"');
            return new Token()
            {
                Type = TokenType.String,
                Value = o
            };
        }

        Token ReadIdent()
        {
            var id = Input.ReadWhile(IsId);
            Token o = new Token()
            {
                Type = TokenType.Identifier,
                Value = id
            };
            if (Keywords.Contains(id))
            {
                o = new Token()
                {
                    Type = TokenType.Keyword,
                    Value = id
                };
            }
            return o;
        }

        Token ReadPunc()
        {
            return new Token()
            {
                Type = TokenType.Punctuation,
                Value = Input.Next()
            };
        }

        Token ReadNumber()
        {
            var o = "";
            char r;
            do
            {
                r = Input.Next();
                if (!IsNumber(r))
                    break;
                o += r;
            } while (IsNumber(r));
            var val = double.Parse(o);
            return new Token()
            {
                Type = TokenType.Number,
                Value = val
            };
        }

        Token CurrentToken;

        public Token ReadNext()
        {
            Input.ReadWhile(c => "\r\n\t ".Contains(c));

            if (Input.Eof || Input.Peek() == '\0')
            {
                Eof = true;
                return new Token()
                {
                    Type = TokenType.Eof,
                    Value = null
                };
            }

            var c = Input.Peek();
            if (c == '#')
            {
                SkipLine();
                return ReadNext();
            }

            if (c == '"')
                return ReadString();

            if (IsIdStart(c))
                return ReadIdent();

            if (IsPunc(c))
                return ReadPunc();

            if (IsNumberStart(c))
                return ReadNumber();

            throw Die($"unexpected character '{c}'");
        }

        public Token Peek()
        {
            if (CurrentToken == null)
                CurrentToken = ReadNext();
            return CurrentToken;
        }

        public Token Next()
        {
            if (CurrentToken != null)
            {
                var t = CurrentToken;
                CurrentToken = null;
                return t;
            }
            return ReadNext();
        }
    }
}
