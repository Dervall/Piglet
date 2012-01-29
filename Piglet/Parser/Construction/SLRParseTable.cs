using System;
using System.Collections.Generic;

namespace Piglet.Parser.Construction
{
    public class SLRParseTable<T> : IParseTable<T>
    {
        private readonly ITable2D actionTable;
        private readonly ITable2D gotoTable;

        public SLRParseTable()
        {
            actionTable = new SparseDictionaryTable();
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
                            // TODO: Specify what sort of exception this is
                            // TODO: based on whatever was in the table and what 
                            // TODO: we tried to put in it.
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

        public static int Shift(int stateToChangeTo)
        {
            // Shift is positive integers
            return stateToChangeTo;
        }

        public ITable2D Action
        {
            get { return actionTable; }
        }

        public ITable2D Goto
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