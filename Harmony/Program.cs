using System;
using System.IO;
using System.Linq;
using System.Text;
using Harmony.AST;
using Harmony.Text;
using Harmony.Interpreter;

namespace Harmony
{
    class Program
    {
        static void Main(string[] args)
        {
            var Code = File.ReadAllText(args.Length != 0 ? args[0] : "Examples/test.har");

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(Code));
            var srw = new StreamReaderWrapper(ms);
            srw.Rewind();
            var tk = new Tokeniser(srw);
            var p = new Parser(tk);
            var ast = p.ParseTopLevel();

            var env = new Interpreter.Environment();
            env.Define("println", new Container((Action<object>)Console.WriteLine));

            var intp = new Interpreter.Interpreter();
            intp.Environment = env;
            intp.Evaluate(ast);
        }
    }
}
