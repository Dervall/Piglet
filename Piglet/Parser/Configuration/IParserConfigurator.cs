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
        /// of declaration, unless the topPrecedence is set. A top precedence will be recognized before all other
        /// declared terminals. A terminal may not be redefined using different onParse actions.
        /// </summary>
        /// <param name="regExp">Regular expression to match</param>
        /// <param name="onParse">Action to take on parsing. If null is passed the default action is f => default(T)</param>
        /// <param name="topPrecedence">If true, this terminal takes precedence over previously created terminals</param>
        /// <returns>The newly created terminal symbol.</returns>
        ITerminal<T> CreateTerminal(string regExp, Func<string, T>? onParse = null, bool topPrecedence = false);

        /// <summary>
        /// Creates a new non-terminal. Production actions may be specified directly, or deferred until later. The
        /// latter is more typical since rules are often recursive in their nature.
        /// </summary>
        /// <returns>The newly created non-terminal symbol.</returns>
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
        /// Sets the given non-terminal symbol as start symbol
        /// </summary>
        /// <param name="start">Non-terminal symbol</param>
        void SetStartSymbol(INonTerminal<T> start);

        /// <summary>
        /// Creates a parser based on the inputted configuration. If a lexer has been desired as well, this method will also create the lexer.
        /// </summary>
        /// <returns>The created parser</returns>
        IParser<T> CreateParser();
    }

    /// <summary>
    /// A static class containing generic extension methods for <see cref="IParserConfigurator{T}"/>.
    /// </summary>
    public static class Extensions
    {
        /// <inheritdoc cref="IParserConfigurator{T}.CreateTerminal"/>
        /// <typeparam name="T">Semantic value of tokens.</typeparam>
        /// <param name="conf">The parser configurator.</param>
        /// <param name="regex">Regular expression to match.</param>
        /// <param name="val">The value stored inside the terminal symbol.</param>
        /// <param name="topPrecedence">If true, this terminal takes precedence over previously created terminals.</param>
        /// <returns>The newly created terminal symbol.</returns>
        public static ITerminal<T> CreateTerminal<T>(this IParserConfigurator<T> conf, string regex, T val, bool topPrecedence = false) =>
            conf.CreateTerminal(regex, _ => val, topPrecedence);

        /// <inheritdoc cref="IParserConfigurator{T}.CreateNonTerminal"/>
        /// <typeparam name="T">Semantic value of tokens.</typeparam>
        /// <param name="conf">The parser configurator.</param>
        /// <param name="name">The non-terminal symbol's name.</param>
        /// <returns>The newly created non-terminal symbol.</returns>
        public static INonTerminal<T> CreateNonTerminal<T>(this IParserConfigurator<T> conf, string name)
        {
            INonTerminal<T> nter = conf.CreateNonTerminal();

            nter.DebugName = name;

            return nter;
        }

        /// <summary>
        /// Creates a new reduce production on the given non-terminal symbol.
        /// <para/>
        /// The <paramref name="args"/> parameter may contains either
        /// previously declared symbols of the grammar or strings, which are interpreted as terminals
        /// which may be given unescaped as per the lexer settings of the main configurator object.
        /// If an empty rule is desired you may pass no parameters to the Production.
        /// </summary>
        /// <typeparam name="T">Semantic value of tokens.</typeparam>
        /// <param name="symb">The non-terminal symbol.</param>
        /// <param name="args">Parts of rule to configure the production</param>
        /// <returns>A production configurator for the created production, for addition configuration.</returns>
        public static IProduction<T> AddReduceProduction<T>(this INonTerminal<T> symb, params object[] args)
        {
            IProduction<T> prod = symb.AddProduction(args);

            prod.SetReduceToFirst();

            return prod;
        }
    }
}