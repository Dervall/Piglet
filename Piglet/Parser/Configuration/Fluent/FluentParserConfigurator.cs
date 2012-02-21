using System;
using System.Collections.Generic;

namespace Piglet.Parser.Configuration.Fluent
{
    internal class FluentParserConfigurator<T> : IFluentParserConfigurator<T>
    {
        private readonly ParserConfigurator<T> configurator;
        private readonly List<FluentRule<T>> rules;

        public FluentParserConfigurator(ParserConfigurator<T> configurator)
        {
            this.configurator = configurator;
            this.rules = new List<FluentRule<T>>();
        }

        public IRule<T> Rule()
        {
            var rule = new FluentRule<T>(this, configurator.NonTerminal());
            rules.Add(rule);
            return rule;
        }

        public IExpressionConfigurator<T> Expression()
        {
            return new FluentExpression<T>(configurator);
        }

        public IExpressionConfigurator<T> QuotedString
        {
            get
            {
                var expr = Expression();
                expr.ThatMatches("\"(\\\\.|[^\"])*\"");
                return expr;
            }
        }

        public IParser<T> CreateParser()
        {
            // At this point the underlying parser configurator contains a bunch of nonterminals
            // It won't contain all of the nonterminals. We are going to replace everything in every rule with the proper
            // [non]terminals. Then we are going to generate the parser.
            foreach (var rule in rules)
            {
                rule.PrepareProductions();
            }

            return configurator.CreateParser();
        }

        public IExpressionConfigurator<T> TypeToRegex(Type type)
        {
            var expr = Expression();
            expr.ThatMatches<int>();
            return expr;
        }
    }
}