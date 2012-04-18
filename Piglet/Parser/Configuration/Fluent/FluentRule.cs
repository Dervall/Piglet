using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Piglet.Parser.Configuration.Fluent
{
    internal class FluentRule : IRuleByConfigurator, IRule, IListItemConfigurator, IOptionalAsConfigurator, IMaybeListNamed
    {
        private readonly FluentParserConfigurator configurator;
        private readonly NonTerminal<object> nonTerminal;
        private readonly List<List<ProductionElement>> productionList;
        private readonly List<Func<dynamic, object>> funcList;

        private class ProductionElement
        {
            public object Symbol;
            public string Name;
        };

        private abstract class ListOfRule : ProductionElement
        {
            public string Separator;
            public bool Optional;

            public abstract NonTerminal<object> MakeListRule(FluentParserConfigurator fluentParserConfigurator);
        };

        private class ListOfTypedObjectRule<TListType> : ListOfRule
        {
            public override NonTerminal<object> MakeListRule(FluentParserConfigurator fluentParserConfigurator)
            {
                return fluentParserConfigurator.MakeListRule<TListType>((IRule)Symbol, Separator);
            }
        }

        public FluentRule(FluentParserConfigurator configurator, INonTerminal<object> nonTerminal)
        {
            this.configurator = configurator;
            this.nonTerminal = (NonTerminal<object>)nonTerminal;
            productionList = new List<List<ProductionElement>> { new List<ProductionElement>() };
            funcList = new List<Func<dynamic, object>> { null };
        }

        private List<ProductionElement> CurrentProduction
        {
            get
            {
                return productionList[productionList.Count - 1];
            }
        }

        public IOptionalAsConfigurator By(string literal)
        {
            CurrentProduction.Add(new ProductionElement { Symbol = literal });
            return this;
        }

        public IOptionalAsConfigurator By<TExpressionType>()
        {
            var e = configurator.Expression();
            e.ThatMatches<TExpressionType>();
            CurrentProduction.Add(new ProductionElement { Symbol = e });
            return this;
        }

        public IOptionalAsConfigurator By(IExpressionConfigurator expression)
        {
            CurrentProduction.Add(new ProductionElement { Symbol = expression });
            return this;
        }

        public IOptionalAsConfigurator By(IRule rule)
        {
            CurrentProduction.Add(new ProductionElement { Symbol = rule });
            return this;
        }

        public IMaybeListNamed ByListOf(IRule listElement)
        {
            return ByListOf<object>(listElement);
        }

        public IMaybeListNamed ByListOf<TListType>(IRule listElement)
        {
            CurrentProduction.Add(new ListOfTypedObjectRule<TListType> { Symbol = listElement });
            return this;
        }

        public IRuleByConfigurator Or
        {
            get
            {
                // Finish the current rule
                productionList.Add(new List<ProductionElement>());
                funcList.Add(null);
                return this;
            }
        }

        public IRuleByConfigurator Followed
        {
            get { return this; }
        }

        public IMaybeNewRuleConfigurator WhenFound(Func<dynamic, object> func)
        {
            funcList[funcList.Count - 1] = func;
            return this;
        }

        public IRuleSequenceConfigurator As(string name)
        {
            CurrentProduction[CurrentProduction.Count - 1].Name = name;
            return this;
        }

        public IRuleByConfigurator IsMadeUp
        {
            get { return this; }
        }

        public IListItemConfigurator ThatIs
        {
            get { return this; }
        }

        IListRuleSequenceConfigurator IMaybeListNamed.As(string name)
        {
            CurrentProduction[CurrentProduction.Count - 1].Name = name;
            return this;
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

            for (var productionIndex = 0; productionIndex < productionList.Count; ++productionIndex)
            {
                var production = productionList[productionIndex];

                for (int i = 0; i < production.Count; ++i)
                {
                    var part = production[i];
                    if (part is ListOfRule)
                    {
                        // This will create new rules, we want to reduce production[i] 
                        var listRule = (ListOfRule)part;
                        var listNonTerminal = listRule.MakeListRule(configurator);

                        if (listRule.Optional)
                        {
                            listNonTerminal = configurator.MakeOptionalRule(listNonTerminal);
                        }
                        production[i].Symbol = listNonTerminal;
                    }
                    else if (part.Symbol is string)
                    {
                        // Pre-escaped literal
                        // Do nothing, this is already handled.
                    }
                    else if (part.Symbol is FluentRule)
                    {
                        production[i].Symbol = ((FluentRule)part.Symbol).nonTerminal;
                    }
                    else if (part.Symbol is FluentExpression)
                    {
                        production[i].Symbol = ((FluentExpression)part.Symbol).Terminal;
                    }
                    else
                    {
                        throw new ParserConfigurationException(
                            "Unknown entity found in production rule list. This should never happen");
                    }
                }

                var newProduction = nonTerminal.AddProduction(production.Select(f => f.Symbol).ToArray());

                // If there is no specific rule specified.
                var func = funcList[productionIndex];
                if (func == null)
                {
                    if (production.Count == 1)
                    {
                        // Use default rule where all rules of length 1 will autoreduce to the
                        // first propertys semantic value
                        newProduction.SetReduceFunction(f => f[0]);
                    }
                }
                else
                {
                    // Specific function found. This needs to be wrapped in another function
                    // which translates the index parameters into a dynamic object by the property names
                    var indexNames = production.Select((f, index) => new Tuple<int, string>(index, f.Name)).Where(f => f.Item2 != null).ToArray();

                    newProduction.SetReduceFunction(f =>
                    {
                        var expandoObject = new ExpandoObject();
                        foreach (var indexName in indexNames)
                        {
                            ((IDictionary<string, object>)expandoObject).Add(indexName.Item2, f[indexName.Item1]);
                        }
                        return func(expandoObject);
                    });
                }
            }
        }
    }
}
