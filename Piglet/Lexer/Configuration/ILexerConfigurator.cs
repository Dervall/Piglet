using System;

namespace Piglet.Lexer.Configuration
{
    public interface ILexerConfigurator<in T>
    {
        void Token(string regEx, Func<string, T> action );
        void Ignore(string regEx);
    }
}