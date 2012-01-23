using System;

namespace Piglet.Lexer
{
    public interface ILexerConfigurator<in T>
    {
        void Token(string regEx, Func<string, T> action );
    }
}