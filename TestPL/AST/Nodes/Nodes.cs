using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPL.AST.Nodes
{
    public class Node
    {
        public NodeType Type;
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
}
