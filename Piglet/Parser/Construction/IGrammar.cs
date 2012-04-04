using System.Collections.Generic;
using Piglet.Parser.Configuration;

namespace Piglet.Parser.Construction
{
    internal interface IGrammar<T>
    {
        IProductionRule<T> Start { get; }
        IEnumerable<IProductionRule<T>> ProductionRules { get; }
        IEnumerable<ISymbol<T>> AllSymbols { get; }
        NonTerminal<T> AcceptSymbol { get; }
        Terminal<T> EndOfInputTerminal { get; }
        ITerminal<T> ErrorToken { get; }
        IPrecedenceGroup GetPrecedence(ITerminal<T> terminal);
    }

    /// <summary>
    /// Defines the associativities that can be set for a given token type.
    /// </summary>
    public enum AssociativityDirection
    {
        /// <summary>
        /// Left associative
        /// </summary>
        Left,

        /// <summary>
        /// Right associative
        /// </summary>
        Right,

        /// <summary>
        /// Non-associative
        /// </summary>
        NonAssociative
    };

    /// <summary>
    /// Represent a group of symbols that have a given precedence level and associativity set.
    /// This interface is also the means to set context dependence precendence.
    /// </summary>
    public interface IPrecedenceGroup
    {
        /// <summary>
        /// Get the associativity that the precedence group was created by
        /// </summary>
        AssociativityDirection Associativity { get; }

        /// <summary>
        /// Get the precedence level
        /// </summary>
        int Precedence { get; }
    }
}
