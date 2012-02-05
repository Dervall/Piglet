using System;

namespace Piglet.Parser.Configuration
{
    /// <summary>
    /// A terminal symbol in the grammar. A terminal symbol may not contain production rules.
    /// If a lexer is desired, Piglet will generate lexer definitions based on the regular
    /// expressions.
    /// </summary>
    /// <typeparam name="T">Semantic token value type</typeparam>
    public interface ITerminal<T> : ISymbol<T>
    {
        /// <summary>
        /// Regular expression this terminal recognizes
        /// </summary>
        string RegExp { get; }
        
        /// <summary>
        /// OnParse action to take. The input is a string which is the parsed lexeme guaranteed to match
        /// the regular expression of this terminal. Output should be an object of this parsers value
        /// type.
        /// </summary>
        Func<string, T> OnParse { get; }
    }
}