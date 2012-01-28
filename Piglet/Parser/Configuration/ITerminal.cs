using System;

namespace Piglet.Parser.Configuration
{
    public interface ITerminal<T> : ISymbol<T>
    {
        string RegExp { get; }
        Func<string, T> OnParse { get; }
    }
}