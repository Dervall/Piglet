using System;
using System.Collections.Generic;
using System.IO;
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
            foreach (var dfaState in dfa.States)
            {
                var acceptingNfaStates = dfaState.NfaStates.Where(a => a.AcceptState).ToArray();
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
        
        protected override Tuple<int, Func<string, T>> GetAction()
        {
            return actions.ContainsKey(State) ? actions[State] : null;
        }

        protected override bool ReachedTermination(DFA.State nextState)
        {
            return nextState == null;
        }

        protected override DFA.State GetNextState(char input)
        {
            return dfa.Transitions
                .Where(f => f.From == State && f.ValidInput.Ranges.Any(r => r.From <= input && r.To >= input))
                .Select(f => f.To)
                .SingleOrDefault();
        }

        protected override void ResetState()
        {
            State = dfa.StartState;
        }
    }
}