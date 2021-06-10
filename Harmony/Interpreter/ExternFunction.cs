using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Interpreter
{
    public class ExternFunction
    {
        public System.Reflection.MethodInfo Method;
        public dynamic Callee = null;

        public Container Call(List<Container> args)
        {
            return new Container(Method.Invoke(Callee, args.Select(e => e.Value).ToArray()));
        }

        public override string ToString()
        {
            return $"<external function '{Method.DeclaringType.Name}.{Method.Name}'>";
        }
    }
}
