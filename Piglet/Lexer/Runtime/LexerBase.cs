using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Piglet.Lexer.Runtime
{
    internal abstract class LexerBase<TContext, T, TState> : ILexer<T>, ILexer<TContext, T>
    {
        private readonly int endOfInputTokenNumber;
        
        protected LexerBase(int endOfInputTokenNumber)
        {
            this.endOfInputTokenNumber = endOfInputTokenNumber;
        }

        private class LexerStateImpl : ILexerInstance<T>, ILexerInstance<TContext, T>
        {
            private readonly LexerBase<TContext, T, TState> lexer;
            private readonly StringBuilder currentLine = new StringBuilder();
            private readonly StringBuilder lexeme = new StringBuilder();
            private readonly TextReader source;

            private int lineNumber = 1;
            private TState state;

            public LexerStateImpl(TextReader source, LexerBase<TContext, T, TState> lexer)
            {
                this.lexer = lexer;
                this.source = source;
            }

            public int CurrentLineNumber { get { return lineNumber; } }
            public string CurrentLine { get { return currentLine.ToString(); } }
            public string LastLexeme { get { return lexeme.ToString(); } }

			public Tuple<int, T> Next()
			{
				return Next(default(TContext));
			}

        	public Tuple<int, T> Next(TContext context)
            {
                state = lexer.GetInitialState();

                lexeme.Clear();

                while (true)
                {
                    int peek = source.Peek();

                    // Replace EOF with 0, or we will read outside of the table.
                    if (peek == -1)
                    {
                        // If reading the end of file and the lexeme is empty, return end of stream token
                        // If the lexeme isn't empty, it must try to find out whatever it is in the lexeme.
                        if (lexeme.Length == 0)
                        {
                            return new Tuple<int, T>(lexer.endOfInputTokenNumber, default(T));
                        }
                        peek = 0;
                    }

                    var c = (char)peek;
                    TState nextState = lexer.GetNextState(state, c);
                    var reachedTermination = lexer.ReachedTermination(nextState);

                    if (reachedTermination)
                    {
                        // We have reached termination
                        // Two possibilities, current state accepts, if so return token ID
                        // else there is an error
                        var action = lexer.GetAction(state);
                        if (action != null && lexeme.Length > 0)
                        {
                            // If tokennumber is int.MinValue it is an ignored token, like typically whitespace.
                            // In that case, dont return, continue lexing with the reset parser to get the next token.
                            if (action.Item1 == int.MinValue)
                            {
                                // Reset state
                                state = lexer.GetInitialState();

                                // Clear lexeme
                                lexeme.Clear();
                            }
                            else
                            {
                                // Token completed. Return it
                                return new Tuple<int, T>(action.Item1,
                                                         action.Item2 == null ? default(T) : action.Item2(context, lexeme.ToString()));
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
                            currentLine.Clear();
                        }
                        else
                        {
                            currentLine.Append(c);
                        }

                        // Machine has not terminated.
                        // Switch states, append character to lexeme.
                        state = nextState;
                        lexeme.Append(c);
                        source.Read();
                    }
                }
            }
        }

        public ILexerInstance<T> Begin(TextReader reader)
        {
            return new LexerStateImpl(reader, this);
        }

    	ILexerInstance<TContext, T> ILexer<TContext, T>.Begin(string source)
    	{
    		return ContextualBegin(source);
    	}

    	private ILexerInstance<TContext, T> ContextualBegin(string source)
    	{
    		return new LexerStateImpl(new StringReader(source), this);
    	}

    	public IEnumerable<Tuple<int, T>> Tokenize(TContext context, string source)
    	{
			var instance = ContextualBegin(source);
			for (var token = instance.Next(context); token.Item1 != -1; token = instance.Next(context))
			{
				yield return token;
			}
    	}

    	ILexerInstance<TContext, T> ILexer<TContext, T>.Begin(TextReader reader)
    	{
			return new LexerStateImpl(reader, this);
    	}

    	public ILexerInstance<T> Begin(string source)
        {
            return Begin(new StringReader(source));
        }

        public IEnumerable<Tuple<int, T>> Tokenize(string source)
        {
			var instance = Begin(source);
			for (var token = instance.Next(); token.Item1 != -1; token = instance.Next())
			{
				yield return token;
			}
        }

        protected abstract Tuple<int, Func<TContext, string, T>> GetAction(TState state);

        protected abstract bool ReachedTermination(TState nextState);

        protected abstract TState GetNextState(TState state, char input);

        protected abstract TState GetInitialState();
    }
}
