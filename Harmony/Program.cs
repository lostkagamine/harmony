using System;
using System.IO;
using System.Linq;
using System.Text;
using Harmony.AST;
using Harmony.Text;
using Harmony.Interpreter;
using System.Reflection;

namespace Harmony
{
    class Program
    {
        static void Main(string[] args)
        {
            var Code = File.ReadAllText(args.Length != 0 ? args[0] : "Examples/test.hc");

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(Code));
            var srw = new StreamReaderWrapper(ms);
            srw.Rewind();
            var tk = new Tokeniser(srw);
            var p = new Parser(tk);
            var ast = p.ParseTopLevel();

            var env = new Interpreter.Environment();

            var intp = new Interpreter.Interpreter();
            intp.Environment = env;

            env.Define("use_assembly!", new Container(
                (Action<string>)((a) =>
                {
                    Assembly.Load(a);
                })
            ));

            env.Define("using", new Container(
                (Action<string>)((a) =>
                {
                    Importer.Import(intp, a);
                })
            ));

            intp.Evaluate(ast);
        }
    }
}
