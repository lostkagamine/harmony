using Harmony.AST.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Interpreter
{
    public static class MoreMath
    {
        public static double Up(double a, double b, double n)
        {
            if (n == 1) return Math.Pow(a, b);
            else if (n >= 1 && b == 0) return 1;
            else return Up(a, Up(a, (b - 1), n), n - 1);
        }
    }

    public class Interpreter
    {
        public Environment Environment;

        internal static Interpreter Instance;

        public Interpreter()
        {
            Instance = this;
        }

        Container DoIf(IfNode ast)
        {
            var cond = Evaluate(ast.Condition);
            if (cond.Truthy())
            {
                return Evaluate(ast.Then);
            }
            else
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

        Container ResolveValue(string value)
        {
            Container thisVal = null;
            var spl = value.Split(".");

            foreach (var a in spl)
            {
                if (thisVal == null)
                {
                    thisVal = Environment.WideGet(a);
                    continue;
                }

                thisVal = thisVal.GetIndex(a);
            }

            return thisVal;
        }

        Container DoBinary(BinaryNode ast)
        {
            T Ensure<T>(Container i)
            {
                if (i.Value is not T)
                    throw new Exception($"expected type '{typeof(T).Name}' here");
                return (T)i.Value;
            };

            double Number(Container i)
            {
                var n = Ensure<double>(i);
                return n;
            }

            double NotZero(Container i)
            {
                var n = Ensure<double>(i);
                if (n == 0)
                {
                    throw new Exception($"found a zero value where there should not have been (divide by zero?)");
                }
                return n;
            }

            var left = Evaluate(ast.Left);
            var right = Evaluate(ast.Right);
            dynamic oval = null;

            switch (ast.Operator)
            {
                // TODO: Operator overloading. Add it in Container. 
                case "+":
                    oval = Number(left) + Number(right);
                    break;
                case "-":
                    oval = Number(left) - Number(right);
                    break;
                case "*":
                    oval = Number(left) * Number(right);
                    break;
                case "/":
                    oval = Number(left) / NotZero(right);
                    break;
                case "^":
                    oval = Math.Pow(Number(left), Number(right)); // Is using ^ for power a good idea?
                    break;
                case "^^":
                    oval = MoreMath.Up(Number(left), Number(right), 2);
                    break;

                // TODO: Implement equality overloading in Container.
                case "==":
                    oval = left.Value == right.Value;
                    break;
                case "!=":
                    oval = left.Value != right.Value;
                    break;
                case ">=":
                    oval = left.Value >= right.Value;
                    break;
                case "<=":
                    oval = left.Value <= right.Value;
                    break;
                case ">":
                    oval = left.Value > right.Value;
                    break;
                case "<":
                    oval = left.Value < right.Value;
                    break;

            }

            return new Container(oval);
        }

        public Container ConstructFunctionInContainer(Node body, List<string> args)
        {
            return new Container(
                new HarmonyFunction()
                {
                    Body = body,
                    Arguments = args
                });
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
                    return ResolveValue(((IdentifierNode)ast).Value);
                case NodeType.Procedure:
                    Container output = null;
                    foreach (var n in ((ProcedureNode)ast).Body)
                    {
                        output = Evaluate(n, _env);
                        if (output != null && output.Returning)
                        {
                            output.Returning = false;
                            break;
                        }
                    }
                    return output;
                case NodeType.Call:
                    var cn = ((CallNode)ast);
                    var fn = Evaluate(cn.Left);
                    if (fn.Nil())
                        throw new Exception($"attempt to call a nil value '{fn}'");
                    if (!fn.IsFunction())
                        throw new Exception($"attempt to call a non-callable value '{((object)fn.Value).GetType().Name}'");
                    var args = new List<Container>();
                    foreach (var t in cn.Arguments)
                    {
                        var res = Evaluate(t);
                        args.Add(res);
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
                case NodeType.Lambda:
                    var lnode = ((LambdaNode)ast);
                    var lobj = new HarmonyFunction()
                    {
                        Arguments = lnode.Arguments,
                        Body = lnode.Body
                    };
                    return new Container(lobj);
                case NodeType.Binary:
                    return DoBinary((BinaryNode)ast);
                case NodeType.Return:
                    var the = Evaluate(((ReturnNode)ast).Value);
                    the.Returning = true;
                    return the;
                case NodeType.Throw:
                    var msg = Evaluate(((ThrowNode)ast).Value).Value;
                    if (msg is Exception)
                    {
                        throw msg;
                    }
                    throw new Exception($"userland error: '{msg}'");
                case NodeType.Override:
                    var or_val = Evaluate(((OverrideNode)ast).Value);
                    var or_body = ((OverrideNode)ast).Body;
                    var or_type = ((OverrideNode)ast).Override;



                    switch (or_type)
                    {
                        case OverrideType.Getter:
                            or_val.Getter = ConstructFunctionInContainer(or_body, new List<string>());
                            break;
                        case OverrideType.Setter:
                            or_val.Setter = ConstructFunctionInContainer(or_body, new List<string>());
                            break;
                    }
                    return new Container(null);

                default:
                    throw new Exception($"interpretation failure: '{ast.Type}'");
            }
        }
    }
}