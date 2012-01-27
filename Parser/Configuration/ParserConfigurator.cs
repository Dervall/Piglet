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

        public ParserConfigurator()
        {
            nonTerminals = new List<NonTerminal<T>>();
        }

        public ITerminal<T> Terminal(string regExp, Func<string, T> onParse = null)
        {
            return new Terminal<T>(regExp, onParse);
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
    }

 /*   public class StartProductionRule<T> : IProductionRule<T>
    {
        private readonly NonTerminal<T> startSymbol;

        public StartProductionRule(NonTerminal<T> startSymbol)
        {
            this.startSymbol = startSymbol;
        }

        public ISymbol<T>[] Symbols
        {
            get { return new ISymbol<T>[] {startSymbol}; }
        }

        public ISymbol<T> ResultSymbol
        {
            get { return null; }
        }

    }*/
}
