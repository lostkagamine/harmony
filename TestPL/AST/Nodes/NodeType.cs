using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPL.AST.Nodes
{
    public enum NodeType
    {
        Procedure,
        Expression,
        Number,
        String,
        Boolean,
        Variable,
        If,
        Assignment,
        Binary
    }
}
