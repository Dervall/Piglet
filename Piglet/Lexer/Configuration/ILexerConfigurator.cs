using System;

namespace Piglet.Lexer.Configuration
{
    /// <summary>
    /// ILexerConfigurator is the main configuration interface used to configure the lexer behaviour programmatically. This
    /// is used inside the LexerFactory.Configure method and should not be retained after the lexer has been created.
    /// Further modifying the LexerConfigurator after a lexer has been created will not result in modifications to the lexer.
    /// </summary>
    /// <typeparam name="T">The output type of semantic values given by the token action method</typeparam>
    public interface ILexerConfigurator<in T>
    {
        /// <summary>
        /// Register a token in the lexer. The tokens will be recognized in order of declaration, i.e.
        /// an earlier declare token will be recognized over one that is declared later.
        /// </summary>
        /// <param name="regEx">The regular expression to match</param>
        /// <param name="action">Action to run when a token is matched. Input to action is matched lexeme, output should be object of T</param>
        void Token(string regEx, Func<string, T> action );

        /// <summary>
        /// Adds a regex to the list of ignored expressions. The lexer will always favour the normal tokens over ignored expressions.
        /// Ignored expressions are never reported from the Next function of the lexer.
        /// </summary>
        /// <param name="regEx">Expression to ignore</param>
        void Ignore(string regEx);
        
        /// <summary>
        /// Set the token number that will be reported when the lexer reached the end of the input stream.
        /// Default is -1
        /// </summary>
        int EndOfInputTokenNumber { get; set; }

        /// <summary>
        /// Should the resulting DFA be minimized?
        /// The default is true, and it should normally be kept that way
        /// </summary>
        bool MinimizeDfa { get; set; }
    }
}