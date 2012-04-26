using System;
using Piglet.Lexer.Configuration;

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
        /// The error token, use in a rule to signal an error accepting rule.
        /// </summary>
        IExpressionConfigurator Error { get; }

        /// <summary>
        /// Create the parser based on the configuration used
        /// </summary>
        /// <returns></returns>
        IParser<object> CreateParser();

        /// <summary>
        /// Group a selection of expressions as left associative
        /// </summary>
        /// <param name="p">expressions</param>
        void LeftAssociative(params object[] p);

        /// <summary>
        /// Group a selection of expressions as right associative
        /// </summary>
        /// <param name="p">expressions</param>
        void RightAssociative(params object[] p);

        /// <summary>
        /// Group a selection of expressions as non associative
        /// </summary>
        /// <param name="p">expressions</param>
        void NonAssociative(params object[] p);

        /// <summary>
        /// Ignores an expression. Text matching this regular expression will never be reported. This is suitable
        /// for comments and stripping text.
        /// </summary>
        /// <param name="ignoreExpression">Regular expression to ignore</param>
        void Ignore(string ignoreExpression);

        /// <summary>
        /// Gets and sets the runtime of the constructed lexer. See the enumeration LexerRuntime for an
        /// explanation of the valid values.
        /// </summary>
        LexerRuntime Runtime { get; set; }
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

    /// <summary>
    /// An expression is a terminal token, and this is the configurator object for setting what the expression should match
    /// </summary>
    public interface IExpressionConfigurator : IHideObjectMembers
    {
        /// <summary>
        /// Match a type
        /// </summary>
        /// <typeparam name="TExpressionType">Type to match, most built-in primitive types are supported.</typeparam>
        void ThatMatches<TExpressionType>();

        /// <summary>
        /// Match a regular expression
        /// </summary>
        /// <param name="regex">Regular expression to match</param>
        /// <returns>Next part of the configuration</returns>
        IExpressionReturnConfigurator ThatMatches(string regex);
    }

    /// <summary>
    /// Allows you to specify the return of the expression
    /// </summary>
    public interface IExpressionReturnConfigurator : IHideObjectMembers
    {
        /// <summary>
        /// Specify what the expression, when matched should return.
        /// </summary>
        /// <param name="func">Function to apply when matched. Input is the matched string, output is an object which
        /// will be available as the result of the rule part.</param>
        void AndReturns(Func<string, object> func);
    }

#pragma warning disable 1591
    public interface IRuleByConfigurator : IHideObjectMembers
#pragma warning restore 1591
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

        /// <summary>
        /// By a list of another rule.
        /// </summary>
        /// <typeparam name="TListType">The type that the rule used to build the list returns. This will enable
        /// getting a correctly typed IList</typeparam>
        /// <param name="listElement">Element that should be repeated</param>
        /// <returns>Next configurator</returns>
        IMaybeListNamed ByListOf<TListType>(IRule listElement);

        /// <summary>
        /// By a list of another rule. Return type of this element will be a List&lt;object&gt;
        /// </summary>
        /// <param name="listElement">Element that should be repeated</param>
        /// <returns>Next configurator</returns>
        IMaybeListNamed ByListOf(IRule listElement);
    }


#pragma warning disable 1591
    public interface IOptionalAsConfigurator : IRuleSequenceConfigurator
#pragma warning restore 1591
    {
        /// <summary>
        /// Specify a rule part name, which makes it accessible for 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IRuleSequenceConfigurator As(string name);
    }

#pragma warning disable 1591
    public interface IMaybeNewRuleConfigurator : IHideObjectMembers
#pragma warning restore 1591
    {
        /// <summary>
        /// Begin the configuration of an alternate rule
        /// </summary>
        IRuleByConfigurator Or { get; }        
    }

#pragma warning disable 1591
    public interface IRuleSequenceConfigurator : IMaybeNewRuleConfigurator
#pragma warning restore 1591
    {
        /// <summary>
        /// State what the rule is followed by
        /// </summary>
        IRuleByConfigurator Followed { get; }

        /// <summary>
        /// Specify what should happen when this rule is matched this way
        /// </summary>
        /// <param name="func">Function that will be called. The dynamic 
        /// parameter will contain the named parameters of the preceeding rule. Return an object
        /// that will be available as a result to whoever uses this rule</param>
        /// <returns>Next configurator</returns>
        IMaybeNewRuleConfigurator WhenFound(Func<dynamic, object> func);
    }

#pragma warning disable 1591
    public interface IMaybeListNamed : IListRuleSequenceConfigurator
#pragma warning restore 1591
    {
        /// <summary>
        /// Set the name of the element, which will add it to the dynamic parameter of the
        /// WhenFound function
        /// </summary>
        /// <param name="name">Name of member. This should be a valid C# property name</param>
        /// <returns>Next configurator</returns>
        IListRuleSequenceConfigurator As(string name);
    }

#pragma warning disable 1591
    public interface IListRuleSequenceConfigurator : IRuleSequenceConfigurator
#pragma warning restore 1591
    {
        /// <summary>
        /// Specify additional options on the rule element
        /// </summary>
        IListItemConfigurator ThatIs { get; }
    }

#pragma warning disable 1591
    public interface IListItemConfigurator : IRuleSequenceConfigurator
#pragma warning restore 1591
    {
        /// <summary>
        /// Specify a list separator
        /// </summary>
        /// <param name="separator">Separator, interpreted as a string literal</param>
        /// <returns>Next configurator</returns>
        IListItemConfigurator SeparatedBy(string separator);

        /// <summary>
        /// Specify that the preceeding element may be missing
        /// </summary>
        IListItemConfigurator Optional { get; }
    }
}