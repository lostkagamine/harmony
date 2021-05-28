using System;
using System.IO;
using System.Linq;
using System.Text;
using TestPL.AST;
using TestPL.Text;

namespace TestPL
{
    class Program
    {
        static string Code = @"""pogger"" 1234 1234";

        static void Main(string[] args)
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(Code));
            var srw = new StreamReaderWrapper(ms);
            srw.Rewind();
            var tk = new Tokeniser(srw);
            var p = new Parser(tk);
            var ast = p.ParseTopLevel();
            Console.WriteLine(ast);
        }
    }
}
