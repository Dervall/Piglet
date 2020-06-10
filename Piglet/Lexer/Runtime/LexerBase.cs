using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Piglet.Lexer.Runtime
{
    internal abstract class LexerBase<T, TState>
        : ILexer<T>
    {
        private readonly int _endOfInputTokenNumber;


        protected LexerBase(int endOfInputTokenNumber) => _endOfInputTokenNumber = endOfInputTokenNumber;

        public ILexerInstance<T> Begin(TextReader reader) => new LexerStateImpl(reader, this);

        public ILexerInstance<T> Begin(string source) => Begin(new StringReader(source));

        public IEnumerable<(int number, LexedToken<T> token)> Tokenize(string source)
        {
            ILexerInstance<T> instance = Begin(source);

            for ((int number, LexedToken<T> token) token = instance.Next(); token.number != -1; token = instance.Next())
                yield return token;
        }

        protected abstract (int number, Func<string, T>? action)? GetAction(TState state);

        protected abstract bool ReachedTermination(TState nextState);

        protected abstract TState GetNextState(TState state, char input);

        [return: MaybeNull]
        protected abstract TState GetInitialState();


        private sealed class LexerStateImpl
            : ILexerInstance<T>
        {
            private readonly LexerBase<T, TState> _lexer;
            private readonly StringBuilder _currentLine = new StringBuilder();
            private readonly StringBuilder _lexeme = new StringBuilder();
            private readonly TextReader _source;
            [MaybeNull]
            private TState _state;


            public int CurrentLineNumber { get; private set; } = 1;
            public int CurrentAbsoluteIndex { get; private set; } = 0;
            public int CurrentCharacterIndex { get; private set; }
            public string CurrentLine => _currentLine.ToString();
            public string LastLexeme => _lexeme.ToString();


            public LexerStateImpl(TextReader source, LexerBase<T, TState> lexer)
            {
                _lexer = lexer;
                _source = source;
                CurrentAbsoluteIndex = 0;
                CurrentCharacterIndex = 0;
            }

            public (int number, LexedToken<T> token) Next()
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
                            return (_lexer._endOfInputTokenNumber, new LexedToken<T>(default, "", CurrentAbsoluteIndex, CurrentLineNumber, CurrentCharacterIndex, true));

                        peek = 0;
                    }

                    char c = (char)peek;
                    TState nextState = _lexer.GetNextState(_state, c);
                    bool reachedTermination = _lexer.ReachedTermination(nextState);

                    if (reachedTermination)
                    {
                        // We have reached termination.
                        // Two possibilities: current state accepts, if so return token ID otherwise there is an error
                        if (_lexer.GetAction(_state) is { } t && _lexeme.Length > 0)
                        {
                            // If tokennumber is int.MinValue it is an ignored token, like typically whitespace.
                            // In that case, dont return, continue lexing with the reset parser to get the next token.
                            if (t.number == int.MinValue)
                            {
                                // Reset state
                                _state = _lexer.GetInitialState();
                                // Clear lexeme
                                _lexeme.Clear();
                            }
                            else
                            {
                                string str = _lexeme.ToString();
                                T value = t.action is null ? default : t.action(str);
                                LexedToken<T> lx = new LexedToken<T>(value, str, CurrentAbsoluteIndex - str.Length, CurrentLineNumber, CurrentCharacterIndex - str.Length, true);

                                return (t.number, lx); // Token completed. Return it
                            }
                        }
                        else
                        {
                            string input = c == '\0' ? "NULL" : c.ToString();

                            // We get here if there is no action at the state where the lexer cannot continue given the input. This fails.
                            throw new LexerException($"Unexpected character '{input}' in '{_currentLine.ToString().TrimStart()}{c} ...' at ({CurrentLineNumber}:{CurrentCharacterIndex})")
                            {
                                Input = input,
                                LineContents = CurrentLine,
                                CharacterIndex = CurrentCharacterIndex,
                                CurrentAbsoluteIndex = CurrentAbsoluteIndex,
                                LineNumber = CurrentLineNumber
                            };
                        }
                    }
                    else
                    {
                        // Peek is still last char. If we are going to be switching lines add to the line number and clear the current line buffer
                        if (c == '\n')
                        {
                            CurrentLineNumber++;
                            CurrentCharacterIndex = 0;
                            _currentLine.Clear();
                        }
                        else
                            _currentLine.Append(c);

                        // Machine has not terminated. Switch states, append character to lexeme.
                        CurrentCharacterIndex++;
                        CurrentAbsoluteIndex++;
                        _state = nextState;
                        _lexeme.Append(c);
                        _source.Read();
                    }
                }
            }
        }
    }
}
