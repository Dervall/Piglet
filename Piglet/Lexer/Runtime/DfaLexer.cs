using System;
using System.Collections.Generic;
using System.Linq;
using Piglet.Lexer.Construction;

namespace Piglet.Lexer.Runtime
{
    internal class DfaLexer<T> : LexerBase<T, DFA.State>
    {
        private readonly DFA dfa;
        private readonly Dictionary<DFA.State, Tuple<int, Func<string, T>>> actions; 

        public DfaLexer(DFA dfa, IList<NFA> nfas, List<Tuple<string, Func<string, T>>> tokens, int endOfInputTokenNumber)
            : base(endOfInputTokenNumber)
        {
            this.dfa = dfa;

            actions = new Dictionary<DFA.State, Tuple<int, Func<string, T>>>();

            // Calculate which DFA state corresponds to each action
            foreach (DFA.State dfaState in dfa.States)
            {
                NFA.State[] acceptingNfaStates = dfaState.NfaStates.Where(a => a.AcceptState).ToArray();
                if (acceptingNfaStates.Any())
                {
                    for (int i = 0; i < nfas.Count; ++i)
                    {
                        if (nfas[i].States.Intersect(acceptingNfaStates).Any())
                        {
                            // This matches, we will store the action in the dictionary
                            actions.Add(dfaState,
                                        i >= tokens.Count
                                            ? new Tuple<int, Func<string, T>>(int.MinValue, null)
                                            : new Tuple<int, Func<string, T>>(i, tokens[i].Item2));
                            break;
                        }
                    }
                }
            }
        }

        protected override Tuple<int, Func<string, T>> GetAction(DFA.State state) => actions.ContainsKey(state) ? actions[state] : null;

        protected override bool ReachedTermination(DFA.State nextState) => nextState == null;

        protected override DFA.State GetNextState(DFA.State state, char input) => dfa.Transitions
                .Where(f => f.From == state && f.ValidInput.Ranges.Any(r => r.From <= input && r.To >= input))
                .Select(f => f.To)
                .SingleOrDefault();

        protected override DFA.State GetInitialState() => dfa.StartState;
    }
}