using System;
using System.Collections.Generic;
using System.Linq;
using Piglet.Common;
using Piglet.Lexer.Construction;

namespace Piglet.Lexer.Runtime
{
    internal class TransitionTable<T>
    {
        private readonly ITable2D table;
        private readonly Tuple<int, Func<string, T>>[] actions;

        public TransitionTable(DFA dfa, IList<NFA> nfas, IList<Tuple<string, Func<string, T>>> tokens)
        {
            var uncompressed = new short[dfa.States.Count(),256];

            // Fill table with -1
            for (int i = 0; i < dfa.States.Count(); ++i )
            {
                for (int j = 0; j < 256; ++j)
                {
                    uncompressed[i, j] = -1;
                }
            }

            actions = new Tuple<int, Func<string, T>>[dfa.States.Count];

            foreach (var state in dfa.States)
            {
                // Store to avoid problems with modified closure
                DFA.State state1 = state; 
                foreach (var transition in dfa.Transitions.Where(f => f.From == state1))
                {
                    // Set the table entry
                    for (int i = 0; i < 256; ++i )
                    {
                        // TODO: Here is the place to support unicode lexing using a tabular lexer.
                        if (transition.ValidInput.ContainsChar((char)i))
                        {
                            uncompressed[state.StateNumber, i] = (short) transition.To.StateNumber;
                        }
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

            table = new CompressedTable(uncompressed);
        }

        public int this[int state, char c]
        {
            get
            {
                return table[state, c];
            }
        }

        public Tuple<int, Func<string, T>> GetAction(int state)
        {
            return actions[state];
        }
    }
}