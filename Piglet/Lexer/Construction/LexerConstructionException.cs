using System;

namespace Piglet.Lexer.Construction
{
    /// <summary>
    /// Class of exceptions that may occur when creating a Lexer.
    /// </summary>
    public class LexerConstructionException : Exception
    {
        /// <summary>
        /// Construct a new LexerConstructionException
        /// </summary>
        /// <param name="message">Message to show</param>
        public LexerConstructionException(string message)
            : base(message)
        {
        }
    }
}