using System;
using System.Collections.Generic;
using System.Linq;

namespace Piglet.Lexer
{
    public class TransitionTable<T>
    {
        private readonly short[,] table;
        private readonly Tuple<int, Func<string, T>>[] actions;

        public TransitionTable(DFA dfa, NFA[] nfas, List<Tuple<string, Func<string, T>>> tokens)
        {
            table = new short[dfa.States.Count(),255];
            
            // Fill table with illegal action everywhere
            for (int i = 0; i < dfa.States.Count(); ++i )
            {
                for (int j =0; j<255; ++j)
                {
                    table[i, j] = -1;
                }
            }

            actions = new Tuple<int, Func<string, T>>[dfa.States.Count];

            foreach (var state in dfa.States)
            {
                DFA.State state1 = state;
                foreach (var transition in dfa.Transitions.Where(f => f.From == state1))
                {
                    // Set the table entry
                    table[state.StateNumber, transition.OnCharacter] = (short)transition.To.StateNumber;

                    // If this is an accepting state, set the action function to be
                    // the FIRST defined action function if multiple ones match
                    if (state.NfaStates.Any(f => f.AcceptState))
                    {
                        // Find the lowest ranking NFA which has the accepting state in it
                        for (int tokenNumber = 0; tokenNumber < nfas.Count(); ++tokenNumber)
                        {
                            NFA nfa = nfas[tokenNumber];
                            if (nfa.States.Intersect(state.NfaStates.Where(f=>f.AcceptState)).Any())
                            {
                                // Match
                                actions[state.StateNumber] =  new Tuple<int, Func<string, T>>(
                                    tokenNumber, tokens[tokenNumber].Item2);
                                break;
                            }
                        }   
                    }
                }
            }
        }

        public short this[int state, char c]
        {
            get { return table[state, c]; }
        }

        public Tuple<int, Func<string, T>> GetAction(int state)
        {
            return actions[state];
        }
    }
}