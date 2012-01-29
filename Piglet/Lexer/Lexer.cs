using System;
using System.IO;
using System.Text;

namespace Piglet.Lexer
{
    public class Lexer<T> : ILexer<T>
    {
        private TextReader Source { get; set; }

        private readonly TransitionTable<T> transitionTable;
        private readonly int endOfInputTokenNumber;
        private int state;
        

        // This is for error reporting purposes
        private int lineNumber = 1;
        private StringBuilder currentLine = new StringBuilder();

        public Lexer(TransitionTable<T> transitionTable, int endOfInputTokenNumber)
        {
            this.transitionTable = transitionTable;
            this.endOfInputTokenNumber = endOfInputTokenNumber;
        }

        public Tuple<int, T> Next()
        {
            // Reset state
            state = 0;

            var lexeme = new StringBuilder();

            while(true)
            {
                int peek = Source.Peek();
                // Replace EOF with 0, or we will read outside of the table.
                if (peek == -1)
                {
                    // If reading the end of file and the lexeme is empty, return end of stream token
                    // If the lexeme isn't empty, it must try to find out whatever it is in the lexeme.
                    if (lexeme.Length == 0)
                    {
                        return new Tuple<int, T>(endOfInputTokenNumber, default(T));                        
                    }
                    peek = 0;
                }

                if (peek == '\n')
                {
                    lineNumber++;
                    currentLine = new StringBuilder();
                }

                var c = (char)peek;
                int nextState = transitionTable[state, c];
                if (nextState == -1)
                {
                    // We have reached termination
                    // Two possibilities, current state accepts, if so return token ID
                    // else there is an error
                    var action = transitionTable.GetAction(state);
                    if (action != null)
                    {
                        // If tokennumber is int.MinValue it is an ignored token, like typically whitespace.
                        // In that case, dont return, continue lexing with the reset parser to get the next token.
                        if (action.Item1 == int.MinValue)
                        {
                            // Reset state
                            state = 0;
                            
                            // Clear lexeme
                            lexeme = new StringBuilder();
                        }
                        else
                        {
                            // Token completed. Return it
                            return new Tuple<int, T>(action.Item1,
                                                     action.Item2 == null ? default(T) : action.Item2(lexeme.ToString()));
                        }
                    }
                    else
                    {
                        // We get here if there is no action at the state where the lexer cannot continue given the input.
                        // This is fail.
                        var lexerException =
                            new LexerException(string.Format("Invalid character '{0}'",
                                                             c == '\0' ? "NULL" : c.ToString()))
                                {
                                    LineContents = currentLine.ToString(), 
                                    LineNumber = lineNumber
                                };

                        throw lexerException;
                    }
                }
                else
                {
                    // Machine has not terminated.
                    // Switch states, append character to lexeme.
                    state = nextState;
                    lexeme.Append(c);
                    currentLine.Append(c);
                    Source.Read();
                }
            }
        }

        public void SetSource(TextReader reader)
        {
            Source = reader;
        }

        public void SetSource(string source)
        {
            Source = new StringReader(source);
        }
    }
}