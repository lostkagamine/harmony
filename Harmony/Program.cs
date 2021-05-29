using System;
using System.IO;
using System.Linq;
using System.Text;
using Harmony.AST;
using Harmony.Text;

namespace Harmony
{
    class Program
    {
        static void Main(string[] args)
        {
            var Code = File.ReadAllText("Examples/test.har");

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(Code));
            var srw = new StreamReaderWrapper(ms);
            srw.Rewind();
            var tk = new Tokeniser(srw);
            var p = new Parser(tk);
            var ast = p.ParseTopLevel();
        }
    }
}
