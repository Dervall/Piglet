using System;
using Piglet.Parser.Construction;

namespace Piglet.Parser.Configuration
{
    /// <summary>
    /// Fluent configuration interface for productions
    /// </summary>
    /// <typeparam name="T">Semantic type of tokens</typeparam>
    public interface IProduction<T>
    {
        /// <summary>
        /// Specifies a reduction function to be performed when parsing applies the production rule
        /// </summary>
        /// <param name="action">Function that takes each of the elements in the given rule and returns a new element. Elements in
        /// input array are ordered the same way as in the production.</param>
        void SetReduceFunction(Func<T[], T> action);

        /// <summary>
        /// Sets context dependent precedence on this rule to make it the same precedence as the given level
        /// </summary>
        /// <param name="precedenceGroup">Precedence level to use</param>
        void SetPrecedence(IPrecedenceGroup precedenceGroup);

        /// <summary>
        /// Set the error reporting function. This is only valid if the rule in question catches
        /// the Error token as predefined by the configurator.
        /// </summary>
        /// <param name="errorHandler">Error handler function</param>
        void SetErrorFunction(Func<ParseException, T[], T> errorHandler);
    }
}