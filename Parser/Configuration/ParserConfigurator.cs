using System;
using System.Collections.Generic;
using System.Linq;
using Piglet.Construction;

namespace Piglet.Configuration
{
    public class ParserConfigurator<T> : IParserConfigurator<T>, IParserConfiguration<T>
    {
        private NonTerminal<T> startSymbol;
        private Func<T, T> acceptAction;

        private IProductionRule<T> startRule;
        private readonly List<NonTerminal<T>> nonTerminals;
        private readonly List<Terminal<T>> terminals; 

        public ParserConfigurator()
        {
            nonTerminals = new List<NonTerminal<T>>();
            terminals = new List<Terminal<T>>();
        }

        public ITerminal<T> Terminal(string regExp, Func<string, T> onParse = null)
        {
            var terminal = new Terminal<T>(regExp, onParse);
            terminals.Add(terminal);
            return terminal;
        }

        public INonTerminal<T> NonTerminal(Action<IProductionConfigurator<T>> productionAction = null)
        {
            var nonTerminal = new NonTerminal<T>(productionAction);
            nonTerminals.Add(nonTerminal);
            return nonTerminal;
        }

        public void OnAccept(INonTerminal<T> start, Func<T, T> acceptAction)
        {
            startSymbol = (NonTerminal<T>) start;
            this.acceptAction = acceptAction;
        }

        public IParser<T> CreateParser()
        {
            return ParserFactory.CreateParser(this);
        }

        public IProductionRule<T> Start
        {
            get 
            { 
                if (startRule == null)
                {
                    // No start rule yet? Augment the grammar
                    // Create the derived start symbol
                    var augmentedStart = (NonTerminal<T>)NonTerminal();  // Unfortunate cast...

                    // Use the start symbols debug name with a ' in front to indicate the augmented symbol.
                    augmentedStart.DebugName = "'" + startSymbol.DebugName;
                    
                    // Create a single production 
                    augmentedStart.Productions(p => p.Production(startSymbol).OnReduce(f => acceptAction(f[0])));
                    startRule = augmentedStart.ProductionRules.First(); // There's only one production.

                }
                return startRule; 
            }
        }

        public IEnumerable<IProductionRule<T>> ProductionRules
        {
            get { return nonTerminals.SelectMany(nonTerminal => nonTerminal.ProductionRules); }
        }

        public IEnumerable<ISymbol<T>> AllSymbols
        {
            get {
                foreach (var nonTerminal in nonTerminals)
                {
                    yield return nonTerminal;
                }
                foreach (var terminal in terminals)
                {
                    yield return terminal;
                }
            }
        }
    }
}
