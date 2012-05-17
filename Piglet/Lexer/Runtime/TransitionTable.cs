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
        private readonly char[] inputRangeEnds;

        public TransitionTable(DFA dfa, IList<NFA> nfas, IList<Tuple<string, Func<string, T>>> tokens)
        {
            // Get a list of all valid input ranges that are distinct.
            // This will fill up the entire spectrum from 0 to max char
            // Sort these ranges so that they start with the lowest to highest start
            var allValidRanges =
                nfas.Select(
                    f =>
                    f.Transitions.Aggregate(Enumerable.Empty<CharRange>(), (acc, a) => acc.Union(a.ValidInput.Ranges)))
                    .Aggregate((acc, a) => acc.Union(a))
                    .OrderBy(f => f.From)
                    .ToList();
            
            // This list might not be properly terminated at both ends. This happens if there
            // never is anything that accepts any character.
            char start = allValidRanges.First().From;
            if (start != '\0')
            {
                // Add a range that goes from \0 to the character before start
                allValidRanges.Insert(0, new CharRange { From = '\0', To = (char) (start - 1)});
            }

            char end = allValidRanges.Last().To;
            if (end != char.MaxValue)
            {
                allValidRanges.Add(new CharRange { From = (char) (end + 1), To = char.MaxValue});
            }

            // Create a 2D table
            // First dimension is the number of states found in the DFA
            // Second dimension is number of distinct character ranges
            var uncompressed = new short[dfa.States.Count(),allValidRanges.Count()];

            // Fill table with -1
            for (int i = 0; i < dfa.States.Count(); ++i )
            {
                for (int j = 0; j < allValidRanges.Count(); ++j)
                {
                    uncompressed[i, j] = -1;
                }
            }

            // Save the ends of the input ranges into an array
            inputRangeEnds = allValidRanges.Select(f => f.To).ToArray();
            actions = new Tuple<int, Func<string, T>>[dfa.States.Count];

            foreach (var state in dfa.States)
            {
                // Store to avoid problems with modified closure
                DFA.State state1 = state; 
                foreach (var transition in dfa.Transitions.Where(f => f.From == state1))
                {
                    // Set the table entry
                    for (int i = 0; i < allValidRanges.Count(); ++i )
                    {
                        var range = allValidRanges[i];
                        if (transition.ValidInput.Ranges.Contains(range))
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
                // Determine the corrent input range index into the table
                int tableIndex = FindTableIndex(c);
                return table[state, tableIndex];
            }
        }

        private int FindTableIndex(char c)
        {
            for (int i = 0; i < inputRangeEnds.Length; ++i)
            {
                // If the character is less or equal to the end of the input range,
                // return the index
                if (c <= inputRangeEnds[i])
                {
                    return i;
                }
            }
            throw new LexerConstructionException("Input ranges are unterminated." +
                                                 "This should never happen and is a bug. Please report this as an issue.");
        }

        public Tuple<int, Func<string, T>> GetAction(int state)
        {
            return actions[state];
        }
    }
}