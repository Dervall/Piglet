using System;
using Piglet.Parser.Configuration;

namespace Piglet.Parser
{
    public static class ParserFactory
    {
        public static IParser<T> Configure<T>(Action<IParserConfigurator<T>> configureAction )
        {
            var parserConfigurator = new ParserConfigurator<T>();
            configureAction(parserConfigurator);
            
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