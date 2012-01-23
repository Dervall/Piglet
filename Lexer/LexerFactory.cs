using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Piglet.Lexer
{
    public static class LexerFactory<T>
    {
        public static ILexer<T> Configure(Action<ILexerConfigurator<T>> configureAction)
        {
            var lexerConfigurator = new LexerConfigurator<T>();
            configureAction(lexerConfigurator);
            return lexerConfigurator.CreateLexer();
        }
    }

    public class LexerConfigurator<T> : ILexerConfigurator<T>
    {
        protected List<Tuple<string, Func<string, T>>> Tokens { get; set; }

        public ILexer<T> CreateLexer()
        {
            // For each token, create a NFA
            NFA[] nfas = Tokens.Select(token => NFA.Create(PostFixConverter.ToPostFix(token.Item1))).ToArray();

            // Create a merged NFA
            NFA mergedNfa = NFA.Merge(nfas);

            // Convert the NFA to a DFA
            DFA dfa = DFA.Create(mergedNfa);

            // Convert the dfa to table form
            var transitionTable = new TransitionTable<T>(dfa, nfas, Tokens);

            return new Lexer.Lexer<T>(transitionTable);
        }

        public LexerConfigurator()
        {
            Tokens = new List<Tuple<string, Func<string, T>>>();
        }

        public void Token(string regEx, Func<string, T> action)
        {
            Tokens.Add(new Tuple<string, Func<string, T>>(regEx, action));
        }
    }

    public class TransitionTable<T>
    {
        private readonly short[,] table;
        private readonly Tuple<int, Func<string, T>>[] actions;

        public TransitionTable(DFA dfa, NFA[] nfas, List<Tuple<string, Func<string, T>>> tokens)
        {
            table = new short[dfa.States.Count(),255];
            
            // Fill table with illegal action everywhere
            for (int i = 0; i < dfa.States.Count(); ++i )
            {
                for (int j =0; j<255; ++j)
                {
                    table[i, j] = -1;
                }
            }

                actions = new Tuple<int, Func<string, T>>[dfa.States.Count];

            foreach (var state in dfa.States)
            {
                DFA.State state1 = state;
                foreach (var transition in dfa.Transitions.Where(f => f.From == state1))
                {
                    // Set the table entry
                    table[state.StateNumber, transition.OnCharacter] = (short)transition.To.StateNumber;

                    // If this is an accepting state, set the action function to be
                    // the FIRST defined action function if multiple ones match
                    if (state.NfaStates.Any(f => f.AcceptState))
                    {
                        // Find the lowest ranking NFA which has the accepting state in it
                        for (int tokenNumber = 0; tokenNumber < nfas.Count(); ++tokenNumber)
                        {
                            NFA nfa = nfas[tokenNumber];
                            if (nfa.States.Intersect(state.NfaStates.Where(f=>f.AcceptState)).Any())
                            {
                                // Match
                                actions[state.StateNumber] =  new Tuple<int, Func<string, T>>(
                                                                         tokenNumber, tokens[tokenNumber].Item2);
                                break;
                            }
                        }   
                    }
                }
            }
        }

        public short this[int state, char c]
        {
            get { return table[state, c]; }
        }

        public Tuple<int, Func<string, T>> GetAction(int state)
        {
            return actions[state];
        }
    }

    public interface ILexerConfigurator<in T>
    {
        void Token(string regEx, Func<string, T> action );
    }

    public interface ILexer<T>
    {
        Tuple<int, T> Next();
        TextReader Source { get; set; }
    }

    public class Lexer<T> : ILexer<T>
    {
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
                short nextState = transitionTable[state, c];
                if (nextState == -1)
                {
                    // We have reached termination
                    // Two possibilities, current state accepts, if so return token ID
                    // else there is an error
                    Tuple<int, Func<string, T>> action = transitionTable.GetAction(state);
                    if (action != null)
                    {
                        // Reset states
                        state = 0;
                        return new Tuple<int, T>(action.Item1, action.Item2 == null ? default(T) : action.Item2(lexeme.ToString()));
                    }
                    throw new Exception("Cannot find token. Stuck in state");
                }
                // Switch states, append character to lexeme.
                state = nextState;
                lexeme.Append(c);
                Source.Read();
            }
        }

        public TextReader Source { get; set; }
    }
}
