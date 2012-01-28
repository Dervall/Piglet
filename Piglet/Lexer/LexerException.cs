using System;

namespace Piglet.Lexer
{
    public class LexerException : Exception
    {
        public LexerException(string message)
            : base(message)
        {
        }
    }
}