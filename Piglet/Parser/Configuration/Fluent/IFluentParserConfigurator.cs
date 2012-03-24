using System;

namespace Piglet.Parser.Configuration.Fluent
{
    /// <summary>
    /// A configuration object for creating fluently configured parsers.
    /// </summary>
    public interface IFluentParserConfigurator : IHideObjectMembers
    {
        /// <summary>
        /// Create a new rule
        /// </summary>
        /// <returns>A new rule</returns>
        IRule Rule();

        /// <summary>
        /// Create a new expression
        /// </summary>
        /// <returns>A new expression</returns>
        IExpressionConfigurator Expression();

        /// <summary>
        /// Ready-made expression for quoted strings.
        /// </summary>
        IExpressionConfigurator QuotedString { get; }

        /// <summary>
        /// Create the parser based on the configuration used
        /// </summary>
        /// <returns></returns>
        IParser<object> CreateParser();
    }

    public interface IRule : IHideObjectMembers
    {
        RuleSyntaxState IsMadeUp { get; }
    }

    public interface IExpressionConfigurator : IHideObjectMembers
    {
        IExpressionReturnConfigurator ThatMatches<TExpressionType>();
        IExpressionReturnConfigurator ThatMatches(string regex);
    }

    public interface IExpressionReturnConfigurator : IHideObjectMembers
    {
        void AndReturns(Func<string, object> func);
    }
}