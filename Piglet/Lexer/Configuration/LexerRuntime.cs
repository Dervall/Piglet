namespace Piglet.Lexer.Configuration
{
    /// <summary>
    /// This enumeration specifies what runtime the lexer should use. This is a tradeoff between construction speed
    /// and runtime speed.    
    /// </summary>
    public enum LexerRuntime
    {
        /// <summary>
        /// Tabular is the slowest to construct but the fastest to run. Lexers built this way will use an internal table
        /// to perform lookups. Use this method if you only construct your lexer once and reuse it continually or parsing very large texts.
        /// Time complexity is O(1) - regardless of input size or grammar size. Memory usage is constant, but might incur a larger memory use than other methods for small grammars.
        /// </summary>
        Tabular,

        /// <summary>
        /// Nfa means that the lexer will run as a non-finite automata. This method of constructing is VERY fast but slower to run.
        /// Also, the lexing performance is not linear and will vary based on the configuration of your grammar. Initially uses less memory
        /// than a tabular approach, but might increase memory usage as the lexing proceeds.
        /// </summary>
        Nfa,

        /// <summary>
        /// Runs the lexing algorithm as a deterministic finite automata. This method of construction is slower than NFA, but faster than Tabular.
        /// It runs in finite memory and has a complexity of O(1) but a slower run time and more memory usage than a tabular lexer.
        /// </summary>
        Dfa,
    };
}