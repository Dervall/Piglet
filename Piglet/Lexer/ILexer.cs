using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Piglet.Lexer
{
	public interface ILexer<in TContext, T>
	{
		ILexerInstance<TContext, T> Begin(TextReader reader);
		ILexerInstance<TContext, T> Begin(string source);
		IEnumerable<Tuple<int, T>> Tokenize(TContext context, string source);
	}

	/// <summary>
    /// A lexer that tokenizes input into tuples of tokennumber and semantic value. Lexers are reentrant and thread safe.
    /// </summary>
    /// <typeparam name="T">The semantic value type</typeparam>
    public interface ILexer<T>
    {
        /// <summary>
        /// Begin lexing a text
        /// </summary>
        /// <param name="reader">TextReader to read from</param>
        ILexerInstance<T> Begin(TextReader reader);

        /// <summary>
        /// Begin lexing a string. This method is the same as writing Begin(new StringReader(source))
        /// </summary>
        /// <param name="source">Source string to read from</param>
        ILexerInstance<T> Begin(string source);

        /// <summary>
        /// Tokenize a string
        /// </summary>
        /// <param name="source">Input string to tokenize</param>
        /// <returns></returns>
        IEnumerable<Tuple<int, T>> Tokenize(string source);
    }
}