using Piglet.Lexer;

namespace Piglet.Parser
{
    public interface IParser<T>
    {
        T Parse(ILexer<T> lexer);
    }
}