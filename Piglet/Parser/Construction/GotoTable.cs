using System.Collections.Generic;
using System.Linq;
using System;

using Piglet.Common;

namespace Piglet.Parser.Construction
{
    internal sealed class GotoTable
        : ITable2D
    {
        /// <summary>
        /// Only for input to the constructor
        /// </summary>
        public struct GotoTableValue
        {
            public int State;
            public int Token;
            public int NewState;
        };

        private readonly short[] _stateDictionary;
        private readonly short[] _gotoValues;


        public GotoTable(IList<GotoTableValue> gotos)
        {
            // Gather the most common gotos for each token.
            int maxToken = gotos.Max(f => f.Token) + 1;

            // Get the most common gotos and store them in the start
            short[] defaultGotos = GetMostCommonGotos(gotos, maxToken);

            // Iterate through the states, and find out where the default GOTOs are not applicable for those states, store an offset
            _stateDictionary = new short[gotos.Max(f => f.State) + 1]; // Need not store more than nStates+1 of the maximum referenced state

            // Holds the gotos for a given state, allocated outside of loop for performance reasons.
            short[] stateGotos = new short[maxToken];
            // Stategotos now holds only 0, which is what we want (every state points to defaultGotos)

            List<short> offsets = new List<short>(defaultGotos); // The offsets is where we will store the default gotos in the end

            foreach (int state in gotos.Select(f => f.State).Distinct())
            {
                // Assemble the goto list
                
                // Make a copy of the default gotos, this is what we hope won't change
                defaultGotos.CopyTo(stateGotos, 0);

                // For each gotoitem, set the stateGoto appropritately.
                int state1 = state;

                List<GotoTableValue> gotosForState = gotos.Where(f => f.State == state1).ToList();

                foreach (GotoTableValue gotoItem in gotosForState)
                    stateGotos[gotoItem.Token] = (short)gotoItem.NewState;

                // Compare the state gotos to the default gotos. If they are the same, don't change a thing.
                int firstMisMatchIndex = -1;
                int lastMisMatchIndex = -1;

                for (int i = 0; i < defaultGotos.Length; ++i)
                    if (stateGotos[i] != defaultGotos[i])
                    {
                        // Mismatch, we will need to create things in the gotoValues table
                        // Get the lowest and the highest token number that the goto table
                        // can be called with.
                        firstMisMatchIndex = gotosForState.Min(f => f.Token);
                        lastMisMatchIndex = gotosForState.Max(f => f.Token);
                    }

                // If we have a mismatch we need to find a match for the sublist in question.
                if (firstMisMatchIndex != -1)
                {
                    List<short> sublist = stateGotos.Skip(firstMisMatchIndex).Take(lastMisMatchIndex - firstMisMatchIndex + 1).ToList();
                    int offsetIndex = offsets.IndexOf(sublist);

                    if (offsetIndex == -1)
                    {
                        // Not found. Add entire sublist to the end
                        offsetIndex = offsets.Count;
                        offsets.AddRange(sublist);
                    }

                    // Set the offset index. This is offsetted by the first mismatch since those tokens will never be called, so they
                    // can be whatever. We're not using the entire list to look for the submatch.
                    _stateDictionary[state] = (short)(offsetIndex - firstMisMatchIndex);
                }
            }

            // Remove the list and condense into array for fast use once parsing starts
            _gotoValues = offsets.ToArray();
        }

        private static short[] GetMostCommonGotos(IEnumerable<GotoTableValue> gotos, int maxToken)
        {
            Dictionary<(int token, int state), int> gotoCounts = new Dictionary<(int token, int state), int>();
            short[] defaultGotos = new short[maxToken];

            foreach (GotoTableValue g in gotos)
            {
                (int, int) t = (g.Token, g.NewState);

                if (!gotoCounts.ContainsKey(t))
                    gotoCounts.Add(t, 0);

                ++gotoCounts[t];
            }

            List<int> unassigned = new List<int>();

            // For every token in the grammar, store the most stored count as the default goto
            for (int t = 0; t < maxToken; ++t)
            {
                int[] def = (from f in gotoCounts
                             where f.Key.token == t
                             orderby -f.Value
                             select f.Key.state).ToArray();

                if (def.Length == 0)
                    unassigned.Add(t);
                else
                    defaultGotos[t] = (short)def[0];
            }

            foreach (int t in unassigned)
                defaultGotos[t] = (from f in gotoCounts
                                   let i2 = (short)f.Key.state
                                   where !defaultGotos.Contains(i2)
                                   orderby -f.Value
                                   select i2).FirstOrDefault();

            return defaultGotos;
        }

        public int this[int state, int input]
        {
            get
            {
                // This check is really unneccessary since the parser will never access outside of the legal state list
                // but for now the debug printer will. So that is why we check for the state bounds
                if (state >= _stateDictionary.Length)
                    return short.MinValue; // Nothing to see here.

                // Index into goto values.
                int offsetIndex = _stateDictionary[state] + input;

                // Also an unnecessary check if it wasn't for the debugging feature
                if (offsetIndex >= _gotoValues.Length)
                    return short.MinValue;

                return _gotoValues[offsetIndex];
            }
        }
    }
}