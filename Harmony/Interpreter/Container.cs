using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Interpreter
{
    /**
     * <summary>A <c>Container</c> is the basic object that holds a certain value.
     * <c>Container</c>s can have their behaviour overridden.</summary>
     */
    public class Container
    {
        public string Name;
        dynamic _Value;

        public Container Getter = null;
        public Container Setter = null;

        public dynamic Value
        {
            get
            {
                return Get();
            }
        }

        internal bool Returning;

        public Container(dynamic v)
        {
            _Value = v;
        }

        public dynamic Get()
        {
            if (Getter != null)
            {
                if (!Getter.IsFunction())
                {
                    throw new Exception($"getter on value '{Name}' is not a function");
                }
                return Getter.Call(Interpreter.Instance, new List<Container>()).Value; // No args
            }

            return _Value;
        }

        public bool Truthy()
        {
            if (Value is null)
                return false;
            if (Value is bool)
                return Value;
            if (Value is string @string)
                return @string.Length != 0;
            if (Value is double @double)
                return @double > 0;
            return true;
        }

        public bool IsNumber()
        {
            return Value is double;
        }

        public bool IsFunction()
        {
            return Value is HarmonyFunction
                or Delegate
                or ExternFunction;
        }

        public Container Call(Interpreter i, List<Container> args)
        {
            if (((object)Value).GetType().IsAssignableTo(typeof(Delegate)))
            {
                var del = (Delegate)Value;
                var ret = del.DynamicInvoke(args.Select(e => e.Value).ToArray());
                return new Container(ret);
            } else if (Value is HarmonyFunction)
            {
                return ((HarmonyFunction)Value).Run(i, args);
            } else if (Value is ExternFunction)
            {
                return ((ExternFunction)Value).Call(args);
            }
            throw new Exception($"attempted to call a non-callable");
        }

        public bool Nil()
        {
            return Value is null;
        }

        public T As<T>()
        {
            return (T)Value;
        }

        public Container GetIndex(dynamic index)
        {
            if (Value is double or bool or null)
            {
                throw new Exception($"cannot index a value of type '{((object)Value).GetType().Name}'");
            }

            var v = ((object)Value);
            var props = v.GetType().GetProperties();

            foreach (var p in props)
            {
                if (p.GetIndexParameters().Length > 0)
                {
                    // This is an indexer
                    return new Container(p.GetGetMethod().Invoke(v, new[] { index }));
                }
            }

            // No indexers
            // Is there a method named that?

            foreach (var m in v.GetType().GetMethods())
            {
                if (m.Name == index)
                {
                    return new Container(new ExternFunction()
                    {
                        Method = m,
                        Callee = v
                    });
                }
            }

            // No such luck
            // Is there a field?

            foreach (var f in v.GetType().GetFields())
            {
                if (f.Name == index)
                {
                    return new Container(f.GetValue(v));
                }
            }

            // No.
            // Let's just die.
            throw new Exception($"no such index '{index}' on value '{Value}'");
        }
    }
}
