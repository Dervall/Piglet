using System;
using System.IO;
using System.Text;

namespace Piglet.Lexer
{
    public class Lexer<T> : ILexer<T>
    {
        public TextReader Source { get; set; }

        private readonly TransitionTable<T> transitionTable;
        private int state = 0;

        public Lexer(TransitionTable<T> transitionTable)
        {
            this.transitionTable = transitionTable;
        }

        public Tuple<int, T> Next()
        {
            if (Source.Peek() == -1)
            {
                // End of stream
                return null;
            }

            var lexeme = new StringBuilder();

            while(true)
            {
                int peek = Source.Peek();
                // Replace EOF with 0, or we will read outside of the table.
                if (peek == -1)
                {
                    peek = 0;
                }

                var c = (char)peek;
                int nextState = transitionTable[state, c];
                if (nextState == -1)
                {
                    // We have reached termination
                    // Two possibilities, current state accepts, if so return token ID
                    // else there is an error
                    Tuple<int, Func<string, T>> action = transitionTable.GetAction(state);
                    if (action != null)
                    {
                        // Reset state
                        state = 0;

                        // If tokennumber is -1 it is an ignored token, like typically whitespace.
                        // In that case, dont return, continue lexing with the reset parser to get the next token.
                        if (action.Item1 != -1)
                        {
                            return new Tuple<int, T>(action.Item1,
                                                     action.Item2 == null ? default(T) : action.Item2(lexeme.ToString()));
                        }
                    }
                    else
                    {
                        // We get here if there is no action at the state where the lexer cannot continue given the input.
                        // This is fail.
                        throw new Exception("Cannot find token. Stuck in state");
                    }
                }
                else
                {
                    // Machine has not terminated.
                    // Switch states, append character to lexeme.
                    state = nextState;
                    lexeme.Append(c);
                    Source.Read();
                }
            }
        }
    }
}