using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Piglet.Lexer
{
    /// <summary>
    /// A lexer that tokenizes input into tuples of tokennumber and semantic value. Lexers are not thread safe, but they are reentrant. You
    /// can reuse the same lexer by setting a new character source.
    /// </summary>
    /// <typeparam name="T">The semantic value type</typeparam>
    public interface ILexer<T>
    {
        /// <summary>
        /// Get the current state of the lexer. This is primarily for error reporting purposes
        /// </summary>
        ILexerState LexerState { get; }

        /// <summary>
        /// Gets the next token from the input stream.
        /// </summary>
        /// <returns>A tuple where firstitem is token number, and second item is the tokens semantic value. If the 
        /// end of input is reached the lexer will return the configuration given end of input token number and default(T) as the
        /// semantic value</returns>
        /// <throws>LexerException if illegal characters are detected</throws>
        Tuple<int, T> Next();

        /// <summary>
        /// Set the source of the lexer.
        /// </summary>
        /// <param name="reader">TextReader to read from</param>
        void SetSource(TextReader reader);

        /// <summary>
        /// Set the source of the lexer. This method is the same as writing SetSource(new StringReader(source))
        /// </summary>
        /// <param name="source">Source string to read from</param>
        void SetSource(string source);
    }
}