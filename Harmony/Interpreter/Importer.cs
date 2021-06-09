using Harmony.AST;
using Harmony.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Interpreter
{
    public static class Importer
    {
        static string GetHarmonyDirectory()
        {
            var userFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            return Path.Combine(userFolder, ".harmony");
        }

        public static void Import(Interpreter interp, string modulename)
        {
            var fallback = GetHarmonyDirectory();
            var envvar = System.Environment.GetEnvironmentVariable("HARMONY_RESOURCE_ROOT");
            var HarmonyLibDirectory = Path.Combine(envvar ?? fallback, "lib");
            var StdlibPath = Path.Combine(HarmonyLibDirectory, "standard");

            var attempts = new List<string>()
            {
                Path.Combine(System.Environment.CurrentDirectory, "lib"),
                System.Environment.CurrentDirectory,
                StdlibPath
            };

            var truename = modulename;
            if (Path.GetExtension(modulename) == ".har")
            {
                truename = Path.GetFileNameWithoutExtension(modulename); // Make sure we don't add ".har" twice.
            }

            var filenameattempts = new List<string>()
            {
                truename,
                Path.GetFileNameWithoutExtension(truename),
                $"{truename}.har"
            };

            var found = false;
            var file = "";

            var tried = new List<string>();
            foreach (var b in filenameattempts)
            {
                foreach (var a in attempts)
                {
                    var c = Path.Combine(a, b);
                    if (File.Exists(c))
                    {
                        found = true;
                        file = c;
                        goto breakall; // The ONLY valid use of goto and the ONLY time you will ever see me use a goto
                    }
                    tried.Add(c);
                }
            }

            breakall:

            if (!found)
            {
                throw new Exception($"Import failure! Could not find '{truename}'. (Tried: {string.Join(", ", tried)})");
            }

            var sourcecode = File.ReadAllText(file);

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(sourcecode));
            var srw = new StreamReaderWrapper(ms);

            var tokeniser = new Tokeniser(srw);
            var parser = new Parser(tokeniser);

            var node = parser.ParseTopLevel();

            interp.Evaluate(node);
        }
    }
}
