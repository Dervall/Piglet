using System;

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

    public interface ILexerInstance<T> : ILexerState
    {
        /// <summary>
        /// Gets the next token from the input stream.
        /// </summary>
        /// <returns>A tuple where firstitem is token number, and second item is the tokens semantic value. If the 
        /// end of input is reached the lexer will return the configuration given end of input token number and default(T) as the
        /// semantic value</returns>
        /// <throws>LexerException if illegal characters are detected</throws>
        Tuple<int, T> Next();
    }
}