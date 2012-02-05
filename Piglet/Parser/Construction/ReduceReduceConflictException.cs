using Piglet.Parser.Configuration;

namespace Piglet.Parser.Construction
{
    /// <summary>
    /// A reduce reduce conflict is thrown if the parser configuration is ambiguous so that multiple reduce actions are valid
    /// at the same points. This is usually indicative of a serious grammar error.
    /// </summary>
    /// <typeparam name="T">Semantic value of symbols used in the grammar</typeparam>
    public class ReduceReduceConflictException<T> : AmbiguousGrammarException
    {
        /// <summary>
        /// Create a new reduce reduce conflict exception
        /// </summary>
        /// <param name="message">Exception message</param>
        public ReduceReduceConflictException(string message)
            : base (message)
        {
        }

        /// <summary>
        /// The reduce symbol that existed in the parse table before the new reduce symbol was applied.
        /// </summary>
        public ISymbol<T> PreviousReduceSymbol { get; internal set; }

        /// <summary>
        /// The reduce symbol that the parser generator tried to apply.
        /// </summary>
        public ISymbol<T> NewReduceSymbol { get; internal set; }
    }
}