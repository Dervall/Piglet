using System;

namespace Piglet.Lexer
{
    /// <summary>
    /// LexerExceptions are thrown when the lexer cannot make sense of the current input.
    /// </summary>
    public sealed class LexerException
        : Exception
    {
        /// <summary>
        /// The current line number of the document the lexer is scanning.
        /// </summary>
        public int LineNumber { get; internal set; }

        /// <summary>
        /// The contents of the current line so far of the current document
        /// </summary>
        public string LineContents { get; internal set; }

        /// <summary>
        /// The current character index (one-based).
        /// </summary>
        public int CharacterIndex { get; internal set; }

        /// <summary>
        /// Construct a new LexerException
        /// </summary>
        /// <param name="message">Message to display</param>
        public LexerException(string message)
            : base(message)
        {
        }
    }
}