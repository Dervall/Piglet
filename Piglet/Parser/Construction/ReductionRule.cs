using System;

namespace Piglet.Parser.Construction
{
    internal class ReductionRule<T>
    {
        public int NumTokensToPop { get; set; }
        public int TokenToPush { get; set; }
        public Func<ParseException, T[], T> OnReduce { get; set; }
    }
}