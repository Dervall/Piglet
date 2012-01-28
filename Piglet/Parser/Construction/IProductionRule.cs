using System;
using Piglet.Parser.Configuration;

namespace Piglet.Parser.Construction
{
    public interface IProductionRule<T>
    {
        ISymbol<T>[] Symbols { get; }
        ISymbol<T> ResultSymbol { get; }
        Func<T[], T> ReduceAction { get; } 
    }
}