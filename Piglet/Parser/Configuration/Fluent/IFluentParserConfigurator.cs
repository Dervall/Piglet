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

    /// <summary>
    /// A rule is a configurable rule entity.
    /// </summary>
    public interface IRule : IHideObjectMembers
    {
        /// <summary>
        /// Specify what this rule is made up by
        /// </summary>
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

    /// <summary>
    /// Specify what the rule part is made by
    /// </summary>
    public interface IRuleByConfigurator : IHideObjectMembers
    {
        /// <summary>
        /// By a literal
        /// </summary>
        /// <param name="literal">Literal to match. This is a strict literal, not a regex</param>
        /// <returns>Next</returns>
        IOptionalAsConfigurator By(string literal);

        /// <summary>
        /// By a type. Not all types are supported yet, most primitives are
        /// </summary>
        /// <typeparam name="TExpressionType">The type to follow</typeparam>
        /// <returns>Next</returns>
        IOptionalAsConfigurator By<TExpressionType>();

        /// <summary>
        /// By an expression. The expression doesn't need to be configured when
        /// this is called
        /// </summary>
        /// <param name="expression">An expression</param>
        /// <returns>Next</returns>
        IOptionalAsConfigurator By(IExpressionConfigurator expression);

        /// <summary>
        /// By another rule
        /// </summary>
        /// <param name="rule">Rule to use. This may be the same rule as the rule called
        /// on. (recursive rules)</param>
        /// <returns>Next</returns>
        IOptionalAsConfigurator By(IRule rule);

        IMaybeListNamed ByListOf<TListType>(IRule listElement);
        IMaybeListNamed ByListOf(IRule listElement);
    }

    /// <summary>
    /// Fluent interface
    /// </summary>
    public interface IOptionalAsConfigurator : IRuleSequenceConfigurator
    {
        /// <summary>
        /// Specify a rule part name, which makes it accessible for 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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