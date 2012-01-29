using Piglet.Parser.Configuration;

namespace Piglet.Parser.Construction
{
    public class ReduceReduceConflictException<T> : AmbiguousGrammarException
    {
        public ReduceReduceConflictException(string  message)
            : base (message)
        {
        }

        public ISymbol<T> PreviousReduceSymbol { get; set; }
        public ISymbol<T> NewReduceSymbol { get; set; }
    }
}