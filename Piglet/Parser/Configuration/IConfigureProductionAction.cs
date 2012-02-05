using System;

namespace Piglet.Parser.Configuration
{
    /// <summary>
    /// Fluent configuration interface for productions
    /// </summary>
    /// <typeparam name="T">Semantic type of tokens</typeparam>
    public interface IConfigureProductionAction<T>
    {
        /// <summary>
        /// Specifies an OnReduce function to be performed when parsing applies the production rule
        /// </summary>
        /// <param name="action">Function that takes each of the elements in the given rule and returns a new element. Elements in
        /// input array are ordered the same way as in the production.</param>
        void OnReduce(Func<T[], T> action);
    }
}