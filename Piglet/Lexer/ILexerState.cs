namespace Piglet.Lexer
{
    /// <summary>
    /// Current state of the lexer
    /// </summary>
    public interface ILexerState
    {
        /// <summary>
        /// The current line number in the input text (one-based).
        /// </summary>
        int CurrentLineNumber { get; }

        /// <summary>
        /// The current character index in the input text (zero-based).
        /// </summary>
        int CurrentAbsoluteIndex { get; }

        /// <summary>
        /// The current character index inside the current line (one-based).
        /// </summary>
        int CurrentCharacterIndex { get; }

        /// <summary>
        /// The contents so far of the current line
        /// </summary>
        string CurrentLine { get; }

        /// <summary>
        /// Get the last lexeme found by the lexer.
        /// </summary>
        string LastLexeme { get; }
    }
}