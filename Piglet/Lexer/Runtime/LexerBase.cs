using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Piglet.Lexer.Runtime
{
    internal abstract class LexerBase<T, TState>
        : ILexer<T>
    {
        private readonly int _endOfInputTokenNumber;


        protected LexerBase(int endOfInputTokenNumber) => _endOfInputTokenNumber = endOfInputTokenNumber;

        public ILexerInstance<T> Begin(TextReader reader) => new LexerStateImpl(reader, this);

        public ILexerInstance<T> Begin(string source) => Begin(new StringReader(source));

        public IEnumerable<Tuple<int, T>> Tokenize(string source)
        {
            ILexerInstance<T> instance = Begin(source);

            for (Tuple<int, T> token = instance.Next(); token.Item1 != -1; token = instance.Next())
                yield return token;
        }

        protected abstract Tuple<int, Func<string, T>> GetAction(TState state);

        protected abstract bool ReachedTermination(TState nextState);

        protected abstract TState GetNextState(TState state, char input);

        protected abstract TState GetInitialState();



        private sealed class LexerStateImpl
            : ILexerInstance<T>
        {
            private readonly LexerBase<T, TState> _lexer;
            private readonly StringBuilder _currentLine = new StringBuilder();
            private readonly StringBuilder _lexeme = new StringBuilder();
            private readonly TextReader _source;
            private TState _state;


            public int CurrentLineNumber { get; private set; } = 1;
            public int CurrentCharacterIndex => _currentLine.Length;
            public string CurrentLine => _currentLine.ToString();
            public string LastLexeme => _lexeme.ToString();


            public LexerStateImpl(TextReader source, LexerBase<T, TState> lexer)
            {
                _lexer = lexer;
                _source = source;
            }

            public Tuple<int, T> Next()
            {
                _state = _lexer.GetInitialState();
                _lexeme.Clear();

                while (true)
                {
                    int peek = _source.Peek();

                    // Replace EOF with 0, or we will read outside of the table.
                    if (peek == -1)
                    {
                        // If reading the end of file and the lexeme is empty, return end of stream token
                        // If the lexeme isn't empty, it must try to find out whatever it is in the lexeme.
                        if (_lexeme.Length == 0)
                            return new Tuple<int, T>(_lexer._endOfInputTokenNumber, default);

                        peek = 0;
                    }

                    char c = (char)peek;
                    TState nextState = _lexer.GetNextState(_state, c);
                    bool reachedTermination = _lexer.ReachedTermination(nextState);

                    if (reachedTermination)
                    {
                        // We have reached termination
                        // Two possibilities, current state accepts, if so return token ID
                        // else there is an error
                        Tuple<int, Func<string, T>> action = _lexer.GetAction(_state);

                        if (action != null && _lexeme.Length > 0)
                        {
                            // If tokennumber is int.MinValue it is an ignored token, like typically whitespace.
                            // In that case, dont return, continue lexing with the reset parser to get the next token.
                            if (action.Item1 == int.MinValue)
                            {
                                // Reset state
                                _state = _lexer.GetInitialState();
                                // Clear lexeme
                                _lexeme.Clear();
                            }
                            else
                                // Token completed. Return it
                                return new Tuple<int, T>(action.Item1, action.Item2 is null ? default : action.Item2(_lexeme.ToString()));
                        }
                        else
                            // We get here if there is no action at the state where the lexer cannot continue given the input. This fails.
                            throw new LexerException($"Unexpected character '{(c == '\0' ? "NULL" : c.ToString())}' in '{_currentLine.ToString().TrimStart()}{c} ...' at ({CurrentLineNumber}:{CurrentCharacterIndex})")
                            {
                                LineContents = CurrentLine,
                                CharacterIndex = CurrentCharacterIndex,
                                LineNumber = CurrentLineNumber
                            };
                    }
                    else
                    {
                        // Peek is still last char. If we are going to be switching lines
                        // add to the line number and clear the current line buffer
                        if (c == '\n')
                        {
                            CurrentLineNumber++;
                            _currentLine.Clear();
                        }
                        else
                            _currentLine.Append(c);

                        // Machine has not terminated.
                        // Switch states, append character to lexeme.
                        _state = nextState;
                        _lexeme.Append(c);
                        _source.Read();
                    }
                }
            }
        }

    }
}
