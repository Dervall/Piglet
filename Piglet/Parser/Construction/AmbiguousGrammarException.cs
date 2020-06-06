using Piglet.Parser.Configuration;

namespace Piglet.Parser.Construction
{
    /// <summary>
    /// Base class for exceptions thrown by the parser generator for ambiguous grammars.
    /// </summary>
    public class AmbiguousGrammarException
        : ParserConfigurationException
    {
        internal AmbiguousGrammarException(string message)
            : base (message)
        {
        }

        /// <summary>
        /// The state number in which the conflict occurred.
        /// </summary>
        public int StateNumber { get; internal set; }

        /// <summary>
        /// The token number that generated the conflict
        /// </summary>
        public int TokenNumber { get; internal set; }

        /// <summary>
        /// The previous value of the parsing table at the point of the conflict.
        /// </summary>
        public int PreviousValue { get; internal set; }

        /// <summary>
        /// The new value that was attempted to be written into the parse table
        /// </summary>
        public int NewValue { get; internal set; }
    }
}