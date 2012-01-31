using System;

namespace Piglet.Lexer.Construction
{
    public class LexerConstructionException : Exception
    {
        public LexerConstructionException(string message)
            : base(message)
        {
        }
    }
}