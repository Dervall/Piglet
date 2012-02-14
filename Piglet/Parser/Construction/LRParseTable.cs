using System.Collections.Generic;
using Piglet.Common;

namespace Piglet.Parser.Construction
{
    internal class LRParseTable<T> : IParseTable<T>
    {
        public ITable2D Action { get; internal set; }
        public ITable2D Goto { get; internal set; }
        public ReductionRule<T>[] ReductionRules { get; set; }

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