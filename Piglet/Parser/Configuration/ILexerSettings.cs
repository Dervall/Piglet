namespace Piglet.Parser.Configuration
{
    /// <summary>
    /// Set additional settings for the lexer
    /// </summary>
    public interface ILexerSettings
    {
        /// <summary>
        /// Set to false if you do not desire a lexer. You will need to supply a lexer manually. Defaults to true
        /// </summary>
        bool CreateLexer { get; set; }

        /// <summary>
        /// Should all literals in parsing rules be automatically escaped? Defaults to true
        /// </summary>
        bool EscapeLiterals { get; set; }

        /// <summary>
        /// Set the list of regular expressions to ignore. The default is to ignore all kinds of whitespace.
        /// </summary>
        string[] Ignore { get; set; }
    }
}