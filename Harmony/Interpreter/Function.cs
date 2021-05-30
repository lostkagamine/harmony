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

        public Container Run(Interpreter i, List<Container> args)
        {
            // TODO
            return new Container(null);
        }
    }
}
