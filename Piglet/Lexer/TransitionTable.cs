using System;
using System.Collections.Generic;
using System.Linq;
using Piglet.Lexer.Construction;

namespace Piglet.Lexer
{
    public class TransitionTable<T>
    {
        private readonly IDictionary<Tuple<int, int>, int> table;
        private readonly Tuple<int, Func<string, T>>[] actions;

        public TransitionTable(DFA dfa, IList<NFA> nfas, IList<Tuple<string, Func<string, T>>> tokens)
        {
            table = new Dictionary<Tuple<int, int>, int>();
            actions = new Tuple<int, Func<string, T>>[dfa.States.Count];

            foreach (var state in dfa.States)
            {
                // Store to avoid problems with modified closure
                DFA.State state1 = state; 
                foreach (var transition in dfa.Transitions.Where(f => f.From == state1))
                {
                    // Set the table entry
                    foreach (var input in transition.ValidInput)
                    {
                        table[new Tuple<int, int>(state.StateNumber, input)] = (short)transition.To.StateNumber;
                    }
                }

                // If this is an accepting state, set the action function to be
                // the FIRST defined action function if multiple ones match
                if (state.NfaStates.Any(f => f.AcceptState))
                {
                    // Find the lowest ranking NFA which has the accepting state in it
                    for (int tokenNumber = 0; tokenNumber < nfas.Count(); ++tokenNumber)
                    {
                        NFA nfa = nfas[tokenNumber];

                        if (nfa.States.Intersect(state.NfaStates.Where(f => f.AcceptState)).Any())
                        {
                            // Match
                            // This might be a token that we ignore. This is if the tokenNumber >= number of tokens
                            // since the ignored tokens are AFTER the normal tokens. If this is so, set the action func to
                            // int.MinValue, NULL to signal that the parsing should restart without reporting errors
                            if (tokenNumber >= tokens.Count())
                            {
                                actions[state.StateNumber] = new Tuple<int, Func<string, T>>(int.MinValue, null);
                            }
                            else
                            {
                                actions[state.StateNumber] = new Tuple<int, Func<string, T>>(
                                    tokenNumber, tokens[tokenNumber].Item2);
                            }
                            break;
                        }
                    }
                }
            }
        }

        public int this[int state, char c]
        {
            get
            {
                int v;
                if (table.TryGetValue(new Tuple<int, int>(state, c), out v))
                    return v;
                return -1;
            }
        }

        public Tuple<int, Func<string, T>> GetAction(int state)
        {
            return actions[state];
        }
    }
}