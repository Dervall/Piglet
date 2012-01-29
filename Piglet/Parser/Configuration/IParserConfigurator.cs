using System;
using Piglet.Lexer;

namespace Piglet.Parser.Configuration
{
    public interface IParserConfigurator<T>
    {
        ITerminal<T> Terminal(string regExp, Func<string, T> onParse = null);
        INonTerminal<T> NonTerminal(Action<IProductionConfigurator<T>> productionAction = null);
        void SetStartSymbol(INonTerminal<T> start);
        IParser<T> CreateParser();
        void AugmentGrammar();
        ILexer<T> CreateLexer();
    }
}