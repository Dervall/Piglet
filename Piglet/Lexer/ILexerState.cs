namespace Piglet.Lexer
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILexerState
    {
        /// <summary>
        /// The current line number in the input text
        /// </summary>
        int CurrentLineNumber { get; }

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