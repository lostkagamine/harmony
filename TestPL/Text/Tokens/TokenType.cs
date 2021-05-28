using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPL.Text.Tokens
{
    public enum TokenType
    {
        Number,
        String,
        Variable,
        Identifier,
        Equals,
        Punctuation,
        Operand,
        Operator,
        Keyword
    }
}
