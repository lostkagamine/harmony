using Harmony.AST.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Interpreter
{
    public class Interpreter
    {
        public Environment Environment;

        Container DoIf(IfNode ast)
        {
            var cond = Evaluate(ast.Condition);
            if (cond.Truthy())
            {
                return Evaluate(ast.Then);
            } else
            {
                if (ast.Else != null)
                    return Evaluate(ast.Else);
                return new Container(null);
            }
        }

        Type GetTypeEverywhere(string name)
        {
            var di = AppDomain.CurrentDomain;
            foreach (var e in di.GetAssemblies())
            {
                var t = e.GetType(name);
                if (t != null)
                    return t;
            }
            return null;
        }

        public Container Evaluate(Node ast, Environment _env = null)
        {
            var env = Environment;
            if (_env != null)
                env = _env;
            Environment = env; // ?

            switch (ast.Type)
            {
                case NodeType.String:
                    return new Container(((StringNode)ast).Value);
                case NodeType.Boolean:
                    return new Container(((BooleanNode)ast).Value);
                case NodeType.Number:
                    return new Container(((NumberNode)ast).Value);
                case NodeType.Identifier:
                    return env.WideGet(((IdentifierNode)ast).Value);
                case NodeType.Procedure:
                    Container output = null;
                    foreach (var n in ((ProcedureNode)ast).Body)
                    {
                        output = Evaluate(n, _env);
                    }
                    return output;
                case NodeType.Call:
                    var cn = ((CallNode)ast);
                    var fn = Evaluate(cn.Left);
                    if (fn.Nil())
                        throw new Exception($"attempt to call a nil value '{fn}'");
                    if (!fn.IsFunction())
                        throw new Exception($"attempt to call a non-callable value '{fn}'");
                    var args = new List<Container>();
                    foreach (var t in cn.Arguments)
                    {
                        args.Add(Evaluate(t));
                    }
                    return fn.Call(this, args);
                case NodeType.If:
                    return DoIf((IfNode)ast);
                case NodeType.Assignment:
                    var an = ((AssignmentNode)ast);
                    var left = an.Left;
                    if (left.Type != NodeType.Identifier)
                        throw new Exception($"attempt to assign to invalid value: {left.Type}");
                    var id = ((IdentifierNode)left).Value;
                    var right = an.Right;
                    var rightval = Evaluate(right);
                    return env.Set(id, rightval);
                case NodeType.Extern:
                    var en = ((ExternNode)ast);
                    var name = en.Value;
                    var spl = name.Split('.');
                    var typename = string.Join('.', spl[0..(spl.Length - 1)]);
                    var methodname = spl[spl.Length - 1];
                    var type = GetTypeEverywhere(typename);
                    if (type == null)
                        throw new Exception($"cannot find extern type '{typename}'");
                    var types = new List<Type>();
                    foreach (var a in en.Types)
                    {
                        types.Add(GetTypeEverywhere(((IdentifierNode)a).Value));
                    }
                    var method = type.GetMethod(methodname, types.ToArray());
                    if (method == null)
                        throw new Exception($"cannot find extern method '{method}'");
                    return new Container(new ExternFunction()
                    {
                        Method = method
                    });
                case NodeType.Function:
                    var fnode = ((FunctionNode)ast);
                    var fname = fnode.Name;
                    var fobj = new HarmonyFunction()
                    {
                        Arguments = fnode.Arguments,
                        Body = fnode.Body
                    };
                    return env.Set(fname, new Container(fobj));

                default:
                    throw new Exception($"interpretation failure: '{ast.Type}'");
            }
        }
    }
}
