using System;

namespace Piglet.Parser.Construction
{
    public interface IReductionRule<T>
    {
        int NumTokensToPop { get; }
        int TokenToPush { get; }
        Func<ParseException, T[], T> OnReduce { get; }
    }
}