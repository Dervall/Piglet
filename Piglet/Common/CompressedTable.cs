using System;
using System.Collections.Generic;
using System.Linq;

namespace Piglet.Common
{
    internal class CompressedTable : ITable2D
    {
        private readonly int[] displacement;
        private readonly short[] data;

        public CompressedTable(short[,] uncompressed)
        {
            // Create a displacement table
            var numStates = uncompressed.GetUpperBound(0) + 1;
            displacement = new int[numStates];

            var table = new List<short>();
        	var offsetHashes = new List<int>();

            // Add the first range straight away.
        	table.AddRange(StateActions(0, uncompressed));
            displacement[0] = 0;

            // For each additional state, try to match as best as possible with the existing list
            for (int state = 1; state < numStates; ++state)
            {
                var stateActions = StateActions(state, uncompressed).ToArray();
            	var hash = stateActions.Aggregate(0, (acc, x) => (acc * 397) ^ x);

                // Need to run *past* the table in order to add wholly incompatible matches
                // this will not index out of the table, so there is no need to worry.
                var tableCount = table.Count();

                for (int displacementIndex = 0; displacementIndex <= tableCount; ++displacementIndex)
                {
                	if (displacementIndex < offsetHashes.Count && offsetHashes[displacementIndex] != hash)
                	{
						continue;
                	}

                	bool spotFound = true;
                    int offset = displacementIndex;
                    foreach (var stateAction in stateActions)
                    {
                        if (offset >= tableCount)
                        {
                            // Run out of table to check, but is still OK.
                            break;
                        }
                        if (stateAction != table[offset])
                        {
                            // Not found
                            spotFound = false;
                            break;
                        }
                        ++offset;
                    }

                    // Exiting the loop, if a spot is found add the correct displacement index
                    if (spotFound)
                    {
                        displacement[state] = displacementIndex;

                        // Add to the state table as much as is needed.
                        table.AddRange(stateActions.Skip(offset - displacementIndex));

						// Add the hashes that does not exist up to the displacement index
						for (int i = offsetHashes.Count; i < displacementIndex; ++i)
						{
							var offsetHash = 0;
							for (int j = i; j < stateActions.Length; ++j )
							{
								offsetHash = (offsetHash * 397) ^ table[j];
							}
							offsetHashes.Add(offsetHash);
						}
                    	offsetHashes.Add(hash);

                    	// Break loop to process next state.
							break;
                    }
                }
            }

            data = table.ToArray();
        }

        private IEnumerable<short> StateActions(int state, short[,] uncompressed)
        {
            for (int i = 0; i <= uncompressed.GetUpperBound(1); ++i)
            {
                yield return uncompressed[state, i];
            }
        }

        public int this[int state, int input]
        {
            get { return data[displacement[state] + input]; }
            set { throw new NotImplementedException(); }
        }
    }
}