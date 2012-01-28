using System;

namespace Piglet.Configuration
{
    public interface INonTerminal<T> : ISymbol<T>
    {
        void Productions(Action<IProductionConfigurator<T>> productionAction);
    }
}