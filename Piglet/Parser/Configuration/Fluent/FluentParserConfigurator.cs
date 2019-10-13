using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Piglet.Lexer.Configuration;

namespace Piglet.Parser.Configuration.Fluent
{
    internal sealed class FluentParserConfigurator
        : IFluentParserConfigurator
    {
        private readonly Dictionary<Tuple<IRule, string>, NonTerminal<object>> _listRules;
        private readonly Dictionary<NonTerminal<object>, NonTerminal<object>> _optionalRules;
        private readonly ParserConfigurator<object> _configurator;
        private readonly List<FluentRule> _rules;
        private readonly List<string> _ignored;
        
        private IExpressionConfigurator _quotedString;
        private IExpressionConfigurator _errorToken;


        public LexerRuntime Runtime
        {
            get => _configurator.LexerSettings.Runtime;
            set => _configurator.LexerSettings.Runtime = value;
        }

        public IExpressionConfigurator Error => _errorToken ?? (_errorToken = new FluentExpression(_configurator.ErrorToken));

        public IExpressionConfigurator QuotedString
        {
            get
            {
                if (_quotedString is null)
                {
                    _quotedString = Expression();
                    _quotedString.ThatMatches("\"(\\\\.|[^\"])*\"").AndReturns(f => f.Substring(1, f.Length - 2));
                }

                return _quotedString;
            }
        }


        public FluentParserConfigurator(ParserConfigurator<object> configurator)
        {
            _configurator = configurator;
            _rules = new List<FluentRule>();
            _listRules = new Dictionary<Tuple<IRule, string>, NonTerminal<object>>();
            _optionalRules = new Dictionary<NonTerminal<object>, NonTerminal<object>>();
            _ignored = new List<string>();
        }

        public IRule Rule()
        {
            FluentRule rule = new FluentRule(this, _configurator.CreateNonTerminal());

            _rules.Add(rule);

            return rule;
        }

        public IExpressionConfigurator Expression() => new FluentExpression(_configurator);

        public IParser<object> CreateParser()
        {
            // At this point the underlying parser configurator contains a bunch of nonterminals
            // It won't contain all of the nonterminals. We are going to replace everything in every rule with the proper
            // [non]terminals. Then we are going to generate the parser.
            foreach (FluentRule rule in _rules)
                rule.ConfigureProductions();

            _configurator.LexerSettings.CreateLexer = true;
            _configurator.LexerSettings.EscapeLiterals = true;
            _configurator.LexerSettings.Ignore = new[] { @"\s+" }.Concat(_ignored).ToArray();

            IParser<object> parser = _configurator.CreateParser();

            parser.Lexer = _configurator.CreateLexer();

            return parser;
        }

        private ITerminal<object>[] ParamsToTerminalArray(object[] p) => p.OfType<string>().Select(f => _configurator.CreateTerminal(Regex.Escape(f)))
                .Concat(p.OfType<FluentExpression>().Select(f => f.Terminal)).ToArray();

        public void LeftAssociative(params object[] p) => _configurator.LeftAssociative(ParamsToTerminalArray(p));

        public void RightAssociative(params object[] p) => _configurator.RightAssociative(ParamsToTerminalArray(p));

        public void NonAssociative(params object[] p) => _configurator.NonAssociative(ParamsToTerminalArray(p));

        public void Ignore(string ignoreExpression) => _ignored.Add(ignoreExpression);

        public NonTerminal<object> MakeListRule<TListType>(IRule rule, string separator)
        {
            Tuple<IRule, string> t = new Tuple<IRule, string>(rule, separator);

            if (_listRules.ContainsKey(t))
                return _listRules[t];

            // Create a new nonterminal
            NonTerminal<object> listRule = (NonTerminal<object>)_configurator.CreateNonTerminal();

            if (separator != null)
                listRule.AddProduction(listRule, separator, ((FluentRule)rule).NonTerminal).SetReduceFunction(f =>
                {
                    List<TListType> list = (List<TListType>)f[0];

                    list.Add((TListType)f[2]);

                    return list;
                });
            else
                listRule.AddProduction(listRule, ((FluentRule)rule).NonTerminal).SetReduceFunction(f =>
                {
                    List<TListType> list = (List<TListType>)f[0];

                    list.Add((TListType)f[1]);

                    return list;
                });

            listRule.AddProduction(((FluentRule)rule).NonTerminal).SetReduceFunction(f => new List<TListType> { (TListType)f[0] });
            _listRules.Add(t, listRule);

            return listRule;
        }

        public NonTerminal<object> MakeOptionalRule(NonTerminal<object> nonTerminal)
        {
            if (_optionalRules.ContainsKey(nonTerminal))
                return _optionalRules[nonTerminal];

            // Makes a new rule
            NonTerminal<object> optionalRule = (NonTerminal<object>)_configurator.CreateNonTerminal();

            optionalRule.AddProduction(nonTerminal).SetReduceFunction(f => f[0]);
            optionalRule.AddProduction();
            _optionalRules.Add(nonTerminal, optionalRule);

            return optionalRule;
        }
    }
}