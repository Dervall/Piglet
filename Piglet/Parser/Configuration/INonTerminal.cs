namespace Piglet.Parser.Configuration
{
    public interface INonTerminal
        : ISymbol
    {
    }

    /// <summary>
    /// A non terminal in a given grammar, which may be configured to have productions.
    /// </summary>
    /// <typeparam name="T">Semantic value of tokens in the grammar</typeparam>
    public interface INonTerminal<T>
        : ISymbol<T>
        , INonTerminal
    {
        /// <summary>
        /// Creates a production on a given non-terminal. The parts parameter may contains either
        /// previously declared symbols of the grammar or strings, which are interpreted as terminals
        /// which may be given unescaped as per the lexer settings of the main configurator object.
        /// If an empty rule is desired you may pass no parameters to the Production. Null must not be passed.
        /// </summary>
        /// <param name="parts">Parts of rule to configure the production</param>
        /// <returns>A production configurator for the created production, for addition configuration.</returns>
        IProduction<T> AddProduction(params object[] parts);
    }
}