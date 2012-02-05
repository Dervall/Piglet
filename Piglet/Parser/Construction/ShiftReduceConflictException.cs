using Piglet.Parser.Configuration;

namespace Piglet.Parser.Construction
{
    /// <summary>
    /// A shift reduce conflict exception is thrown by the parser generator when the grammar is
    /// ambiguous in such a way that the parser cannot decide if to shift another token or to reduce
    /// by a given rule.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ShiftReduceConflictException<T> : AmbiguousGrammarException
    {
        /// <summary>
        /// Construct a new shift reduce exception
        /// </summary>
        /// <param name="message">Exception message</param>
        public ShiftReduceConflictException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// The shift symbol in the conflict
        /// </summary>
        public ISymbol<T> ShiftSymbol { get; internal set; }

        /// <summary>
        /// The reduce symbol in the conflict
        /// </summary>
        public ISymbol<T> ReduceSymbol { get; internal set; }
    }
}