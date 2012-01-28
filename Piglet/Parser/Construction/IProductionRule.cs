using System;
using Piglet.Configuration;

namespace Piglet.Construction
{
    public interface IProductionRule<T>
    {
        ISymbol<T>[] Symbols { get; }
        ISymbol<T> ResultSymbol { get; }
        Func<T[], T> ReduceAction { get; } 
    }
}