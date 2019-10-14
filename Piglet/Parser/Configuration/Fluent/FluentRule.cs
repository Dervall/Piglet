using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System;

namespace Piglet.Parser.Configuration.Fluent
{
    internal sealed class FluentRule
        : IRuleByConfigurator
        , IRule
        , IListItemConfigurator
        , IOptionalAsConfigurator
        , IMaybeListNamed
    {
        private readonly FluentParserConfigurator _configurator;
        private readonly NonTerminal<object> _nonTerminal;
        private readonly List<List<ProductionElement>> _productionList;
        private readonly List<Func<dynamic, object>?> _funcList;


        private List<ProductionElement> CurrentProduction => _productionList[_productionList.Count - 1];

        public IListItemConfigurator Optional
        {
            get
            {
                ((ListOfRule)CurrentProduction[CurrentProduction.Count - 1]).Optional = true;

                return this;
            }
        }

        public INonTerminal<object> NonTerminal => _nonTerminal;

        public IRuleByConfigurator Or
        {
            get
            {
                // Finish the current rule
                _productionList.Add(new List<ProductionElement>());

                _funcList.Add(null);
                
                return this;
            }
        }

        public IRuleByConfigurator IsMadeUp => this;

        public IListItemConfigurator ThatIs => this;

        public IRuleByConfigurator Followed => this;


        public FluentRule(FluentParserConfigurator configurator, INonTerminal<object> nonTerminal)
        {
            _configurator = configurator;
            _nonTerminal = (NonTerminal<object>)nonTerminal;
            _productionList = new List<List<ProductionElement>> { new List<ProductionElement>() };
            _funcList = new List<Func<dynamic, object>?> { null };
        }

        public IOptionalAsConfigurator By(string literal)
        {
            CurrentProduction.Add(new ProductionElement { Symbol = literal });
            return this;
        }

        public IOptionalAsConfigurator By<TExpressionType>()
        {
            IExpressionConfigurator e = _configurator.Expression();
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

        public IMaybeListNamed ByListOf(IRule listElement) => ByListOf<object>(listElement);

        public IMaybeListNamed ByListOf<TListType>(IRule listElement)
        {
            CurrentProduction.Add(new ListOfTypedObjectRule<TListType> { Symbol = listElement });
            return this;
        }

        public IMaybeNewRuleConfigurator WhenFound(Func<dynamic, object> func)
        {
            _funcList[_funcList.Count - 1] = func;
            return this;
        }

        public IRuleSequenceConfigurator As(string name)
        {
            CurrentProduction[CurrentProduction.Count - 1].Name = name;
            return this;
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

        public void ConfigureProductions()
        {
            // This method prepares the list of objects to another list of objects
            // and sends that to the other configuration interface.
            // Use the nonterminal to configure the production

            for (int productionIndex = 0; productionIndex < _productionList.Count; ++productionIndex)
            {
                List<ProductionElement> production = _productionList[productionIndex];
                bool isErrorRule = false;

                for (int i = 0; i < production.Count; ++i)
                {
                    ProductionElement part = production[i];

                    if (part is ListOfRule)
                    {
                        // This will create new rules, we want to reduce production[i] 
                        ListOfRule listRule = (ListOfRule)part;
                        NonTerminal<object> listNonTerminal = listRule.MakeListRule(_configurator);

                        if (listRule.Optional)
                            listNonTerminal = _configurator.MakeOptionalRule(listNonTerminal);

                        production[i].Symbol = listNonTerminal;
                    }
                    else if (part.Symbol is string)
                    {
                        // Pre-escaped literal
                        // Do nothing, this is already handled.
                    }
                    else if (part.Symbol is FluentRule)
                        production[i].Symbol = ((FluentRule)part.Symbol)._nonTerminal;
                    else if (part.Symbol is FluentExpression)
                    {
                        isErrorRule |= part.Symbol == _configurator.Error;
                        production[i].Symbol = ((FluentExpression)part.Symbol).Terminal;
                    }
                    else
                        throw new ParserConfigurationException("Unknown entity found in production rule list. This should never happen");
                }

                IProduction<object> newProduction = _nonTerminal.AddProduction(production.Select(f => f.Symbol).ToArray());

                // If there is no specific rule specified.
                Func<dynamic, object>? func = _funcList[productionIndex];

                if (func is null)
                {
                    if (production.Count == 1)
                        // Use default rule where all rules of length 1 will autoreduce to the
                        // first propertys semantic value
                        newProduction.SetReduceFunction(f => f[0]);
                }
                else
                {
                    // Specific function found. This needs to be wrapped in another function
                    // which translates the index parameters into a dynamic object by the property names
                    (int index, string name)[] indexNames = production.Select((f, i) => (i, f.Name)).Where(f => f.Item2 is { }).ToArray()!;

                    if (isErrorRule)
                        newProduction.SetErrorFunction((e, f) => func(CreateExpandoObject(f, e, indexNames)));
                    else
                        newProduction.SetReduceFunction(f => func(CreateExpandoObject(f, null, indexNames)));
                }
            }
        }

        private static ExpandoObject CreateExpandoObject(object[] f, object? error, (int index, string name)[] indexNames)
        {
            ExpandoObject expandoObject = new ExpandoObject();
            IDictionary<string, object> dic = expandoObject;

            foreach ((int idx, string name) in indexNames)
                dic[name] = f[idx];

            if (error is { } e)
                dic["Error"] = e;

            return expandoObject;
        }


        private class ProductionElement
        {
            public object? Symbol;
            public string? Name;
        };

        private abstract class ListOfRule
            : ProductionElement
        {
            public string? Separator;
            public bool Optional;


            public abstract NonTerminal<object> MakeListRule(FluentParserConfigurator fluentParserConfigurator);
        };

        private sealed class ListOfTypedObjectRule<TListType>
            : ListOfRule
        {
            public override NonTerminal<object> MakeListRule(FluentParserConfigurator fluentParserConfigurator) => fluentParserConfigurator.MakeListRule<TListType>((IRule)Symbol, Separator);
        }
    }
}
