using System;
using System.IO;
using System.Text;

namespace Piglet.Lexer.Runtime
{
    internal abstract class LexerBase<T, TState> : ILexer<T>
    {
        private TextReader Source { get; set; }
        private readonly int endOfInputTokenNumber;
        protected TState State;

        // This is for error reporting purposes
        private int lineNumber = 1;
        private StringBuilder currentLine = new StringBuilder();
        private StringBuilder lexeme = new StringBuilder();

        protected LexerBase(int endOfInputTokenNumber)
        {
            this.endOfInputTokenNumber = endOfInputTokenNumber;
        }

        public ILexerState LexerState
        {
            get { return new LexerStateImpl(lineNumber, currentLine.ToString(), lexeme.ToString()); }
        }

        private class LexerStateImpl : ILexerState
        {
            private readonly int lineNumber;
            private readonly string currentLine;
            private readonly string lastLexeme;

            public LexerStateImpl(int lineNumber, string currentLine, string lastLexeme)
            {
                this.lineNumber = lineNumber;
                this.currentLine = currentLine;
                this.lastLexeme = lastLexeme;
            }

            public int CurrentLineNumber { get { return lineNumber; } }
            public string CurrentLine { get { return currentLine; } }
            public string LastLexeme { get { return lastLexeme; } }
        }

        public void SetSource(TextReader reader)
        {
            Source = reader;
        }

        public void SetSource(string source)
        {
            Source = new StringReader(source);
        }

        public Tuple<int, T> Next()
        {
            ResetState();

            lexeme = new StringBuilder();

            while (true)
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

                var c = (char)peek;
                TState nextState = GetNextState(c);
                var reachedTermination = ReachedTermination(nextState);

                if (reachedTermination)
                {
                    // We have reached termination
                    // Two possibilities, current state accepts, if so return token ID
                    // else there is an error
                    var action = GetAction();
                    if (action != null && lexeme.Length > 0)
                    {
                        // If tokennumber is int.MinValue it is an ignored token, like typically whitespace.
                        // In that case, dont return, continue lexing with the reset parser to get the next token.
                        if (action.Item1 == int.MinValue)
                        {
                            // Reset state
                            ResetState();

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
                    // Peek is still last char. If we are going to be switching lines
                    // add to the line number and clear the current line buffer
                    if (c == '\n')
                    {
                        lineNumber++;
                        currentLine = new StringBuilder();
                    }
                    else
                    {
                        currentLine.Append(c);
                    }

                    // Machine has not terminated.
                    // Switch states, append character to lexeme.
                    State = nextState;
                    lexeme.Append(c);
                    Source.Read();
                }
            }
        }

        protected abstract Tuple<int, Func<string, T>> GetAction();

        protected abstract bool ReachedTermination(TState nextState);

        protected abstract TState GetNextState(char input);

        protected abstract void ResetState();
    }
}
