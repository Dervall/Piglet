using System;
using Piglet.Lexer;

namespace Piglet.Parser
{
    /// <summary>
    /// ParseExceptions are thrown when the parser detects an illegal token according to the given
    /// grammar.
    /// </summary>
    public class ParseException : Exception
    {
        /// <summary>
        /// Current state of the lexer
        /// </summary>
        public ILexerState LexerState { get; internal set; }

        /// <summary>
        /// Construct a new Parseexception
        /// </summary>
        /// <param name="message"></param>
        public ParseException(string message)
            : base(message)
        {
        }
    }
}