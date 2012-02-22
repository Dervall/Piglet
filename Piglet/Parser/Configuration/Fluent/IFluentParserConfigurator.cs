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
        IExpressionConfigurator ThatMatches<TExpressionType>();
        IExpressionConfigurator ThatMatches(string regex);
    }

    public interface IRuleByConfigurator : IHideObjectMembers
    {
        IRuleSequenceConfigurator By(string literal);
        IRuleSequenceConfigurator By<TExpressionType>();
        IRuleSequenceConfigurator By(IExpressionConfigurator expression);
        IRuleSequenceConfigurator By(IRule rule);
        IListRuleSequenceConfigurator ByListOf(IRule listElement);
    }

    public interface IRuleSequenceConfigurator : IHideObjectMembers
    {
        IRuleByConfigurator Or { get; }
        IRuleByConfigurator Followed { get; }
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