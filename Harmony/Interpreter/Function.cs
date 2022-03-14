using Harmony.AST.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Interpreter
{
    /**
     * <summary>
     *  A <c>Function</c> is an object that holds an abstract syntax tree,
     *  and that can be executed.
     * </summary>
     */
    public interface IFunction
    {
        public Container Run(Interpreter i, List<Container> args);
    }

    public class HarmonyFunction : IFunction
    {
        public Node Body;
        public List<string> Arguments;
        public HarmonyFunction Precondition = null;

        public Container Run(Interpreter i, List<Container> args)
        {
            var oldscope = i.Environment;
            var myscope = oldscope.Extend();

            var ind = 0;
            foreach (var key in Arguments)
            {
                Container val = null;
                if (ind < args.Count)
                    val = args[ind];
                myscope.Define(key, val);
                ind++;
            }

            var ret = i.Evaluate(Body, myscope);

            i.Environment = oldscope;

            return ret;
        }
    }
}
