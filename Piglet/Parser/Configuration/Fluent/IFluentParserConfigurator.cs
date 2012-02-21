namespace Piglet.Parser.Configuration.Fluent
{
    public interface IFluentParserConfigurator
    {
        IRule Rule();
        IExpressionConfigurator Expression();
        IExpressionConfigurator QuotedString { get; }
        IParser<object> CreateParser();
    }

    public interface IRule
    {
        IRuleByConfigurator IsMadeUp { get; }
    }

    public interface IExpressionConfigurator
    {
        IExpressionConfigurator ThatMatches<TExpressionType>();
        IExpressionConfigurator ThatMatches(string regex);
    }

    public interface IRuleByConfigurator
    {
        IRuleSequenceConfigurator By(string literal);
        IRuleSequenceConfigurator By<TExpressionType>();
        IRuleSequenceConfigurator By(IExpressionConfigurator expression);
        IRuleSequenceConfigurator By(IRule rule);
        IListRuleSequenceConfigurator ByListOf(IRule listElement);
    }

    public interface IRuleSequenceConfigurator
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