using System;

namespace Piglet
{
    public interface ITerminal<T> : ISymbol<T>
    {
        string RegExp { get; }
        Func<string, T> OnParse { get; }
    }
}