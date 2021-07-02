using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.AST.Nodes
{
    public class Node
    {
        public NodeType Type;
    }

    public class IdentifierNode : Node
    {
        public IdentifierNode()
        {
            Type = NodeType.Identifier;
        }

        public string Value;
    }

    public class ProcedureNode : Node
    {
        public ProcedureNode()
        {
            Type = NodeType.Procedure;
        }

        public List<Node> Body;
    }

    public class StringNode : Node
    {
        public StringNode()
        {
            Type = NodeType.String;
        }

        public string Value;
    }

    public class NumberNode : Node
    {
        public NumberNode()
        {
            Type = NodeType.Number;
        }

        public double Value;
    }

    public class BooleanNode : Node
    {
        public BooleanNode()
        {
            Type = NodeType.Boolean;
        }

        public bool Value;
    }

    public class BinaryNode : Node
    {
        public BinaryNode()
        {
            Type = NodeType.Binary;
        }

        public string Operator;
        public Node Left;
        public Node Right;
    }

    public class AssignmentNode : BinaryNode
    {
        public AssignmentNode()
        {
            Type = NodeType.Assignment;
        }
    }

    public class FunctionNode : Node
    {
        public FunctionNode()
        {
            Type = NodeType.Function;
        }

        public string Name;
        public ProcedureNode Body;
        public List<string> Arguments;
    }

    public class CallNode : Node
    {
        public CallNode()
        {
            Type = NodeType.Call;
        }

        public Node Left;
        public List<Node> Arguments;
    }

    public class EndNode : Node
    {
        public EndNode()
        {
            Type = NodeType.End;
        }
    }

    public class IfNode : Node
    {
        public IfNode()
        {
            Type = NodeType.If;
        }

        public Node Condition;
        public Node Then;
        public Node Else;
    }

    public class ExternNode : Node
    {
        public ExternNode()
        {
            Type = NodeType.Extern;
        }

        public string Value;
        public List<Node> Types;
    }

    public class ReturnNode : Node
    {
        public ReturnNode()
        {
            Type = NodeType.Return;
        }

        public Node Value;
    }

    public class ThrowNode : Node
    {
        public ThrowNode()
        {
            Type = NodeType.Throw;
        }

        public Node Value;
    }

    public class LambdaNode : Node
    {
        public LambdaNode()
        {
            Type = NodeType.Lambda;
        }

        public List<string> Arguments;
        public Node Body;
    }

    public class OverrideNode : Node
    {
        public OverrideNode()
        {
            Type = NodeType.Override;
        }

        public Node Value;
        public OverrideType Override;
        public Node Body;
    }
}
