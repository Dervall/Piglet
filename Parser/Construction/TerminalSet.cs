using System.Collections.Generic;
using System.Linq;
using Piglet.Configuration;

namespace Piglet.Construction
{
    public class TerminalSet<T>
    {
        private readonly Dictionary<ISymbol<T>, List<Terminal<T>>> dict;

        public TerminalSet(IGrammar<T> grammar)
        {
            dict = new Dictionary<ISymbol<T>, List<Terminal<T>>>();

            // Iterate through all the symbols we've got in the grammar
            // and add stuff to the first set
            foreach (var symbol in grammar.AllSymbols.OfType<NonTerminal<T>>())
            {
                // Initialize the list
                dict[symbol] = new List<Terminal<T>>();
            }
        }

        public bool Add(NonTerminal<T> symbol, Terminal<T> terminal)
        {
            var terminals = dict[symbol];
            if (terminals.Contains(terminal))
            {
                return false;
            }
            terminals.Add(terminal);
            return true;
        }

        public IEnumerable<Terminal<T>> this[NonTerminal<T> nonTerminal]
        {
            get { return dict[nonTerminal]; }
        }
    }
}