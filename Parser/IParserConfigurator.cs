using System;

namespace Piglet
{
    public interface IParserConfigurator<T>
    {
        ITerminal<T> Terminal(string regExp, Func<string, T> onParse = null);
        INonTerminal<T> NonTerminal(Action<IProductionConfigurator<T>> productionAction = null);
        void OnAccept(INonTerminal<T> start, Func<T, T> acceptAction);
        IParser<T> CreateParser();
    }

    public enum Whitespace
    {
        Ignore,
        Preserve
    }

    public interface ILexerConfiguration
    {
        Whitespace WhitespaceBehaviour { get; set; }
    }
}