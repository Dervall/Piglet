using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Piglet.Parser.Construction;

namespace Piglet.Parser.Configuration
{
    internal class NonTerminal<T> : Symbol<T>, INonTerminal<T>
    {
        private readonly IParserConfigurator<T> configurator;
        private readonly IList<NonTerminalProduction> productions;

        public NonTerminal(IParserConfigurator<T> configurator)
        {
            this.configurator = configurator;
            productions = new List<NonTerminalProduction>();
        }

        public IEnumerable<IProductionRule<T>> ProductionRules
        {
            get { return productions; }
        }

        public IProduction<T> AddProduction(params object[] parts)
        {
            if (parts.Any(part => !(part is string || part is ISymbol<T>)))
            {
                throw new ArgumentException("Only string and ISymbol are valid arguments.", "parts");
            }

            var nonTerminalProduction = new NonTerminalProduction(configurator, this, parts);
            productions.Add(nonTerminalProduction);

            return nonTerminalProduction;
        }

        private class NonTerminalProduction : IProduction<T>, IProductionRule<T>
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
                        var regex = (string)part;
                        if (configurator.LexerSettings.EscapeLiterals)
                        {
                            regex = Regex.Escape(regex);
                        }

                        this.symbols[i] = configurator.CreateTerminal(regex, null);
                        this.symbols[i].DebugName = (string)part;   // Set debug name to unescaped string, so it's easy on the eyes.
                    }
                    else
                    {
                        this.symbols[i] = (ISymbol<T>)symbols[i];
                    }
                    ++i;
                }
            }

            public void SetReduceFunction(Func<T[], T> action)
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