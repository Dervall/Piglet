using System;

namespace Piglet.Parser.Configuration
{
    public interface INonTerminal<T> : ISymbol<T>
    {
        void Productions(Action<IProductionConfigurator<T>> productionAction);
    }
}