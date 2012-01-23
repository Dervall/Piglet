using System;
using System.Collections.Generic;

namespace Piglet
{
    public interface INonTerminal<T> : ISymbol<T>
    {
        void Productions(Action<IProductionConfigurator<T>> productionAction);
     //   internal void GatherSymbols(ISet<INonTerminal<T>> nonTerminals, ISet<ITerminal<T>> terminals);
    }
}