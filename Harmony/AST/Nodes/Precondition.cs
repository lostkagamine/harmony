using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.AST.Nodes
{
    public enum PreconditionType
    {
        Pre,
        Post
    }

    public class Precondition
    {
        public PreconditionType Type = PreconditionType.Pre;
        public LambdaNode Condition;
    }
}