using System;

namespace Piglet.Parser.Configuration.Fluent
{
    public interface IFluentParserConfigurator : IHideObjectMembers
    {
        IRule Rule();
        IExpressionConfigurator Expression();
        IExpressionConfigurator QuotedString { get; }
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
        IMaybeListNamed ByListOf(IRule listElement);
    }

    public interface IOptionalAsConfigurator : IRuleSequenceConfigurator
    {
        IRuleSequenceConfigurator As(string name);
    }

    public interface IRuleSequenceConfigurator : IHideObjectMembers
    {
        IRuleByConfigurator Or { get; }
        IRuleByConfigurator Followed { get; }
        IRuleByConfigurator WhenFound(Func<dynamic, object> func);
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