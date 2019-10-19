using Piglet.Parser.Configuration;

namespace Piglet.Parser.Construction
{
    /// <summary>
    /// A shift reduce conflict exception is thrown by the parser generator when the grammar is
    /// ambiguous in such a way that the parser cannot decide if to shift another token or to reduce
    /// by a given rule.
    /// </summary>
    public sealed class ShiftReduceConflictException<T>
        : AmbiguousGrammarException
    {
        /// <summary>
        /// The shift symbol in the conflict
        /// </summary>
        public ISymbol<T> ShiftSymbol { get; }

        /// <summary>
        /// The reduce symbol in the conflict
        /// </summary>
        public ISymbol<T> ReduceSymbol { get; }


        /// <summary>
        /// Construct a new shift reduce exception
        /// </summary>
        public ShiftReduceConflictException(ISymbol<T> shift, ISymbol<T> reduce)
            : base($"The grammar contains a shift-reduce conflict.\nShift symbol: {shift}\nReduce symbol: {reduce}\nDid you forget to set an associativity/precedence?")
        {
            ShiftSymbol = shift;
            ReduceSymbol = reduce;
        }
    }
}