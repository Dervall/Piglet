using System.Collections.Generic;
using System.Linq;
using System;

using Piglet.Lexer.Construction;
using Piglet.Common;

namespace Piglet.Lexer.Runtime
{
    internal sealed class TransitionTable<T>
    {
        private readonly (int number, Func<string, T>? action)?[] _actions;
        private readonly char[] _inputRangeEnds;
        private readonly int[] _asciiIndices;
        private readonly ITable2D _table;


        public TransitionTable(DFA dfa, IList<NFA> nfas, IList<(string regex, Func<string, T> action)> tokens)
        {
            // Get a list of all valid input ranges that are distinct.
            // This will fill up the entire spectrum from 0 to max char
            // Sort these ranges so that they start with the lowest to highest start
            List<CharRange> allValidRanges =
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
                // Add a range that goes from \0 to the character before start
                allValidRanges.Insert(0, new CharRange { From = '\0', To = (char)(start - 1) });

            char end = allValidRanges.Last().To;

            if (end != char.MaxValue)
                allValidRanges.Add(new CharRange { From = (char)(end + 1), To = char.MaxValue });

            // Create a 2D table
            // First dimension is the number of states found in the DFA
            // Second dimension is number of distinct character ranges
            short[,] uncompressed = new short[dfa.States.Count(),allValidRanges.Count()];

            // Fill table with -1
            for (int i = 0; i < dfa.States.Count(); ++i)
                for (int j = 0; j < allValidRanges.Count(); ++j)
                    uncompressed[i, j] = -1;

            // Save the ends of the input ranges into an array
            _inputRangeEnds = allValidRanges.Select(f => f.To).ToArray();
            _actions = new (int, Func<string, T>?)?[dfa.States.Count];

            foreach (DFA.State state in dfa.States)
            {
                // Store to avoid problems with modified closure
                DFA.State state1 = state;

                foreach (Transition<DFA.State> transition in dfa.Transitions.Where(f => f.From == state1))
                    // Set the table entry
                    foreach (CharRange range in transition.ValidInput.Ranges)
                    {
                        int ix = allValidRanges.BinarySearch(range);

                        uncompressed[state.StateNumber, ix] = (short) transition.To.StateNumber;
                    }

                // If this is an accepting state, set the action function to be
                // the FIRST defined action function if multiple ones match
                if (state.NfaStates.Any(f => f.AcceptState))
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
                            _actions[state.StateNumber] = tokenNumber >= tokens.Count() ? (int.MinValue, null) : (tokenNumber, tokens[tokenNumber].action);

                            break;
                        }
                    }
            }

            _table = new CompressedTable(uncompressed);
            _asciiIndices = new int[256];

            for (int i = 0; i < _asciiIndices.Length; ++i)
                _asciiIndices[i] = FindTableIndexFromRanges((char)i);
        }

        // Determine the corrent input range index into the table
        public int this[int state, char c] => _table[state, FindTableIndex(c)];

        private int FindTableIndex(char c) => c < _asciiIndices.Length ? _asciiIndices[c] : FindTableIndexFromRanges(c);

        private int FindTableIndexFromRanges(char c)
        {
            int ix = Array.BinarySearch(_inputRangeEnds, c);

            if (ix < 0)
                ix = ~ix;

            return ix;
        }

        public (int number, Func<string, T>? action)? GetAction(int state) => _actions[state];
    }
}