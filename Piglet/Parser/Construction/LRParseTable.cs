using System.Collections.Generic;
using Piglet.Common;

namespace Piglet.Parser.Construction
{
    internal class LRParseTable<T> : IParseTable<T>
    {
        public ITable2D Action { get; internal set; }
        public ITable2D Goto { get { return gotoTable; } }
        public ReductionRule<T>[] ReductionRules { get; set; }

        private readonly ITable2D gotoTable;

        public LRParseTable()
        {
            gotoTable = new SparseDictionaryTable();
        }

        private class SparseDictionaryTable : ITable2D
        {
            private readonly IDictionary<int, IDictionary<int, int>> table;

            public SparseDictionaryTable()
            {
                table = new Dictionary<int, IDictionary<int, int>>();
            }

            public int this[int stateNumber, int tokenNumber]
            {
                get
                {
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
                            // This is an error condition, find out what sort of exception it is
                            int oldValue = table[stateNumber][tokenNumber];
                            if (oldValue != value)
                            {
                                try
                                {
                                    if (oldValue < 0 && value < 0)
                                    {
                                        // Both values are reduce. Throw a reduce reduce conflict
                                        throw new ReduceReduceConflictException<T>("Grammar contains a reduce reduce conflict");
                                    }
                                    throw new ShiftReduceConflictException<T>("Grammar contains a shift reduce conflict");
                                }
                                catch (AmbiguousGrammarException ex)
                                {
                                    // Fill in more information on the error and rethrow the error
                                    ex.StateNumber = stateNumber;
                                    ex.TokenNumber = tokenNumber;
                                    ex.PreviousValue = oldValue;
                                    ex.NewValue = value;
                                    throw;
                                }
                            }
                            return;
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

        

        public static short Shift(int stateToChangeTo)
        {
            // Shift is positive integers
            return (short) stateToChangeTo;
        }
        
        public static short Reduce(int reductionRule)
        {
            // Reduce is negative integers
            // with -1 to not conflict with a possible shift to state 0
            return (short)-(reductionRule + 1);
        }

        public static short Accept()
        {
            return short.MaxValue; // Max means accept
        }
    }
}