using System;
using System.Collections.Generic;

namespace Piglet.Construction
{
    public class SLRParseTable<T>
    {
        private readonly ISparseParseTable actionTable;
        private readonly ISparseParseTable gotoTable;

        public SLRParseTable()
        {
            actionTable = new SparseDictionaryTable();
            gotoTable = new SparseDictionaryTable();
        }

        private class SparseDictionaryTable : ISparseParseTable
        {
            private readonly IDictionary<int, IDictionary<int, int>> table;

            public SparseDictionaryTable()
            {
                table = new Dictionary<int, IDictionary<int, int>>();
            }

            public int this[int stateNumber, int tokenNumber]
            {
                get { 
                    // TODO: Suboptimal implementation
                    if (table.ContainsKey(stateNumber))
                    {
                        if (table[stateNumber].ContainsKey(tokenNumber))
                        {
                            return table[stateNumber][tokenNumber];
                        }
                    }
                    return int.MinValue; // No action, error action
                }
                set
                {
                    if (table.ContainsKey(stateNumber))
                    {
                        if (table[stateNumber].ContainsKey(tokenNumber))
                        {
                            // TODO: Specify what sort of exception this is
                            // TODO: based on whatever was in the table and what 
                            // TODO: we tried to put in it.
                            throw new Exception("State table conflict.");
                        }
                    }
                    else
                    {
                        table.Add(stateNumber, new Dictionary<int, int>());
                    }
                    table[stateNumber][tokenNumber] = value;
                }
            }
        }

        public static int Shift(int stateToChangeTo)
        {
            // Shift is positive integers
            // Add 1 to state
            return stateToChangeTo + 1;
        }

        public ISparseParseTable Action
        {
            get { return actionTable; }
        }

        public ISparseParseTable Goto
        {
            get { return gotoTable; }
        }

        public ReductionRule<T>[] ReductionRules { get; set; } 

        public static int Reduce(int reductionRule)
        {
            // Reduce is negative integers
            // with -1 to not conflict with a possible shift to state 0
            return -(reductionRule + 1);
        }

        public static int Accept()
        {
            return int.MaxValue; // Max means accept
        }
    }
}