using System;
using Piglet.Lexer;

namespace Piglet.Parser
{
    /// <summary>
    /// ParseExceptions are thrown when the parser detects an illegal token according to the given
    /// grammar.
    /// </summary>
    public sealed class ParseException
        : Exception
    {
        /// <summary>
        /// Current state of the lexer.
        /// </summary>
        public ILexerState LexerState { get; internal set; }

        /// <summary>
        /// This is a list of tokens that would have been valid given the current state
        /// when the parsing failed. This contains the debug name of the tokens.
        /// </summary>
        public string[] ExpectedTokens { get; set; }

        /// <summary>
        /// The debug name of the token that was found instead.
        /// </summary>
        public string FoundToken { get; set; }

        /// <summary>
        /// The state number of the parser when it failed
        /// </summary>
        public int ParserState { get; set; }

        /// <summary>
        /// The token ID of the token that was found.
        /// </summary>
        public int FoundTokenId { get; set; }

        /// <summary>
        /// The current line number in the input text
        /// </summary>
        public int CurrentLineNumber => LexerState.CurrentLineNumber;

        /// <summary>
        /// The contents so far of the current line
        /// </summary>
        public string CurrentLine => LexerState.CurrentLine;

        public int CurrentCharacterIndex => LexerState.CurrentCharacterIndex;


        /// <summary>
        /// Construct a new ParseException
        /// </summary>
        /// <param name="message"></param>
        internal ParseException(string message)
            : base(message)
        {
        }
    }
}