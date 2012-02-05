using System;
using Piglet.Parser.Configuration;

namespace Piglet.Parser
{
    /// <summary>
    /// The parserfactory is the main way of obtaining parsers from Piglet.
    /// </summary>
    public static class ParserFactory
    {
        /// <summary>
        /// Configure a parser using code based configuration
        /// </summary>
        /// <typeparam name="T">Semantic value type of tokens</typeparam>
        /// <param name="configureAction">Action that configures the parser</param>
        /// <returns>A parser, ready for use</returns>
        public static IParser<T> Configure<T>(Action<IParserConfigurator<T>> configureAction )
        {
            var parserConfigurator = new ParserConfigurator<T>();
            configureAction(parserConfigurator);
            parserConfigurator.AugmentGrammar();

            var parser = parserConfigurator.CreateParser();

            // If our lexer settings says that we are supposed to create a lexer, do so now and assign
            // the lexer to the created parser.
            if (parserConfigurator.LexerSettings.CreateLexer)
            {
                parser.Lexer = parserConfigurator.CreateLexer();
            }

            return parser;
        }
    }
}