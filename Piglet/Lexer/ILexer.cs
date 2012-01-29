using System;
using System.IO;

namespace Piglet.Lexer
{
    public interface ILexer<T>
    {
        Tuple<int, T> Next();
        void SetSource(TextReader reader);
        void SetSource(string source);
    }
}