using Piglet.Parser.Configuration;

namespace Piglet.Parser.Construction
{
    public class ShiftReduceConflictException<T> : AmbiguousGrammarException
    {
        public ShiftReduceConflictException(string message)
            : base(message)
        {
        }

        public ISymbol<T> ShiftSymbol { get; set; }
        public ISymbol<T> ReduceSymbol { get; set; }
    }
}