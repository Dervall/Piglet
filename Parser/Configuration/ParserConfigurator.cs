using System;
using System.Collections.Generic;
using System.Linq;
using Piglet.Construction;

namespace Piglet.Configuration
{
    public class ParserConfigurator<T> : IParserConfigurator<T>, IGrammar<T>
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
            Terminal<T> terminal = terminals.SingleOrDefault(f => f.RegExp == regExp);
            if (terminal != null)
            {
                if (terminal.OnParse != onParse)
                    throw new ParserConfigurationException(
                        "Redefinition of terminal uses the same regex but different onParse action");
            }
            else
            {
                terminal = new Terminal<T>(regExp, onParse);
                terminals.Add(terminal);
            }
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
            // First we need to augment the grammar with a start rule and a new start symbol
            // Create the derived start symbol
            var augmentedStart = (NonTerminal<T>)NonTerminal();  // Unfortunate cast...

            // Use the start symbols debug name with a ' in front to indicate the augmented symbol.
            augmentedStart.DebugName = "'" + startSymbol.DebugName;

            // Create a single production 
            augmentedStart.Productions(p => p.Production(startSymbol).OnReduce(f => acceptAction(f[0])));
            startRule = augmentedStart.ProductionRules.First(); // There's only one production.

            // Add the end of input symbol
            var eoi = Terminal(null, s => default(T));
            eoi.DebugName = "$";

            // Make sure all the terminals are registered.
            // This becomes neccessary since the user can configure the parser using only strings.
            // Since the nonterminal used for that does not carry a back-reference to the configurator,
            // we do it this way.
            foreach (var nonTerminal in nonTerminals)
            {
                foreach (var terminal in nonTerminal.ProductionRules.SelectMany( f => f.Symbols).OfType<Terminal<T>>())
                {
                    var oldTerminal = terminals.SingleOrDefault(f => f.RegExp == terminal.RegExp);
                    if (oldTerminal != null)
                    {
                        if (oldTerminal.OnParse != terminal.OnParse)
                        {
                            throw new ParserConfigurationException(
                                "Multiply defined terminal has more than one OnParse action");
                        }
                    }
                    else
                    {
                        terminals.Add(terminal);
                    }
                }
            }

            // This class is now a valid implementation of IGrammar, ready to use. Create a new parserfactory
            // and pass this into it, for use.

            return new ParserFactory<T>(this).CreateParser();
        }

        public IProductionRule<T> Start
        {
            get 
            { 
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
                foreach (var terminal in terminals)
                {
                    yield return terminal;
                }
 
                foreach (var nonTerminal in nonTerminals)
                {
                    yield return nonTerminal;
                }
            }
        }

        public NonTerminal<T> AcceptSymbol
        {
            get { return (NonTerminal<T>)Start.ResultSymbol; }
        }

        public Terminal<T> EndOfInputTerminal
        {
            get { return terminals.Single(f => f.RegExp == null); }
        }
    }
}
