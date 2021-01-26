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
        public int LineNumber { get; }

        /// <summary>
        /// The contents of the current line so far of the current document.
        /// </summary>
        public string LineContents { get; }

        /// <summary>
        /// The current character index inside the current line (zero-based).
        /// </summary>
        public int CharacterIndex { get; }

        /// <summary>
        /// The current character index in the input text (zero-based).
        /// </summary>
        public int CurrentAbsoluteIndex { get; }

        /// <summary>
        /// The lexed input string.
        /// </summary>
        public string Input { get; }


        /// <summary>
        /// Construct a new LexerException
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="lineNumber">The current line number of the document the lexer is scanning.</param>
        /// <param name="lineContents">The contents of the current line so far of the current document.</param>
        /// <param name="characterIndex">The current character index inside the current line (zero-based).</param>
        /// <param name="currentAbsoluteIndex">The current character index in the input text (zero-based).</param>
        /// <param name="input">The lexed input string.</param>
        internal LexerException(string message, int lineNumber, string lineContents, int characterIndex, int currentAbsoluteIndex, string input)
            : base(message)
        {
            LineNumber = lineNumber;
            LineContents = lineContents;
            CharacterIndex = characterIndex;
            CurrentAbsoluteIndex = currentAbsoluteIndex;
            Input = input;
        }
    }
}