using System;

using Piglet.Lexer.Runtime;
using Piglet.Parser.Configuration;

namespace Piglet.Parser.Construction
{
    /// <summary>
    /// A rule which can be applied on a reduction.
    /// </summary>
    /// <typeparam name="T">Parser value type</typeparam>
    public interface IReductionRule<T>
    {
        /// <summary>
        /// The non-terminal symbol, to which the current rule will be reduced.
        /// </summary>
        INonTerminal<T> ReductionSymbol { get; }

        /// <summary>
        /// Number of tokens to pop from the parsing stack when rule is applied.
        /// </summary>
        int NumTokensToPop { get; }

        /// <summary>
        /// The token number of the resulting symbol to push on the parse stack.
        /// </summary>
        int TokenToPush { get; }

        /// <summary>
        /// The reduction function to apply. This may also handle an exception in the case of error recovery. The exception parameter will be null if no error has occurred.
        /// </summary>
        Func<ParseException, LexedToken<T>[], T> OnReduce { get; }
    }
}