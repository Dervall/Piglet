using System;
using System.IO;

namespace Piglet.Lexer
{
    public interface ILexer<T>
    {
        Tuple<int, T> Next();
        TextReader Source { get; set; }
    }
}