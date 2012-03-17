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
        IRuleByConfigurator IsMadeUp { get; }
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

    public interface IRuleByConfigurator : IHideObjectMembers
    {
        IOptionalAsConfigurator By(string literal);
        IOptionalAsConfigurator By<TExpressionType>();
        IOptionalAsConfigurator By(IExpressionConfigurator expression);
        IOptionalAsConfigurator By(IRule rule);

        IMaybeListNamed ByListOf<TListType>(IRule listElement);
        IMaybeListNamed ByListOf(IRule listElement);
    }

    public interface IOptionalAsConfigurator : IRuleSequenceConfigurator
    {
        IRuleSequenceConfigurator As(string name);
    }

    public interface IMaybeNewRuleConfigurator : IHideObjectMembers
    {
        IRuleByConfigurator Or { get; }        
    }

    public interface IRuleSequenceConfigurator : IMaybeNewRuleConfigurator
    {
        IRuleByConfigurator Followed { get; }
        IMaybeNewRuleConfigurator WhenFound(Func<dynamic, object> func);
    }

    public interface IMaybeListNamed : IListRuleSequenceConfigurator
    {
        IListRuleSequenceConfigurator As(string name);
    }

    public interface IListRuleSequenceConfigurator : IRuleSequenceConfigurator
    {
        IListItemConfigurator ThatIs { get; }
    }

    public interface IListItemConfigurator : IRuleSequenceConfigurator
    {
        IListItemConfigurator SeparatedBy(string separator);
        IListItemConfigurator Optional { get; }
    }
}