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
    }
}
