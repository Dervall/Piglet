using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Piglet.Parser.Configuration.Fluent
{
    /// <summary>
    /// Builder class for the piglet fluent DSL for Rules
    /// </summary>
    /// <builderclass name="RuleSyntaxState" namespace="Piglet.Parser.Configuration.Fluent">
    ///    
    ///    rulePartList : rulePartList Or rulePart
    ///                 | rulePart ;
    ///    
    ///    rulePart : listElements optionalWhenFound ;
    ///    
    ///    optionalWhenFound : WhenFound
    ///                       | ;
    ///    
    ///    listElements : listElements Followed by
    ///                 | by ;
    ///    
    ///    by : ByLiteral | bySimple | byList ;
    ///    
    ///    byList : listBys optionalNaming optionalListSettings ;
    ///    
    ///    listBys : ByTypedListOf
    ///    | ByListOf ;
    ///    
    ///    optionalListSettings : ThatIs listOfListSettings
    ///    | ;
    ///    
    ///    listOfListSettings : listOfListSettings And listSetting
    ///    | listSetting;
    ///    
    ///    listSetting : Optional | SeparatedBy ;
    ///    
    ///    bySimple : simpleBys optionalNaming optionalSimpleSettings;
    ///    
    ///    simpleBys : ByTExpressionType | ByExpression | ByRule;
    ///    
    ///    optionalSimpleSettings : ThatIs | ;
    ///    
    ///    optionalNaming : As | ;
    /// 
    /// </builderclass>
    internal class RuleBuilder : IRule
    {
        private readonly FluentParserConfigurator configurator;
        private readonly NonTerminal<object> nonTerminal;
        private readonly List<List<ProductionElement>> productionList;
        private readonly List<Func<dynamic, object>> funcList;

        private class ProductionElement
        {
            public object Symbol;
            public string Name;
            public bool Optional;
        };

        private abstract class ListOfRule : ProductionElement
        {
            public string Separator;

            public abstract NonTerminal<object> MakeListRule(FluentParserConfigurator fluentParserConfigurator);
        };

        private class ListOfTypedObjectRule<TListType> : ListOfRule
        {
            public override NonTerminal<object> MakeListRule(FluentParserConfigurator fluentParserConfigurator)
            {
                return fluentParserConfigurator.MakeListRule<TListType>((IRule)Symbol, Separator);
            }
        }

        public RuleBuilder(FluentParserConfigurator configurator, INonTerminal<object> nonTerminal)
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

        public RuleSyntaxState IsMadeUp
        {
            get { return new RuleSyntaxState(this); }
        }

        /// <summary>
        /// Begin the next element in a sequence
        /// </summary>
        /// <buildermethod name="Followed"/>
        public void NextElement()
        {
        }

        /// <summary>
        /// Accept a literal string
        /// </summary>
        /// <param name="literal">Literal accepted by the parser.</param>
        /// <buildermethod name="By" dslname="ByLiteral"/>
        public void AddLiteral(string literal)
        {
            CurrentProduction.Add(new ProductionElement { Symbol = literal });
        }

        /// <summary>
        /// Accept a rule as the next element
        /// </summary>
        /// <param name="rule">Rule to accept. The rule need not be configured yet</param>
        /// <buildermethod name="By" dslname="ByRule"/>
        public void AddRule(IRule rule)
        {
            CurrentProduction.Add(new ProductionElement { Symbol = rule });
        }

        /// <summary>
        /// Accept a type
        /// </summary>
        /// <typeparam name="TExpressionType">Type accepted</typeparam>
        /// <buildermethod name="By" dslname="ByTExpressionType"/>
        public void AddType<TExpressionType>()
        {
            CurrentProduction.Add(new ProductionElement { Symbol = configurator.Expression().ThatMatches<TExpressionType>() });
        }

        /// <summary>
        /// Accept a previously defined expression
        /// </summary>
        /// <buildermethod name="By" dslname="ByExpression"/>
        /// <param name="expression">Expression accepted</param>
        public void AddExpression(IExpressionConfigurator expression)
        {
            CurrentProduction.Add(new ProductionElement { Symbol = expression });
        }

        /// <summary>
        /// Accept a list of elements, returing a specific type.
        /// </summary>
        /// <buildermethod name="ByListOf" dslname="ByTypedListOf"/>
        /// <typeparam name="TListType">The type of list that the rule should return</typeparam>
        /// <param name="listElement">Rule that describes a list element</param>
        public void AddTypedList<TListType>(IRule listElement)
        {
            CurrentProduction.Add(new ListOfTypedObjectRule<TListType> { Symbol = listElement });
        }

        /// <summary>
        /// Accept a list of elements, returning a List&lt;object&gt; to the caller.
        /// </summary>
        /// <param name="listElement">Rule describing a single list element</param>
        /// <buildermethod name="ByListOf"/>
        public void AddListOf(IRule listElement)
        {
            AddTypedList<object>(listElement);
        }

        /// <summary>
        /// Names a rule part, allowing it to be used from the methods executed when the
        /// rule has been matched succesfully.
        /// </summary>
        /// <param name="name">Name of element. This must be a valid C# identifier</param>
        /// <buildermethod name="As"/>
        public void SetProductionElementName(string name)
        {
            CurrentProduction[CurrentProduction.Count - 1].Name = name;
        }

        /// <summary>
        /// Specify additional rules applying to the last defined rule part
        /// </summary>
        /// <buildermethod name="ThatIs"/>
        public void StartElementSpecification()
        {
        }

        /// <summary>
        /// Make the rule part optional
        /// </summary>
        /// <buildermethod name="Optional"/>
        public void SetOptionalFlag()
        {
            CurrentProduction[CurrentProduction.Count - 1].Optional = true;
        }

        /// <summary>
        /// Specify another element attribute
        /// </summary>
        /// <buildermethod name="And"/>
        public void NextElementAttribute()
        {
        }

        /// <summary>
        /// Specify a separator of list elements. 
        /// </summary>
        /// <param name="separator">String literal to use as a separator</param>
        /// <buildermethod name="SeparatedBy"/>
        public void SetListSeparator(string separator)
        {
            ((ListOfRule)CurrentProduction[CurrentProduction.Count - 1]).Separator = separator;
        }

        /// <summary>
        /// Add an alternative interpretation of the rule.
        /// </summary>
        /// <buildermethod name="Or"/>
        public void BeginNextProduction()
        {
            // Finish the current rule
            productionList.Add(new List<ProductionElement>());
            funcList.Add(null);
        }

        /// <summary>
        /// If the rule is interpreted this way, specify what the return should be.
        /// </summary>
        /// <param name="func">Function to return the rule value</param>
        /// <buildermethod name="WhenFound"/>
        public void SetReductionRule(Func<dynamic, object> func)
        {
            funcList[funcList.Count - 1] = func;
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
                    else if (part.Symbol is RuleBuilder)
                    {
                        production[i].Symbol = ((RuleBuilder)part.Symbol).NonTerminal;
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
