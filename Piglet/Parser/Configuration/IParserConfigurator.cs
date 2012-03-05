using System;

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
        ITerminal<T> Terminal(string regExp, Func<string, T> onParse = null);
        
        /// <summary>
        /// Create a new NonTerminal. Production actions may be specified directly, or deferred until later. The
        /// latter is more typical since rules are often recursive in their nature.
        /// </summary>
        /// <param name="productionAction">Specifies a production action directly.</param>
        /// <returns></returns>
        INonTerminal<T> NonTerminal(Action<IProductionConfigurator<T>> productionAction = null);

        /// <summary>
        /// Additional lexer settings in addition to the settings provided by the declared terminals.
        /// </summary>
        ILexerSettings LexerSettings { get; }

        /// <summary>
        /// Makes a group of tokens left associative at a given precedence level. If you require two or more tokens
        /// to have the same precedence you must pass both at the same time to the precedence call. If you pass
        /// the same token to a precedence function more than once you will get a ParserConfigurationException.
        /// </summary>
        /// <param name="symbols">Symbols to set associativity on</param>
        void LeftAssociative(params ITerminal<T>[] symbols);

        /// <summary>
        /// Makes a group of tokens right associative at a given precedence level. If you require two or more tokens
        /// to have the same precedence you must pass both at the same time to the precedence call. If you pass
        /// the same token to a precedence function more than once you will get a ParserConfigurationException.
        /// </summary>
        /// <param name="symbols">Symbols to set associativity on</param>
        void RightAssociative(params ITerminal<T>[] symbols);

        /// <summary>
        /// Makes a group of tokens non-associative at a given precedence level. If you require two or more tokens
        /// to have the same precedence you must pass both at the same time to the precedence call. If you pass
        /// the same token to a precedence function more than once you will get a ParserConfigurationException.
        /// </summary>
        /// <param name="symbols">Symbols to set associativity on</param>
        void NonAssociative(params ITerminal<T>[] symbols);
    }
}