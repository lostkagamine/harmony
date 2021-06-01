using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony.AST.Nodes;
using Harmony.Text;
using Harmony.Text.Tokens;

namespace Harmony.AST
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
            { "%", 20 },
            { "^", 30 }
        };

        public Parser(Tokeniser t)
        {
            tk = t;
        }

        Tokeniser tk;
        Token current => tk.Peek();

        void SkipPunc(char punc)
        {
            if (current.Type != TokenType.Punctuation || current.Value != punc)
                throw tk.Die($"expected a '{punc}'");
            tk.Next();
        }

        void SkipPuncOptional(char punc)
        {
            if (current.Type == TokenType.Punctuation
                && current.Value == punc)
                tk.Next();
        }

        void SkipKw(string kw)
        {
            if (current.Type != TokenType.Keyword || current.Value != kw)
                throw tk.Die($"expected a '{kw}' here");
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
            do
            {
                if (IsPunc(stop)) break;
                if (first) first = false;
                else SkipPunc(sep);
                if (IsPunc(stop)) break;
                a.Add(parser());
            } while (!tk.Eof);
            SkipPunc(stop);
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

        Node ParseBlock()
        {
            var t = new List<Node>();
            var receivedEnd = false;
            do
            {
                var r = ParseExpr();
                if (r.Type == NodeType.End)
                {
                    receivedEnd = true;
                    break;
                }
                t.Add(r);
                if (!tk.Eof) SkipPuncOptional(';');
            } while (!tk.Eof);

            if (!receivedEnd)
                throw tk.Die("expected 'end'");

            return new ProcedureNode()
            {
                Body = t
            };
        }

        bool inIf = false;

        Node ParseIf()
        {
            SkipKw("if");
            var condition = ParseExpr();
            SkipKw("then");
            inIf = true;
            var then = ParseBlock();
            var next = tk.Peek();
            if (next.Type == TokenType.Keyword && next.Value == "else")
            {
                SkipKw("else");
                var elseb = ParseBlock();
                return new IfNode()
                {
                    Condition = condition,
                    Then = then,
                    Else = elseb
                };
            }
            return new IfNode()
            {
                Condition = condition,
                Then = then,
                Else = null
            };
        }

        Node ParseCall(Node c)
        {
            var vals = Delimited('(', ')', ',', ParseExpr);
            return new CallNode()
            {
                Left = c,
                Arguments = vals
            };
        }

        Node ParseIdent(Token c)
        {
            return new IdentifierNode()
            {
                Value = c.Value
            };
        }

        Node ParseAssignment(Token c)
        {
            tk.Next();
            var n = new AssignmentNode()
            {
                Left = ParseIdent(c),
                Operator = "=",
                Right = ParseExpr()
            };
            return n;
        }

        Node IdentAndSkip()
        {
            var n = current;
            tk.Next();
            return ParseIdent(n);
        }

        Node ParseFunction()
        {
            SkipKw("function");
            var id = ParseIdent(current);
            tk.Next();
            var args = Delimited('(', ')', ',', IdentAndSkip);
            SkipKw("do");
            var bd = ParseBlock();
            return new FunctionNode()
            {
                Name = ((IdentifierNode)id).Value,
                Body = (ProcedureNode)bd,
                Arguments = args.Select(e => ((IdentifierNode)e).Value).ToList()
            };
        }

        Node MaybeCall(Node c)
        {
            if (current.Type == TokenType.Punctuation && current.Value == '(')
            {
                return ParseCall(c);
            } else
            {
                return c;
            }
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

            if (IsKeyword("if"))
                return ParseIf();

            if (IsKeyword("end") || (inIf && IsKeyword("else")))
            {
                if (inIf)
                {
                    inIf = false;
                }
                if (!IsKeyword("else")) tk.Next();
                return new EndNode();
            }

            if (IsKeyword("function"))
                return ParseFunction();

            if (IsKeyword("extern"))
            {
                tk.Next();
                var id = tk.Next();
                var types = Delimited('(', ')', ',', IdentAndSkip);
                return new ExternNode()
                {
                    Value = id.Value,
                    Types = types
                };
            }

            var nt = tk.Next();

            if (nt.Type == TokenType.Identifier)
            {
                var id = tk.Peek();
                if (id.Type == TokenType.Punctuation && id.Value == '=')
                {
                    return ParseAssignment(nt);
                }
                return new IdentifierNode()
                {
                    Value = nt.Value
                };
            }

            if (nt.Type == TokenType.Variable |
                nt.Type == TokenType.Number |
                nt.Type == TokenType.String)
            {
                return ConvertToPrimitive(nt);
            }

            throw tk.Die($"unexpected input: {nt.Type} '{nt.Value}'");
        }

        Node ParseExpr()
        {
            return MaybeCall(MaybeBinary(ParseAtom(), 0));
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
            var t = current;
            tk.Next();
            if (t.Type == TokenType.Keyword && t.Value == "true")
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
                if (!tk.Eof) SkipPuncOptional(';');
            } while (!tk.Eof);
            return new ProcedureNode()
            {
                Body = prog
            };
        }
    }
}
