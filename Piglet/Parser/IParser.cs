using System.IO;
using Piglet.Lexer;

namespace Piglet.Parser
{
    public interface IParser<T>
    {
        ILexer<T> Lexer { get; set; }
        
        T Parse(string input);
        T Parse(StringReader input);
    }
}