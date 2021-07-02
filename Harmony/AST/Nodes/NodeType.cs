using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.AST.Nodes
{
    public enum NodeType
    {
        Procedure,
        Expression,
        Number,
        String,
        Boolean,
        Identifier,
        If,
        Assignment,
        Binary,
        Call,
        Function,
        End,
        Extern,
        Return,
        Throw,
        Lambda,
        Override
    }
}
