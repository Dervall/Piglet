using System;

namespace Piglet.Lexer
{
    public class LexerException : Exception
    {
        public int LineNumber { get; set; }
        public string LineContents { get; set; }

        public LexerException(string message)
            : base(message)
        {
        }
    }
}