using System.Collections.Generic;
using System.Linq;
using System;

using Piglet.Lexer.Construction;

namespace Piglet.Lexer.Runtime
{
    internal sealed class DfaLexer<T>
        : LexerBase<T, DFA.State>
    {
        private readonly DFA _dfa;
        private readonly Dictionary<DFA.State, (int number, Func<string, T>? action)> _actions; 


        public DfaLexer(DFA dfa, IList<NFA> nfas, List<(string regex, Func<string, T> action)> tokens, int endOfInputTokenNumber)
            : base(endOfInputTokenNumber)
        {
            _dfa = dfa;
            _actions = new Dictionary<DFA.State, (int number, Func<string, T>? action)>();

            // Calculate which DFA state corresponds to each action
            foreach (DFA.State dfaState in dfa.States)
            {
                NFA.State[] acceptingNfaStates = dfaState.NfaStates.Where(a => a.AcceptState).ToArray();

                if (acceptingNfaStates.Any())
                    for (int i = 0; i < nfas.Count; ++i)
                        if (nfas[i].States.Intersect(acceptingNfaStates).Any())
                        {
                            // This matches, we will store the action in the dictionary
                            _actions[dfaState] = i >= tokens.Count ? (int.MinValue, null) : (i, tokens[i].action);

                            break;
                        }
            }
        }

        protected override (int number, Func<string, T>? action)? GetAction(DFA.State state) => _actions.ContainsKey(state) ? _actions[state] : ((int, Func<string, T>)?)null;

        protected override bool ReachedTermination(DFA.State nextState) => nextState is null;

        protected override DFA.State GetNextState(DFA.State state, char input) => (from t in _dfa.Transitions
                                                                                   where t.From == state
                                                                                   where t.ValidInput.Ranges.Any(r => r.From <= input && r.To >= input)
                                                                                   select t.To).SingleOrDefault();

        protected override DFA.State GetInitialState() => _dfa.StartState;
    }
}