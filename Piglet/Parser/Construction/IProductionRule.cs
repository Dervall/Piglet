using System;

using Piglet.Parser.Configuration;
using Piglet.Lexer.Runtime;

namespace Piglet.Parser.Construction
{
    internal interface IProductionRule<T>
    {
        ISymbol<T>[] Symbols { get; }
        ISymbol<T> ResultSymbol { get; }
        Func<ParseException, LexedToken<T>[], T> ReduceAction { get; }
        IPrecedenceGroup ContextPrecedence { get; }
    }
}