using System;
using System.Collections.Generic;

namespace Piglet.Configuration
{
    public class ParserConfigurator<T> : IParserConfigurator<T>
    {
        private INonTerminal<T> startSymbol;
        private Func<T, T> acceptAction;

        public ParserConfigurator()
        {
        }

        public ITerminal<T> Terminal(string regExp, Func<string, T> onParse = null)
        {
            return new Terminal<T>(regExp, onParse);
        }

        public INonTerminal<T> NonTerminal(Action<IProductionConfigurator<T>> productionAction = null)
        {
            return new NonTerminal<T>(productionAction);
        }

        public void OnAccept(INonTerminal<T> start, Func<T, T> acceptAction)
        {
            startSymbol = start;
            this.acceptAction = acceptAction;
        }

        public IParser<T> CreateParser()
        {
            IList<NonTerminal<T>> nonTerminals = new List<NonTerminal<T>>();
            IList<Terminal<T>> terminals = new List<Terminal<T>>();

            // Gather every symbol in use in the configuration
            ((NonTerminal<T>) startSymbol).GatherSymbols(nonTerminals, terminals);

            // Generate the lexer
            //ILexer<T> lexer = new LexerImpl<T>(terminals, Lexer);


            // We will now generate the LR(1) states from the production rules.
           // IEnumerable<LR1State> lr1States = GenerateLr1States() 

            Console.WriteLine("Terminals:");
            foreach (var terminal in terminals)
            {
                Console.WriteLine(terminal.ToString());
            }

            Console.WriteLine("NonTerminals");
            foreach (var nonTerminal in nonTerminals)
            {
                Console.WriteLine(nonTerminal.ToString());
            }

            return null;
        }
    }
}
