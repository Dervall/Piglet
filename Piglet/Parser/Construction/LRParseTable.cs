using Piglet.Common;

namespace Piglet.Parser.Construction
{
    internal sealed class LRParseTable<T>
        : IParseTable<T>
    {
        public ITable2D? Action { get; internal set; }
        public ITable2D? Goto { get; internal set; }
        public IReductionRule<T>[]? ReductionRules { get; set; }
        public int StateCount { get; set; }


        public static short Shift(int stateToChangeTo) => (short)stateToChangeTo; // Shift is positive integers

        public static short Reduce(int reductionRule) => (short)-(reductionRule + 1);// Reduce is negative integers with -1 to not conflict with a possible shift to state 0

        public static short Accept() => short.MaxValue; // Max means accept
    }
}