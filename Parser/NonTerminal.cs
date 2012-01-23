using System;
using System.Collections.Generic;
using System.Linq;

namespace Piglet
{
    public class NonTerminal<T> : Symbol<T>, INonTerminal<T>
    {
        private readonly IList<NonTerminalProduction> productions;

        public NonTerminal(Action<IProductionConfigurator<T>> productionAction)
        {
            productions = new List<NonTerminalProduction>();
            Productions(productionAction);
        }

        public void Productions(Action<IProductionConfigurator<T>> productionAction)
        {
            if (productionAction != null)
            {
                productionAction(new NonTerminalProductionConfigurator(this));
            }
        }

        public override string ToString()
        {
            return string.Format("{0} =>", DebugName);
        }

        private class NonTerminalProduction : IConfigureProductionAction<T>
        {
            private Func<T[], T> reduceAction;
            private readonly ISymbol<T>[] symbols;

            public ISymbol<T>[] Symbols { get { return symbols; } }

            public NonTerminalProduction(object[] symbols)
            {
                // Move production symbols to the list
                this.symbols = new ISymbol<T>[symbols.Length];
                int i = 0;
                foreach (var part in symbols)
                {
                    if (part is string)
                    {
                        this.symbols[i] = new Terminal<T>((string) part, null);
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

        private class NonTerminalProductionConfigurator : IProductionConfigurator<T>
        {
            private readonly NonTerminal<T> nonTerminal;

            public NonTerminalProductionConfigurator(NonTerminal<T> nonTerminal)
            {
                this.nonTerminal = nonTerminal;
            }

            public IConfigureProductionAction<T> Production(params object[] parts)
            {
                if (parts.Any(part => !(part is string || part is ISymbol<T>)))
                {
                    throw new ArgumentException("Only string and ISymbol are valid arguments.", "parts");
                }

                var nonTerminalProduction = new NonTerminalProduction(parts);
                nonTerminal.productions.Add(nonTerminalProduction);

                return nonTerminalProduction;
            }
        }

        public void GatherSymbols(IList<NonTerminal<T>> nonTerminals, IList<Terminal<T>> terminals)
        {
            if (!nonTerminals.Contains(this))
            {
                nonTerminals.Add(this);
            }

            foreach (var production in productions)
            {
                foreach (var symbol in production.Symbols)
                {
                    if (symbol is NonTerminal<T>)
                    {
                        if (!nonTerminals.Contains(symbol))
                        {
                            ((NonTerminal<T>)symbol).GatherSymbols(nonTerminals, terminals);
                        }
                    } 
                    else
                    {
                        var terminal = (Terminal<T>)symbol;
                        if (terminals.Any(f => f.RegExp == terminal.RegExp))
                        {
                            if (terminals.Any(f => f.OnParse != terminal.OnParse))
                            {
                                // There's a terminal which has been defined earlier with a different OnParse
                                // This is illegal!
                                throw new ParserConfigurationException(
                                    string.Format(
                                        "Two terminal symbols using the RegExp {0} but using different OnParse actions",
                                        terminal.RegExp));

                            }
                        }
                        else
                        {
                            terminals.Add(terminal);
                        }
                    }
                }
            }
        }
    }
}