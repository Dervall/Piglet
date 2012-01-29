using System;
using System.Collections.Generic;
using System.Linq;
using Piglet.Parser.Construction;

namespace Piglet.Parser.Configuration
{
    public class NonTerminal<T> : Symbol<T>, INonTerminal<T>
    {
        private readonly IParserConfigurator<T> configurator;
        private readonly IList<NonTerminalProduction> productions;

        public NonTerminal(IParserConfigurator<T> configurator, Action<IProductionConfigurator<T>> productionAction)
        {
            this.configurator = configurator;
            productions = new List<NonTerminalProduction>();
            Productions(productionAction);
        }

        public IEnumerable<IProductionRule<T>> ProductionRules
        {
            get { return productions; }
        }

        public void Productions(Action<IProductionConfigurator<T>> productionAction)
        {
            if (productionAction != null)
            {
                productionAction(new NonTerminalProductionConfigurator(configurator, this));
            }
        }

        private class NonTerminalProductionConfigurator : IProductionConfigurator<T>
        {
            private readonly IParserConfigurator<T> configurator;
            private readonly NonTerminal<T> nonTerminal;

            public NonTerminalProductionConfigurator(IParserConfigurator<T> configurator, NonTerminal<T> nonTerminal)
            {
                this.configurator = configurator;
                this.nonTerminal = nonTerminal;
            }

            public IConfigureProductionAction<T> Production(params object[] parts)
            {
                if (parts.Any(part => !(part is string || part is ISymbol<T>)))
                {
                    throw new ArgumentException("Only string and ISymbol are valid arguments.", "parts");
                }

                var nonTerminalProduction = new NonTerminalProduction(configurator, nonTerminal, parts);
                nonTerminal.productions.Add(nonTerminalProduction);

                return nonTerminalProduction;
            }
        }

        private class NonTerminalProduction : IConfigureProductionAction<T>, IProductionRule<T>
        {
            private Func<T[], T> reduceAction;
            private readonly ISymbol<T>[] symbols;
            private readonly INonTerminal<T> resultSymbol;

            public ISymbol<T>[] Symbols { get { return symbols; } }
            public ISymbol<T> ResultSymbol { get { return resultSymbol; } }
            public Func<T[], T> ReduceAction { get { return reduceAction; } } 

            public NonTerminalProduction(IParserConfigurator<T> configurator, INonTerminal<T> resultSymbol, object[] symbols)
            {
                this.resultSymbol = resultSymbol;

                // Move production symbols to the list
                this.symbols = new ISymbol<T>[symbols.Length];
                int i = 0;
                foreach (var part in symbols)
                {
                    if (part is string)
                    {
                        this.symbols[i] = configurator.Terminal((string)part, null);
                    }
                    else
                    {
                        this.symbols[i] = (ISymbol<T>)symbols[i];
                    }
                    ++i;
                }
            }

            public void OnReduce(Func<T[], T> action)
            {
                reduceAction = action;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} =>", DebugName);
        }
    }
}