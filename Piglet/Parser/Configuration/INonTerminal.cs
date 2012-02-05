using System;

namespace Piglet.Parser.Configuration
{
    /// <summary>
    /// A non terminal in a given grammar, which may be configured to have productions.
    /// </summary>
    /// <typeparam name="T">Semantic value of tokens in the grammar</typeparam>
    public interface INonTerminal<T> : ISymbol<T>
    {
        /// <summary>
        /// Configure the productions of this nonterminal.
        /// </summary>
        /// <param name="productionAction">Production configuration action provided with a production configurator for this nonterminal</param>
        void Productions(Action<IProductionConfigurator<T>> productionAction);
    }
}