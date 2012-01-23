using System;

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
    }
}
