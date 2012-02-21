using System.Collections.Generic;

namespace Piglet.Parser.Configuration.Fluent
{
    internal class FluentRule : IRuleByConfigurator, IRule, IListRuleSequenceConfigurator, IListItemConfigurator
    {
        private readonly FluentParserConfigurator configurator;
        private readonly NonTerminal<object> nonTerminal;
        private readonly List<List<object>> productionList;

        private class ListOfRule
        {
            public IRule Rule;
            public string Separator;
            public bool Optional;
        };

        public FluentRule(FluentParserConfigurator configurator, INonTerminal<object> nonTerminal)
        {
            this.configurator = configurator;
            this.nonTerminal = (NonTerminal<object>)nonTerminal;
            productionList = new List<List<object>> { new List<object>() };
        }

        private List<object> CurrentProduction
        {
            get
            {
                return productionList[productionList.Count - 1];
            }
        }

        public IRuleSequenceConfigurator By(string literal)
        {
            CurrentProduction.Add(literal);
            return this;
        }

        public IRuleSequenceConfigurator By<TExpressionType>()
        {
            CurrentProduction.Add(configurator.Expression().ThatMatches<TExpressionType>());
            return this;
        }

        public IRuleSequenceConfigurator By(IExpressionConfigurator expression)
        {
            CurrentProduction.Add(expression);
            return this;
        }

        public IRuleSequenceConfigurator By(IRule rule)
        {
            CurrentProduction.Add(rule);
            return this;
        }

        public IListRuleSequenceConfigurator ByListOf(IRule listElement)
        {
            CurrentProduction.Add(new ListOfRule { Rule = listElement });
            return this;
        }

        public IRuleByConfigurator Or
        {
            get
            {
                // Finish the current rule
                productionList.Add(new List<object>());
                return this;
            }
        }

        public IRuleByConfigurator Followed
        {
            get { return this; }
        }

        public IRuleByConfigurator IsMadeUp
        {
            get { return this; }
        }

        public IListItemConfigurator ThatIs
        {
            get { return this; }
        }

        public IListItemConfigurator SeparatedBy(string separator)
        {
            ((ListOfRule)CurrentProduction[CurrentProduction.Count - 1]).Separator = separator;
            return this;
        }

        public IListItemConfigurator Optional
        {
            get
            {
                ((ListOfRule)CurrentProduction[CurrentProduction.Count - 1]).Optional = true;
                return this;
            }
        }

        public INonTerminal<object> NonTerminal
        {
            get
            {
                return nonTerminal;
            }
        }

        public void ConfigureProductions()
        {
            // This method prepares the list of objects to another list of objects
            // and sends that to the other configuration interface.
            // Use the nonterminal to configure the production
            nonTerminal.Productions(p =>
            {
                foreach (var production in productionList)
                {
                    for (int i = 0; i < production.Count; ++i)
                    {
                        var part = production[i];
                        if (part is string)
                        {
                            // Pre-escaped literal
                            // Do nothing, this is already handled.
                        }
                        else if (part is FluentRule)
                        {
                            production[i] = ((FluentRule)part).nonTerminal;
                        }
                        else if (part is FluentExpression)
                        {
                            production[i] = ((FluentExpression)part).Terminal;
                        }
                        else if (part is ListOfRule)
                        {
                            // This will create new rules, we want to reduce production[i] 
                            var listRule = (ListOfRule)part;
                            var listNonTerminal = configurator.MakeListRule(listRule.Rule, listRule.Separator);
                            if (listRule.Optional)
                            {
                                listNonTerminal = configurator.MakeOptionalRule(listNonTerminal);
                            }
                            production[i] = listNonTerminal;
                        }
                        else
                        {
                            throw new ParserConfigurationException(
                                "Unknown entity found in production rule list. This should never happen");
                        }
                    }

                    p.Production(production.ToArray()); // TODO: Insert onreduce here.
                }
            });
        }
    }
}