using System;
using System.Collections.Generic;
using System.Linq;
using Piglet.Lexer.Construction;

namespace Piglet.Lexer.Runtime
{
    internal class DfaLexer<T>
        : LexerBase<T, DFA.State>
    {
        private readonly DFA _dfa;
        private readonly Dictionary<DFA.State, (int index, Func<string, T> function)> _actions; 


        public DfaLexer(DFA dfa, IList<NFA> nfas, List<(string regex, Func<string, T> function)> tokens, int endOfInputTokenNumber)
            : base(endOfInputTokenNumber)
        {
            _dfa = dfa;
            _actions = new Dictionary<DFA.State, (int index, Func<string, T> function)>();

            // Calculate which DFA state corresponds to each action
            foreach (DFA.State dfaState in dfa.States)
            {
                NFA.State[] acceptingNfaStates = dfaState.NfaStates.Where(a => a.AcceptState).ToArray();

                if (acceptingNfaStates.Any())
                    for (int i = 0; i < nfas.Count; ++i)
                        if (nfas[i].States.Intersect(acceptingNfaStates).Any())
                        {
                            // This matches, we will store the action in the dictionary
                            _actions[dfaState] = i >= tokens.Count ? (int.MinValue, null) : (i, tokens[i].function);

                            break;
                        }
            }
        }

        protected override (int index, Func<string, T> function)? GetAction(DFA.State state) => _actions.ContainsKey(state) ? _actions[state] : ((int, Func<string, T>)?)null;

        protected override bool ReachedTermination(DFA.State nextState) => nextState == null;

        protected override DFA.State GetNextState(DFA.State state, char input) =>
            _dfa.Transitions
                .Where(f => f.From == state && f.ValidInput.Ranges.Any(r => r.From <= input && r.To >= input))
                .Select(f => f.To)
                .SingleOrDefault();

        protected override DFA.State GetInitialState() => _dfa.StartState;
    }
}