using System;
using System.ComponentModel;
namespace Piglet.Parser.Configuration.Fluent
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IHideRuleSyntaxStateObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        Type GetType();
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        int GetHashCode();
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        string ToString();
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool Equals(object obj);
    }
    
    public class RuleSyntaxState : IHideRuleSyntaxStateObjectMembers
    {
        private readonly RuleBuilder builder;
        
        internal RuleSyntaxState(RuleBuilder builder) { this.builder = builder; }
        
        ///<summary>
        ///Accept a literal string
        ///</summary>
        ///<param name="literal">Literal accepted by the parser.</param>
        public RuleSyntaxState1 By(string literal)
        {
            builder.AddLiteral(literal);
            return new RuleSyntaxState1(builder);
        }
        
        ///<summary>
        ///Accept a rule as the next element
        ///</summary>
        ///<param name="rule">Rule to accept. The rule need not be configured yet</param>
        public RuleSyntaxState2 By(IRule rule)
        {
            builder.AddRule(rule);
            return new RuleSyntaxState2(builder);
        }
        
        ///<summary>
        ///Accept a type
        ///</summary>
        ///<typeparam name="TExpressionType">Type accepted</typeparam>
        public RuleSyntaxState2 By<TExpressionType>()
        {
            builder.AddType<TExpressionType>();
            return new RuleSyntaxState2(builder);
        }
        
        ///<summary>
        ///Accept a previously defined expression
        ///</summary>
        ///<param name="expression">Expression accepted</param>
        public RuleSyntaxState2 By(IExpressionConfigurator expression)
        {
            builder.AddExpression(expression);
            return new RuleSyntaxState2(builder);
        }
        
        ///<summary>
        ///Accept a list of elements, returing a specific type.
        ///</summary>
        ///<typeparam name="TListType">The type of list that the rule should return</typeparam>
        ///<param name="listElement">Rule that describes a list element</param>
        public RuleSyntaxState5 ByListOf<TListType>(IRule listElement)
        {
            builder.AddTypedList<TListType>(listElement);
            return new RuleSyntaxState5(builder);
        }
        
        ///<summary>
        ///Accept a list of elements, returning a List&lt;object&gt; to the caller.
        ///</summary>
        ///<param name="listElement">Rule describing a single list element</param>
        public RuleSyntaxState5 ByListOf(IRule listElement)
        {
            builder.AddListOf(listElement);
            return new RuleSyntaxState5(builder);
        }
    }
    
    public class RuleSyntaxState1 : IHideRuleSyntaxStateObjectMembers
    {
        private readonly RuleBuilder builder;
        
        internal RuleSyntaxState1(RuleBuilder builder) { this.builder = builder; }
        
        ///<summary>
        ///Begin the next element in a sequence
        ///</summary>
        public RuleSyntaxState Followed
        {
            get
            {
                builder.NextElement();
                return new RuleSyntaxState(builder);
            }
        }
        
        ///<summary>
        ///Add an alternative interpretation of the rule.
        ///</summary>
        public RuleSyntaxState Or
        {
            get
            {
                builder.BeginNextProduction();
                return new RuleSyntaxState(builder);
            }
        }
        
        ///<summary>
        ///If the rule is interpreted this way, specify what the return should be.
        ///</summary>
        ///<param name="func">Function to return the rule value</param>
        public RuleSyntaxState7 WhenFound(Func<dynamic,object> func)
        {
            builder.SetReductionRule(func);
            return new RuleSyntaxState7(builder);
        }
    }
    
    public class RuleSyntaxState2 : IHideRuleSyntaxStateObjectMembers
    {
        private readonly RuleBuilder builder;
        
        internal RuleSyntaxState2(RuleBuilder builder) { this.builder = builder; }
        
        ///<summary>
        ///Begin the next element in a sequence
        ///</summary>
        public RuleSyntaxState Followed
        {
            get
            {
                builder.NextElement();
                return new RuleSyntaxState(builder);
            }
        }
        
        ///<summary>
        ///Names a rule part, allowing it to be used from the methods executed when the
        ///rule has been matched succesfully.
        ///</summary>
        ///<param name="name">Name of element. This must be a valid C# identifier</param>
        public RuleSyntaxState19 As(string name)
        {
            builder.SetProductionElementName(name);
            return new RuleSyntaxState19(builder);
        }
        
        ///<summary>
        ///Specify additional rules applying to the last defined rule part
        ///</summary>
        public RuleSyntaxState1 ThatIs
        {
            get
            {
                builder.StartElementSpecification();
                return new RuleSyntaxState1(builder);
            }
        }
        
        ///<summary>
        ///Add an alternative interpretation of the rule.
        ///</summary>
        public RuleSyntaxState Or
        {
            get
            {
                builder.BeginNextProduction();
                return new RuleSyntaxState(builder);
            }
        }
        
        ///<summary>
        ///If the rule is interpreted this way, specify what the return should be.
        ///</summary>
        ///<param name="func">Function to return the rule value</param>
        public RuleSyntaxState7 WhenFound(Func<dynamic,object> func)
        {
            builder.SetReductionRule(func);
            return new RuleSyntaxState7(builder);
        }
    }
    
    public class RuleSyntaxState5 : IHideRuleSyntaxStateObjectMembers
    {
        private readonly RuleBuilder builder;
        
        internal RuleSyntaxState5(RuleBuilder builder) { this.builder = builder; }
        
        ///<summary>
        ///Begin the next element in a sequence
        ///</summary>
        public RuleSyntaxState Followed
        {
            get
            {
                builder.NextElement();
                return new RuleSyntaxState(builder);
            }
        }
        
        ///<summary>
        ///Names a rule part, allowing it to be used from the methods executed when the
        ///rule has been matched succesfully.
        ///</summary>
        ///<param name="name">Name of element. This must be a valid C# identifier</param>
        public RuleSyntaxState19 As(string name)
        {
            builder.SetProductionElementName(name);
            return new RuleSyntaxState19(builder);
        }
        
        ///<summary>
        ///Specify additional rules applying to the last defined rule part
        ///</summary>
        public RuleSyntaxState24 ThatIs
        {
            get
            {
                builder.StartElementSpecification();
                return new RuleSyntaxState24(builder);
            }
        }
        
        ///<summary>
        ///Add an alternative interpretation of the rule.
        ///</summary>
        public RuleSyntaxState Or
        {
            get
            {
                builder.BeginNextProduction();
                return new RuleSyntaxState(builder);
            }
        }
        
        ///<summary>
        ///If the rule is interpreted this way, specify what the return should be.
        ///</summary>
        ///<param name="func">Function to return the rule value</param>
        public RuleSyntaxState7 WhenFound(Func<dynamic,object> func)
        {
            builder.SetReductionRule(func);
            return new RuleSyntaxState7(builder);
        }
    }
    
    public class RuleSyntaxState7 : IHideRuleSyntaxStateObjectMembers
    {
        private readonly RuleBuilder builder;
        
        internal RuleSyntaxState7(RuleBuilder builder) { this.builder = builder; }
        
        ///<summary>
        ///Add an alternative interpretation of the rule.
        ///</summary>
        public RuleSyntaxState Or
        {
            get
            {
                builder.BeginNextProduction();
                return new RuleSyntaxState(builder);
            }
        }
    }
    
    public class RuleSyntaxState19 : IHideRuleSyntaxStateObjectMembers
    {
        private readonly RuleBuilder builder;
        
        internal RuleSyntaxState19(RuleBuilder builder) { this.builder = builder; }
        
        ///<summary>
        ///Begin the next element in a sequence
        ///</summary>
        public RuleSyntaxState Followed
        {
            get
            {
                builder.NextElement();
                return new RuleSyntaxState(builder);
            }
        }
        
        ///<summary>
        ///Specify additional rules applying to the last defined rule part
        ///</summary>
        public RuleSyntaxState24 ThatIs
        {
            get
            {
                builder.StartElementSpecification();
                return new RuleSyntaxState24(builder);
            }
        }
        
        ///<summary>
        ///Add an alternative interpretation of the rule.
        ///</summary>
        public RuleSyntaxState Or
        {
            get
            {
                builder.BeginNextProduction();
                return new RuleSyntaxState(builder);
            }
        }
        
        ///<summary>
        ///If the rule is interpreted this way, specify what the return should be.
        ///</summary>
        ///<param name="func">Function to return the rule value</param>
        public RuleSyntaxState7 WhenFound(Func<dynamic,object> func)
        {
            builder.SetReductionRule(func);
            return new RuleSyntaxState7(builder);
        }
    }
    
    public class RuleSyntaxState21 : IHideRuleSyntaxStateObjectMembers
    {
        private readonly RuleBuilder builder;
        
        internal RuleSyntaxState21(RuleBuilder builder) { this.builder = builder; }
        
        ///<summary>
        ///Begin the next element in a sequence
        ///</summary>
        public RuleSyntaxState Followed
        {
            get
            {
                builder.NextElement();
                return new RuleSyntaxState(builder);
            }
        }
        
        ///<summary>
        ///Specify additional rules applying to the last defined rule part
        ///</summary>
        public RuleSyntaxState1 ThatIs
        {
            get
            {
                builder.StartElementSpecification();
                return new RuleSyntaxState1(builder);
            }
        }
        
        ///<summary>
        ///Add an alternative interpretation of the rule.
        ///</summary>
        public RuleSyntaxState Or
        {
            get
            {
                builder.BeginNextProduction();
                return new RuleSyntaxState(builder);
            }
        }
        
        ///<summary>
        ///If the rule is interpreted this way, specify what the return should be.
        ///</summary>
        ///<param name="func">Function to return the rule value</param>
        public RuleSyntaxState7 WhenFound(Func<dynamic,object> func)
        {
            builder.SetReductionRule(func);
            return new RuleSyntaxState7(builder);
        }
    }
    
    public class RuleSyntaxState24 : IHideRuleSyntaxStateObjectMembers
    {
        private readonly RuleBuilder builder;
        
        internal RuleSyntaxState24(RuleBuilder builder) { this.builder = builder; }
        
        ///<summary>
        ///Make the rule part optional
        ///</summary>
        public RuleSyntaxState28 Optional
        {
            get
            {
                builder.SetOptionalFlag();
                return new RuleSyntaxState28(builder);
            }
        }
        
        ///<summary>
        ///Specify a separator of list elements.
        ///</summary>
        ///<param name="separator">String literal to use as a separator</param>
        public RuleSyntaxState28 SeparatedBy(string separator)
        {
            builder.SetListSeparator(separator);
            return new RuleSyntaxState28(builder);
        }
    }
    
    public class RuleSyntaxState28 : IHideRuleSyntaxStateObjectMembers
    {
        private readonly RuleBuilder builder;
        
        internal RuleSyntaxState28(RuleBuilder builder) { this.builder = builder; }
        
        ///<summary>
        ///Begin the next element in a sequence
        ///</summary>
        public RuleSyntaxState Followed
        {
            get
            {
                builder.NextElement();
                return new RuleSyntaxState(builder);
            }
        }
        
        ///<summary>
        ///Specify another element attribute
        ///</summary>
        public RuleSyntaxState24 And
        {
            get
            {
                builder.NextElementAttribute();
                return new RuleSyntaxState24(builder);
            }
        }
        
        ///<summary>
        ///Add an alternative interpretation of the rule.
        ///</summary>
        public RuleSyntaxState Or
        {
            get
            {
                builder.BeginNextProduction();
                return new RuleSyntaxState(builder);
            }
        }
        
        ///<summary>
        ///If the rule is interpreted this way, specify what the return should be.
        ///</summary>
        ///<param name="func">Function to return the rule value</param>
        public RuleSyntaxState7 WhenFound(Func<dynamic,object> func)
        {
            builder.SetReductionRule(func);
            return new RuleSyntaxState7(builder);
        }
    }
    
}
