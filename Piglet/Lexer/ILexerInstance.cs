using System;

namespace Piglet.Lexer
{
	public interface ILexerInstance<in TContext, T> : ILexerState
	{
		Tuple<int, T> Next(TContext context);
	}

	/// <summary>
	/// A running instance of a lexer containing the lexer state
	/// </summary>
	/// <typeparam name="T">Return type of the lexer tokens</typeparam>
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