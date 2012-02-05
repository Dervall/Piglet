namespace Piglet.Parser.Configuration
{
    public interface ILexerSettings
    {
        bool CreateLexer { get; set; }
        bool EscapeLiterals { get; set; }
        string[] Ignore { get; set; }
    }
}