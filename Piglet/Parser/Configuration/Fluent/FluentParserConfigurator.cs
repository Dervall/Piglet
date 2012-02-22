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
            var rule = new FluentRule(this, configurator.NonTerminal());
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
                expr.ThatMatches("\"(\\\\.|[^\"])*\"");
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
            configurator.LexerSettings.Ignore = new[] {@"\s+"};

            var parser = configurator.CreateParser();
            parser.Lexer = configurator.CreateLexer();
            
            return parser;
        }

        public NonTerminal<object> MakeListRule(IRule rule, string separator)
        {
            var t = new Tuple<IRule, string>(rule, separator);
            if (listRules.ContainsKey(t))
                return listRules[t];
            
            // Create a new nonterminal
            var listRule = (NonTerminal<object>)configurator.NonTerminal();
            
            listRule.Productions(p =>
            {
                if (separator != null)
                {
                    p.Production(listRule, separator, ((FluentRule)rule).NonTerminal).OnReduce(f =>
                    {
                        var list = (List<object>)f[0];
                        list.Add(f[2]);
                        return list;
                    });                                             
                }
                else
                {
                    p.Production(listRule, ((FluentRule)rule).NonTerminal).OnReduce( f =>
                    {
                        var list = (List<object>)f[0];
                        list.Add(f[1]);
                        return list;
                    } );                                             
                }
                p.Production(((FluentRule) rule).NonTerminal).OnReduce(f =>
                {
                    return new List<object> { f[0] };
                });
            });

            listRules.Add(t, listRule);
            return listRule;
        }

        public NonTerminal<object> MakeOptionalRule(NonTerminal<object> nonTerminal)
        {
            if (optionalRules.ContainsKey(nonTerminal))
                return optionalRules[nonTerminal];

            // Makes a new rule
            var optionalRule = (NonTerminal<object>) configurator.NonTerminal();
            optionalRule.Productions(p =>
                                         {
                                             p.Production(nonTerminal).OnReduce(f => f[0]);
                                             p.Production();
                                         });
            optionalRules.Add(nonTerminal, optionalRule);
            return optionalRule;
        }
    }
}