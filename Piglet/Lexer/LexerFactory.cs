using System;
using System.Collections.Generic;
using Piglet.Lexer.Configuration;
using Piglet.Parser.Configuration;
using Piglet.Parser.Construction;
using System.Linq;

namespace Piglet.Lexer
{
    public static class LexerFactory<T>
    {
        public static ILexer<T> Configure(Action<ILexerConfigurator<T>> configureAction)
        {
            var lexerConfigurator = new LexerConfigurator<T>();
            configureAction(lexerConfigurator);
            return lexerConfigurator.CreateLexer();
        }

        internal static ILexer<T> ConfigureFromGrammar(IGrammar<T> grammar)
        {
            // This works because the grammar tokens will recieve the same token number
            // since they are assigned to this list in just the same way. AND BECAUSE the
            // end of input token is LAST. if this is changed it WILL break.
            // This might be considered dodgy later on, since it makes it kinda sorta hard to
            // use other lexers with Piglet. Let's see what happens.
            return Configure(c =>
            {
                List<ITerminal<T>> terminals = grammar.AllSymbols.OfType<ITerminal<T>>().ToList();
                foreach (var terminal in terminals)
                {
                    if (terminal.RegExp != null)
                    {
                        c.Token(terminal.RegExp, terminal.OnParse);
                    }
                }
                c.EndOfInputTokenNumber = terminals.FindIndex(f => f.RegExp == null);
            });
        }
    }
}
