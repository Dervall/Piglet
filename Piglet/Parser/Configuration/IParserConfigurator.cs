using System;
using Piglet.Lexer;

namespace Piglet.Parser.Configuration
{
    public interface IParserConfigurator<T>
    {
        ITerminal<T> Terminal(string regExp, Func<string, T> onParse = null);
        INonTerminal<T> NonTerminal(Action<IProductionConfigurator<T>> productionAction = null);

        ILexerSettings LexerSettings { get; }
    }

    public interface ILexerSettings
    {
        bool CreateLexer { get; set; }
        bool EscapeLiterals { get; set; }
        string[] Ignore { get; set; }
    }
}