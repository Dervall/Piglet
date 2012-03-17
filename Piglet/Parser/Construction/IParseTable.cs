using Piglet.Common;

namespace Piglet.Parser.Construction
{
    /// <summary>
    /// Abstracts a parse table for use with a LR parser
    /// </summary>
    /// <typeparam name="T">Type to parse</typeparam>
    public interface IParseTable<T>
    {
        /// <summary>
        /// Get the action table for this parser
        /// </summary>
        ITable2D Action { get; }

        /// <summary>
        /// Get the goto table for this parser
        /// </summary>
        ITable2D Goto { get; }

        /// <summary>
        /// Get the reduction rules
        /// </summary>
        IReductionRule<T>[] ReductionRules { get; set; }

        /// <summary>
        /// Total number of states used by the parser
        /// </summary>
        int StateCount { get; }
    }
}