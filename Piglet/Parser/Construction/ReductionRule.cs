using System;

namespace Piglet.Parser.Construction
{
    internal class ReductionRule<T>
    {
        public int NumTokensToPop { get; set; }
        public int TokenToPush { get; set; }
        public Func<T[], T> OnReduce { get; set; }
    }
}