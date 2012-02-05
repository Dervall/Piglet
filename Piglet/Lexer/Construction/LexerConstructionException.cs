using System;

namespace Piglet.Lexer.Construction
{
    /// <summary>
    /// Class of exceptions that may occur when creating a Lexer.
    /// </summary>
    public class LexerConstructionException : Exception
    {
        public LexerConstructionException(string message)
            : base(message)
        {
        }
    }
}