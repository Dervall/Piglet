using Piglet.Parser.Construction;
using Piglet.Lexer.Runtime;
using Piglet.Lexer;

namespace Piglet.Parser
{
    /// <summary>
    /// This interface describes a Piglet generated parser.
    /// </summary>
    /// <typeparam name="T">The semantic token value type</typeparam>
    public interface IParser<T>
    {
        /// <summary>
        /// Gets or sets the lexer associated with the parser.
        /// </summary>
        ILexer<T> Lexer { get; set; }

        /// <summary>
        /// Get the internal parse table for this parser.
        /// </summary>
        IParseTable<T> ParseTable { get; }

        /// <summary>
        /// Parse an input string, returning the resulting semantic value type that is left on the parse
        /// stack.
        /// </summary>
        /// <param name="input">Input string to parse</param>
        /// <returns>The resulting semantic value symbol</returns>
        T Parse(string input);

        LexedToken<T> ParseTokens(string input);
    }
}