using System;
using Piglet.Lexer.Configuration;
using Piglet.Parser.Configuration;
using Piglet.Parser.Construction;
using System.Linq;

namespace Piglet.Lexer
{
    /// <summary>
    /// The lexer factory is the main way of obtaining lexers in Piglet. 
    /// </summary>
    /// <typeparam name="T">Semantic value class of tokens recognized</typeparam>
    public static class LexerFactory<T>
    {
        /// <summary>
        /// Configure and create a lexer in code using a configure function.
        /// </summary>
        /// <param name="configureAction">Actions needed to configure the lexer</param>
        /// <returns>A lexer implementing the configuration specified</returns>
        /// <throws>LexerConfigurationException for errors</throws>
        public static ILexer<T> Configure(Action<ILexerConfigurator<T>> configureAction)
        {
            LexerConfigurator<T> lexerConfigurator = new LexerConfigurator<T>();
            configureAction(lexerConfigurator);
            return lexerConfigurator.CreateLexer();
        }

        /// <summary>
        /// This is the method used by Piglets parserfactory to obtain preconfigured lexers.
        /// </summary>
        /// <param name="grammar">Grammar to generate lexers from</param>
        /// <param name="lexerSettings">Additional lexing settings</param>
        /// <returns>A lexer compatibe with the given grammars tokenizing rules</returns>
        internal static ILexer<T> ConfigureFromGrammar(IGrammar<T> grammar, ILexerSettings lexerSettings) =>
            // This works because the grammar tokens will recieve the same token number
            // since they are assigned to this list in just the same way. AND BECAUSE the
            // end of input token is LAST. if this is changed it WILL break.
            // This might be considered dodgy later on, since it makes it kinda sorta hard to
            // use other lexers with Piglet. Let's see what happens, if anyone ever wants to write their
            // own lexer for Piglet.
            Configure(c =>
            {
                c.Runtime = lexerSettings.Runtime;

                System.Collections.Generic.List<ITerminal<T>> terminals = grammar.AllSymbols.OfType<ITerminal<T>>().ToList();
                foreach (ITerminal<T> terminal in terminals)
                {
                    if (terminal.RegExp != null)
                    {
                        c.Token(terminal.RegExp, terminal.OnParse);
                    }
                }
                c.EndOfInputTokenNumber = terminals.FindIndex(f => f == grammar.EndOfInputTerminal);

                foreach (string ignored in lexerSettings.Ignore)
                {
                    c.Ignore(ignored);
                }
            });
    }
}
