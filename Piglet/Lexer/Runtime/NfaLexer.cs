using System;
using System.Collections.Generic;
using System.Linq;

using Piglet.Lexer.Construction;

namespace Piglet.Lexer.Runtime
{
    internal class NfaLexer<T>
        : LexerBase<T, HashSet<NFA.State>>
    {
        private readonly NFA _nfa;
        private readonly (NFA.State state, (int index, Func<string, T> function))[] _actions;

        public NfaLexer(NFA nfa, IEnumerable<NFA> nfas, List<(string regex, Func<string, T> function)> tokens, int endOfInputTokenNumber)
            : base(endOfInputTokenNumber)
        {
            _nfa = nfa;
            _actions = nfas.Select((n, i) => (n.States.Single(f => f.AcceptState), (i, i < tokens.Count ? tokens[i].function : null))).ToArray();
        }

        protected override (int index, Func<string, T> function)? GetAction(HashSet<NFA.State> state)
        {
            // If none of the included states are accepting states we will return null to signal that there is no appropriate action to take
            if (!state.Any(f => f.AcceptState))
                return null;

            // Get the first applicable action. This returns null if there is no action defined but there are accepting states.
            // This is fine, this means an ignored token.
            (NFA.State state, (int index, Func<string, T> function)) action = _actions.FirstOrDefault(f => state.Contains(f.state));

            if (action.Item2.function is { })
                return action.Item2;

            return (int.MinValue, null);
        }

        protected override bool ReachedTermination(HashSet<NFA.State> nextState) => !nextState.Any();

        protected override HashSet<NFA.State> GetNextState(HashSet<NFA.State> state, char input)
        {
            HashSet<NFA.State> nextState = new HashSet<NFA.State>();

            nextState.UnionWith(_nfa.Closure(_nfa.Transitions.Where(t => t.ValidInput.ContainsChar(input) && state.Contains(t.From)).Select(f => f.To).ToArray()));

            return nextState;
        }

        protected override HashSet<NFA.State> GetInitialState()
        {
            HashSet<NFA.State> initialState = new HashSet<NFA.State>();

            initialState.UnionWith(_nfa.Closure(new[] { _nfa.StartState }));

            return initialState;
        }
    }
}