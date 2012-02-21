using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Piglet.Parser.Configuration.Fluent
{
    internal class FluentRule<T> : IRuleByConfigurator<T>, IRule<T>, IListRuleSequenceConfigurator<T>, IListItemConfigurator<T>
    {
        private readonly FluentParserConfigurator<T> configurator;
        private readonly NonTerminal<T> nonTerminal;
        private readonly List<List<object>> productionList;

        private class ListOfRule
        {
            public IRule<T> Something;
            public string Separator;
            public bool Optional;
        };

        public FluentRule(FluentParserConfigurator<T> configurator, INonTerminal<T> nonTerminal)
        {
            this.configurator = configurator;
            this.nonTerminal = (NonTerminal<T>) nonTerminal;
            this.productionList = new List<List<object>> {new List<object>()};
        }
        
        private List<object> CurrentProduction { 
            get
            {
                return productionList[productionList.Count - 1];
            } 
        }

        public IRuleSequenceConfigurator<T> By(string literal)
        {
            CurrentProduction.Add(Regex.Escape(literal));
            return this;
        }

        public IRuleSequenceConfigurator<T> By<TExpressionType>()
        {
            CurrentProduction.Add(configurator.TypeToRegex(typeof (TExpressionType)));
            return this;
        }

        public IRuleSequenceConfigurator<T> By(IExpressionConfigurator<T> expression)
        {
            CurrentProduction.Add(expression);
            return this;
        }

        public IRuleSequenceConfigurator<T> By(IRule<T> rule)
        {
            CurrentProduction.Add(rule);
            return this;
        }

        public IListRuleSequenceConfigurator<T> ByListOf(IRule<T> listElement)
        {
            CurrentProduction.Add(new ListOfRule { Something = listElement });
            return this;
        }

        public IRuleByConfigurator<T> Or
        {
            get
            {
                // Finish the current rule
                productionList.Add(new List<object>());
                return this;
            }
        }

        public IRuleByConfigurator<T> Followed
        {
            get { return this; }
        }

        public IRuleByConfigurator<T> IsMadeUp
        {
            get { return this; }
        }

        public IListItemConfigurator<T> ThatIs
        {
            get { return this; }
        }

        public IListItemConfigurator<T> SeparatedBy(string separator)
        {
            ((ListOfRule)CurrentProduction[CurrentProduction.Count-1]).Separator = separator;
            return this;
        }

        public IListItemConfigurator<T> Optional
        {
            get { 
                ((ListOfRule) CurrentProduction[CurrentProduction.Count - 1]).Optional = true;
                return this;
            }
        }

        public void PrepareProductions()
        {
            foreach (var production in productionList)
            {
                for (int i = 0; i < productionList.Count; ++i)
                {
                    var part = production[i];
                    if (part is string)
                    {
                        // Pre-escaped literal
                        // Do nothing, this is already handled.
                    }
                    else if (part is FluentRule<T>)
                    {
                        production[i] = ((FluentRule<T>) part).nonTerminal;
                    }
                    else if (part is FluentExpression<T>)
                    {
                        production[i] = ((FluentExpression<T>) part).Terminal;
                    }
                    else if (part is ListOfRule)
                    {
                        // This will create new rules, we want to reduce production[i] 

                    }
                    else
                    {
                        throw new ParserConfigurationException(
                            "Unknown entity found in production rule list. This should never happen");
                    }
                }    
            }
        }
    }
}