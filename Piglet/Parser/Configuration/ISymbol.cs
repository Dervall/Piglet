namespace Piglet.Parser.Configuration
{
    public interface ISymbol
    {
        /// <summary>
        /// DebugName is exclusively used for debugging purposes, as the name implies.
        /// Setting the debug name gives an easier-to-read error reporting when a parser
        /// configuration fails, but it is entirely optional to set this.
        /// </summary>
        string? DebugName { get; set; }
    }

    /// <summary>
    /// Base class of symbols in the grammar
    /// </summary>
    /// <typeparam name="T">Semantic token value type</typeparam>
    public interface ISymbol<T>
        : ISymbol
    {
    }
}