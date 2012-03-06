using System;
using System.Collections.Generic;

namespace Piglet.Parser.Configuration.Fluent
{
    internal class FluentParserConfigurator : IFluentParserConfigurator
    {
        private readonly ParserConfigurator<object> configurator;
        private readonly List<FluentRule> rules;
        private readonly Dictionary<Tuple<IRule, string>, NonTerminal<object>> listRules;
        private readonly Dictionary<NonTerminal<object>, NonTerminal<object>> optionalRules;

        public FluentParserConfigurator(ParserConfigurator<object> configurator)
        {
            this.configurator = configurator;
            rules = new List<FluentRule>();
            listRules = new Dictionary<Tuple<IRule, string>, NonTerminal<object>>();
            optionalRules = new Dictionary<NonTerminal<object>, NonTerminal<object>>();
        }

        public IRule Rule()
        {
            var rule = new FluentRule(this, configurator.CreateNonTerminal());
            rules.Add(rule);
            return rule;
        }

        public IExpressionConfigurator Expression()
        {
            return new FluentExpression(configurator);
        }

        public IExpressionConfigurator QuotedString
        {
            get
            {
                var expr = Expression();
                expr.ThatMatches("\"(\\\\.|[^\"])*\"").AndReturns(f => f.Substring(1, f.Length - 2));

                return expr;
            }
        }

        public IParser<object> CreateParser()
        {
            // At this point the underlying parser configurator contains a bunch of nonterminals
            // It won't contain all of the nonterminals. We are going to replace everything in every rule with the proper
            // [non]terminals. Then we are going to generate the parser.
            foreach (var rule in rules)
            {
                rule.ConfigureProductions();
            }

            configurator.LexerSettings.CreateLexer = true;
            configurator.LexerSettings.EscapeLiterals = true;
            configurator.LexerSettings.Ignore = new[] { @"\s+" };

            var parser = configurator.CreateParser();
            parser.Lexer = configurator.CreateLexer();

            return parser;
        }

        public NonTerminal<object> MakeListRule<TListType>(IRule rule, string separator)
        {
            var t = new Tuple<IRule, string>(rule, separator);
            if (listRules.ContainsKey(t))
                return listRules[t];

            // Create a new nonterminal
            var listRule = (NonTerminal<object>)configurator.CreateNonTerminal();

            if (separator != null)
            {
                listRule.AddProduction(listRule, separator, ((FluentRule)rule).NonTerminal).SetReduceFunction(f =>
                {
                    var list = (List<TListType>)f[0];
                    list.Add((TListType)f[2]);
                    return list;
                });
            }
            else
            {
                listRule.AddProduction(listRule, ((FluentRule)rule).NonTerminal).SetReduceFunction(f =>
                {
                    var list = (List<TListType>)f[0];
                    list.Add((TListType)f[1]);
                    return list;
                });
            }
            listRule.AddProduction(((FluentRule)rule).NonTerminal).SetReduceFunction(f => new List<TListType> { (TListType)f[0] });

            listRules.Add(t, listRule);
            return listRule;
        }

        public NonTerminal<object> MakeOptionalRule(NonTerminal<object> nonTerminal)
        {
            if (optionalRules.ContainsKey(nonTerminal))
                return optionalRules[nonTerminal];

            // Makes a new rule
            var optionalRule = (NonTerminal<object>)configurator.CreateNonTerminal();

            optionalRule.AddProduction(nonTerminal).SetReduceFunction(f => f[0]);
            optionalRule.AddProduction();

            optionalRules.Add(nonTerminal, optionalRule);

            return optionalRule;
        }
    }
}