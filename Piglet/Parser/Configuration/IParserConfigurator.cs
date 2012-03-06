using System;
using Piglet.Parser.Construction;

namespace Piglet.Parser.Configuration
{
    /// <summary>
    /// This interface is the main interface for configuring a new parser in code. It is only valid in the context that is is
    /// obtained, typically in ParserFactory. If methods are called after the parser has been created no changes will be applied
    /// to the already created parser.
    /// </summary>
    /// <typeparam name="T">Semantic value of tokens</typeparam>
    public interface IParserConfigurator<T>
    {
        /// <summary>
        /// Create a new Terminal. If using the built in lexer terminals will be recognized in the order
        /// of declaration. A terminal may not be redefined using different onParse actions.
        /// </summary>
        /// <param name="regExp">Regular expression to match</param>
        /// <param name="onParse">Action to take on parsing. If null is passed the default action is f => default(T)</param>
        /// <returns>A terminal symbol</returns>
        ITerminal<T> CreateTerminal(string regExp, Func<string, T> onParse = null);
        
        /// <summary>
        /// Create a new NonTerminal. Production actions may be specified directly, or deferred until later. The
        /// latter is more typical since rules are often recursive in their nature.
        /// </summary>
        /// <returns></returns>
        INonTerminal<T> CreateNonTerminal();

        /// <summary>
        /// Additional lexer settings in addition to the settings provided by the declared terminals.
        /// </summary>
        ILexerSettings LexerSettings { get; }

        /// <summary>
        /// The error token, used for catching errors in the parsing process.
        /// </summary>
        ITerminal<T> ErrorToken { get; }

        /// <summary>
        /// Makes a group of tokens left associative at a given precedence level. If you require two or more tokens
        /// to have the same precedence you must pass both at the same time to the precedence call. If you pass
        /// the same token to a precedence function more than once you will get a ParserConfigurationException.
        /// </summary>
        /// <param name="symbols">Symbols to set associativity on</param>
        IPrecedenceGroup LeftAssociative(params ITerminal<T>[] symbols);

        /// <summary>
        /// Makes a group of tokens right associative at a given precedence level. If you require two or more tokens
        /// to have the same precedence you must pass both at the same time to the precedence call. If you pass
        /// the same token to a precedence function more than once you will get a ParserConfigurationException.
        /// </summary>
        /// <param name="symbols">Symbols to set associativity on</param>
        IPrecedenceGroup RightAssociative(params ITerminal<T>[] symbols);

        /// <summary>
        /// Makes a group of tokens non-associative at a given precedence level. If you require two or more tokens
        /// to have the same precedence you must pass both at the same time to the precedence call. If you pass
        /// the same token to a precedence function more than once you will get a ParserConfigurationException.
        /// </summary>
        /// <param name="symbols">Symbols to set associativity on</param>
        IPrecedenceGroup NonAssociative(params ITerminal<T>[] symbols);

        /// <summary>
        /// Creates a parser based on the inputted configuration. If a lexer has been desired as well, this method will also create the lexer.
        /// </summary>
        /// <returns>The created parser</returns>
        IParser<T> CreateParser();
    }
}