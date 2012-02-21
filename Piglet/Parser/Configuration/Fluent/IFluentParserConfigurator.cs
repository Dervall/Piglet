namespace Piglet.Parser.Configuration.Fluent
{
    public interface IFluentParserConfigurator<T>
    {
        IRule<T> Rule();
        IExpressionConfigurator<T> Expression();
        IExpressionConfigurator<T> QuotedString { get; }
        IParser<T> CreateParser();
    }

    public interface IRule<T>
    {
        IRuleByConfigurator<T> IsMadeUp { get; } 
    }

    public interface IExpressionConfigurator<T>
    {
        IExpressionConfigurator<T> ThatMatches<TExpressionType>();
        IExpressionConfigurator<T> ThatMatches(string regex);
    }

    public interface IRuleByConfigurator<T>
    {
        IRuleSequenceConfigurator<T> By(string literal);
        IRuleSequenceConfigurator<T> By<TExpressionType>();
        IRuleSequenceConfigurator<T> By(IExpressionConfigurator<T> expression);
        IRuleSequenceConfigurator<T> By(IRule<T> rule);
        IListRuleSequenceConfigurator<T> ByListOf(IRule<T> listElement);
    }

    public interface IRuleSequenceConfigurator<T>
    {
        IRuleByConfigurator<T> Or { get; }
        IRuleByConfigurator<T> Followed { get; }
    }

    public interface IListRuleSequenceConfigurator<T> : IRuleSequenceConfigurator<T>
    {
        IListItemConfigurator<T> ThatIs { get; }
    }

    public interface IListItemConfigurator<T> : IRuleSequenceConfigurator<T>
    {
        IListItemConfigurator<T> SeparatedBy(string separator);
        IListItemConfigurator<T> Optional { get; }
    }
}