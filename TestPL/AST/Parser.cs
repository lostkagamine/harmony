using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestPL.AST.Nodes;
using TestPL.Text;
using TestPL.Text.Tokens;

namespace TestPL.AST
{
    public class Parser
    {
        static readonly Dictionary<string, int> Precedence = new()
        {
            { "=", 1 },
            { "<", 7 },
            { ">", 7 },
            { "<=", 7 },
            { ">=", 7 },
            { "==", 7 },
            { "!=", 7 },
            { "+", 10 },
            { "-", 10 },
            { "*", 20 },
            { "/", 20 },
            { "%", 20 }
        };

        public Parser(Tokeniser t)
        {
            tk = t;
        }

        Tokeniser tk;
        Token current => tk.Peek();

        void SkipPunc(char punc)
        {
            if (current.Type != TokenType.Punctuation)
                throw tk.Die($"expected a '{punc}'");
            tk.Next();
        }

        bool IsPunc(char punc)
        {
            if (current.Type == TokenType.Punctuation)
            {
                var j = current;
                return j.Value == punc;
            }
            return false;
        }

        Token IsOperator(string op = null)
        {
            return
                (current.Type == TokenType.Operator && (op == null || current.Value == op) ? current : null);
        }

        void SkipOperator(string op = null)
        {
            if (IsOperator(op) != null) tk.Next();
            throw new Exception($"expecting operator \"{op}\"");
        }

        List<Node> Delimited(char start, char stop, char sep, Func<Node> parser)
        {
            var a = new List<Node>();
            var first = true;
            SkipPunc(start);
            while (!tk.Eof)
            {
                if (IsPunc(stop)) break;
                if (first) first = false;
                else SkipPunc(sep);
                if (IsPunc(stop)) break;
                a.Add(parser());
            }
            return a;
        }

        Node ConvertToPrimitive(Token t)
        {
            switch (t.Type)
            {
                case TokenType.String:
                    return new StringNode()
                    {
                        Value = t.Value
                    };
                case TokenType.Number:
                    return new NumberNode()
                    {
                        Value = t.Value
                    };
            }
            return null;
        }

        Node ParseAtom()
        {
            if (IsPunc('('))
            {
                tk.Next();
                var ex = ParseExpr();
                SkipPunc(')');
                return ex;
            }

            if (IsKeyword("true") || IsKeyword("false"))
                return ParseBoolean();

            var t = current;
            tk.Next();

            if (t.Type == TokenType.Variable |
                t.Type == TokenType.Number |
                t.Type == TokenType.String)
            {
                return ConvertToPrimitive(t);
            }

            throw tk.Die($"unexpected input");
        }

        Node ParseExpr()
        {
            return MaybeBinary(ParseAtom(), 0);
        }

        Node MaybeBinary(Node left, int myprecedence)
        {
            var t = IsOperator();
            if (t != null)
            {
                var prec = Precedence[t.Value];
                if (prec > myprecedence)
                {
                    tk.Next();

                    Node inp;
                    if (t.Value == "=")
                    {
                        inp = new AssignmentNode()
                        {
                            Operator = t.Value,
                            Left = left,
                            Right = MaybeBinary(ParseAtom(), prec)
                        };
                    } else
                    {
                        inp = new BinaryNode()
                        {
                            Operator = t.Value,
                            Left = left,
                            Right = MaybeBinary(ParseAtom(), prec)
                        };
                    }

                    return MaybeBinary(inp, myprecedence);
                }
            }

            return left;
        }

        private bool IsKeyword(string v)
        {
            if (current.Type == TokenType.Keyword)
                return current.Value == v;
            return false; // Tokens that are not keyword tokens cannot be keywords for obvious reasons
        }

        Node ParseBoolean()
        {
            if (IsKeyword("true"))
                return new BooleanNode()
                {
                    Value = true
                };
            else return new BooleanNode()
                {
                    Value = false
                };
        }

        public Node ParseTopLevel()
        {
            var prog = new List<Node>();
            do
            {
                prog.Add(ParseExpr());
                if (!tk.Eof) SkipPunc(';');
            } while (!tk.Eof);
            return new ProcedureNode()
            {
                Body = prog
            };
        }
    }
}
