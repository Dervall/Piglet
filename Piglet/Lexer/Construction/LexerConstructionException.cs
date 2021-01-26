using System;

namespace Piglet.Lexer.Construction
{
    /// <summary>
    /// Class of exceptions that may occur when creating a Lexer.
    /// </summary>
    public sealed class LexerConstructionException
        : Exception
    {
        /// <summary>
        /// Construct a new LexerConstructionException
        /// </summary>
        /// <param name="message">Message to show</param>
        public LexerConstructionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Construct a new LexerConstructionException
        /// </summary>
        /// <param name="message">Message to show</param>
        /// <param name="innerException">Inner exception</param>
        public LexerConstructionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}