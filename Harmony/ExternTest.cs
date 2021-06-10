using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony.Interpreter;
using Harmony.AST;
using Harmony.Text;
using System.IO;
using Harmony.AST.Nodes;
using System;

namespace Harmony
{
    public class AnObject
    {
        public string ThisIsAValue = "yes it is";

        public void DoSomething(string a)
        {
            Console.WriteLine(a);
        }
    }

    public static class ObjectFactory
    {
        public static AnObject GetObject()
        {
            return new AnObject();
        }
    }
}
