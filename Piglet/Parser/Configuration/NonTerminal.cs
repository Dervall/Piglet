using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Piglet.Lexer.Runtime;
using Piglet.Parser.Construction;

namespace Piglet.Parser.Configuration
{
    internal class NonTerminal<T>
        : Symbol<T>
        , INonTerminal<T>
    {
        private readonly IParserConfigurator<T> configurator;
        private readonly IList<NonTerminalProduction> productions;


        public NonTerminal(IParserConfigurator<T> configurator)
        {
            this.configurator = configurator;

            productions = new List<NonTerminalProduction>();
        }

        public IEnumerable<IProductionRule<T>> ProductionRules => productions;

        public IProduction<T> AddProduction(params object?[] parts)
        {
            if (parts.Any(part => !(part is string || part is ISymbol<T>)))
                throw new ArgumentException("Only string and ISymbol are valid arguments.", nameof(parts));

            NonTerminalProduction nonTerminalProduction = new NonTerminalProduction(configurator, this, parts);

            productions.Add(nonTerminalProduction);

            return nonTerminalProduction;
        }

        public override string ToString() =>
            $"{DebugName} --> {string.Join(" | ", from r in ProductionRules select string.Join(" ", from s in r.Symbols select s is ITerminal<T> ? $"'{s.DebugName}'" : s.DebugName))}";


        internal sealed class NonTerminalProduction
            : IProduction<T>
            , IProductionRule<T>
        {
            private readonly INonTerminal<T> _resultSymbol;

            public ISymbol<T>[] Symbols { get; }
            public ISymbol<T> ResultSymbol => _resultSymbol;
            public Func<ParseException, LexedToken<T>[], T> ReduceAction { get; private set; }
            public IPrecedenceGroup ContextPrecedence { get; private set; }


            public NonTerminalProduction(IParserConfigurator<T> configurator, INonTerminal<T> resultSymbol, object?[] symbols)
            {
                _resultSymbol = resultSymbol;

                // Move production symbols to the list
                Symbols = new ISymbol<T>[symbols.Length];

                int i = 0;

                foreach (object? part in symbols)
                {
                    if (part is string regex)
                    {
                        if (configurator.LexerSettings.EscapeLiterals)
                            regex = Regex.Escape(regex);

                        Symbols[i] = configurator.CreateTerminal(regex, null, true);
                        Symbols[i].DebugName = (string)part;   // Set debug name to unescaped string, so it's easy on the eyes.
                    }
                    else
                        Symbols[i] = (ISymbol<T>)symbols[i];

                    ++i;
                }
            }

            public void SetReduceFunction(Func<LexedToken<T>[], T> action) => ReduceAction = (e, f) => action(f);// This creates a little lambda that ignores the exception

            public void SetReduceFunction(Func<T[], T> action) => SetReduceFunction(t => action(t.Select(t => t.SymbolValue).ToArray()));

            public void SetReduceToFirst() => SetReduceFunction(f => f[0]);

            public void SetReduceToIndex(int index) => SetReduceFunction(f => f[index]);

            public void SetPrecedence(IPrecedenceGroup precedenceGroup) => ContextPrecedence = precedenceGroup;

            public void SetErrorFunction(Func<ParseException, LexedToken<T>[], T> errorHandler) => ReduceAction = errorHandler;

            public override string ToString()
            {
                string tstr<U>(ISymbol<U> s) => s is ITerminal<U> ? $"'{s.DebugName}'" : s.DebugName;
                
                return $"{string.Join(" ", Symbols.Select(tstr))} --> {tstr(ResultSymbol)}";
            }
        }
    }
}