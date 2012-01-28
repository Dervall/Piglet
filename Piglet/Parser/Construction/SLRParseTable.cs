using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Piglet.Configuration;

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
                            if (table[stateNumber][tokenNumber] != value)
                                throw new Exception("State table conflict.");
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

        public string ToDebugString(IGrammar<T> grammar, int numStates)
        {
            int numTokens = grammar.AllSymbols.Count();
            int numTerminals = grammar.AllSymbols.OfType<Terminal<T>>().Count();

            var formatString = new StringBuilder("{0,8}|");
            for (int i = 0; i < numTokens; ++i)
            {
                if (i == numTerminals)
                    formatString.Append("|"); // Extra bar to separate actions and gotos
                formatString.Append("|{" + (i + 1) + ",8}");
            }
            formatString.Append("|\n");
            string format = formatString.ToString();
            var sb = new StringBuilder();
            sb.Append(string.Format(format, new[] { "STATE" }.Concat(grammar.AllSymbols.Select(f => f.DebugName)).ToArray<object>()));
            for (int i = 0; i < numStates; ++i)
            {
                object[] formatParams = new[] {i.ToString()}.Concat(grammar.AllSymbols.OfType<Terminal<T>>().Select(f =>
                    {
                        var actionValue = actionTable[i, f.TokenNumber];
                        if (actionValue == int.MaxValue)
                        {
                            return "acc";
                        }

                        if (actionValue == int.MinValue)
                        {
                            return "";
                        }

                        if (actionValue < 0)
                        {
                            return "r" + -(actionValue + 1);
                        }

                        return "s" + actionValue;
                    }).Concat(grammar.AllSymbols.OfType<NonTerminal<T>>().Select(f => Goto[i, f.TokenNumber] ==
                                                                                      int.MinValue
                                                                                          ? ""
                                                                                          : Goto[i, f.TokenNumber].ToString()))).ToArray<object>();
                sb.Append(string.Format(format, formatParams));
            }
            return sb.ToString();
        }
    }
}