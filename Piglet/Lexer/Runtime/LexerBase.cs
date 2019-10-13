using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Piglet.Lexer.Runtime
{
    internal abstract class LexerBase<T, TState>
        : ILexer<T>
    {
        private readonly int endOfInputTokenNumber;

        protected LexerBase(int endOfInputTokenNumber) => this.endOfInputTokenNumber = endOfInputTokenNumber;


        private sealed class LexerStateImpl
            : ILexerInstance<T>
        {
            private readonly LexerBase<T, TState> _lexer;
            private readonly StringBuilder _currentLine = new StringBuilder();
            private readonly StringBuilder _lexeme = new StringBuilder();
            private readonly TextReader _source;
            private TState _state;


            public int CurrentLineNumber { get; private set; } = 1;
            public int CurrentCharacterIndex => _currentLine.Length + 1;
            public string CurrentLine => _currentLine.ToString();
            public string LastLexeme => _lexeme.ToString();


            public LexerStateImpl(TextReader source, LexerBase<T, TState> lexer)
            {
                _lexer = lexer;
                _source = source;
            }

            public (int index, T value) Next()
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
                            return (_lexer.endOfInputTokenNumber, default);

                        peek = 0;
                    }

                    char c = (char)peek;
                    TState nextState = _lexer.GetNextState(_state, c);
                    bool reachedTermination = _lexer.ReachedTermination(nextState);

                    if (reachedTermination)
                    {
                        // We have reached termination.
                        // Two possibilities: current state accepts, if so return token ID otherwise there is an error

                        if (_lexer.GetAction(_state) is (int index, Func<string, T> function) && _lexeme.Length > 0)
                        {
                            // If tokennumber is int.MinValue it is an ignored token, like typically whitespace.
                            // In that case, dont return, continue lexing with the reset parser to get the next token.
                            if (index == int.MinValue)
                            {
                                // Reset state
                                _state = _lexer.GetInitialState();

                                // Clear lexeme
                                _lexeme.Clear();
                            }
                            else
                                // Token completed. Return it
                                return (index, function is null ? default : function(_lexeme.ToString()));
                        }
                        else
                            // We get here if there is no action at the state where the lexer cannot continue given the input. This is fail.
                            throw new LexerException($"Unexpected character '{(c == '\0' ? "NULL" : c.ToString())}' in '{_currentLine.ToString().TrimStart()}{c} ...' at ({CurrentLineNumber}:{CurrentCharacterIndex})")
                            {
                                LineContents = _currentLine.ToString(),
                                CharacterIndex = CurrentCharacterIndex,
                                LineNumber = CurrentLineNumber
                            };
                    }
                    else
                    {
                        // Peek is still last char. If we are going to be switching lines add to the line number and clear the current line buffer
                        if (c == '\n')
                        {
                            CurrentLineNumber++;
                            _currentLine.Clear();
                        }
                        else
                            _currentLine.Append(c);

                        // Machine has not terminated. Switch states, append character to lexeme.
                        _state = nextState;
                        _lexeme.Append(c);
                        _source.Read();
                    }
                }
            }
        }

        public ILexerInstance<T> Begin(TextReader reader) => new LexerStateImpl(reader, this);

        public ILexerInstance<T> Begin(string source) => Begin(new StringReader(source));

        public IEnumerable<(int index, T value)> Tokenize(string source)
        {
            ILexerInstance<T> instance = Begin(source);

            for ((int index, T value) token = instance.Next(); token.index != -1; token = instance.Next())
                yield return token;
        }

        protected abstract (int index, Func<string, T> function)? GetAction(TState state);

        protected abstract bool ReachedTermination(TState nextState);

        protected abstract TState GetNextState(TState state, char input);

        protected abstract TState GetInitialState();
    }
}
