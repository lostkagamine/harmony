using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPL.Text
{
    public class StreamReaderWrapper : IDisposable
    {
        public StreamReader Reader;
        public Stream Stream;
        public long Position = 0;
        public int Line = 1;
        public int Column = 0;

        string backend = "";

        public StreamReaderWrapper(Stream st)
        {
            Stream = st;
            Reader = new StreamReader(Stream);
            Reader.BaseStream.Position = 0;
            backend = Reader.ReadToEnd();
            Stream.Close();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Reader.Close();
        }

        public Exception Die(string msg)
        {
            return new Exception($"{Line}:{Column}: {msg}");
        }

        public char Next()
        {
            if (Eof)
                return '\0';
            var j = backend[(int)Position];
            if (j == '\n')
            {
                Line++;
                Column = 0;
            }
            else
            {
                Column++;
            }
            Position++;
            return j;
        }

        public char Peek() => backend[(int)Position];

        public string ReadUntil(Func<char, bool> predicate)
        {
            var o = "";
            while (!Eof && !predicate(Peek()))
                o += Next();
            return o;
        }

        public string ReadWhile(Func<char, bool> predicate)
        {
            var o = "";
            while (!Eof && predicate(Peek()))
                o += Next();
            return o;
        }

        public bool Eof => Position >= backend.Length;

        public void Rewind()
        {
            Position = 0;
        }
    }
}
