using System.Collections.Generic;
using System.Linq;
using System;

using Piglet.Lexer.Construction;

namespace Piglet.Lexer.Runtime
{
    internal sealed class NfaLexer<T>
        : LexerBase<T, HashSet<NFA.State>>
    {
        private readonly NFA _nfa;
        private readonly (NFA.State, (int number, Func<string, T>? action)?)?[] _actions;


        public NfaLexer(NFA nfa, IEnumerable<NFA> nfas, List<(string regex, Func<string, T> action)> tokens, int endOfInputTokenNumber)
            : base(endOfInputTokenNumber)
        {
            _nfa = nfa;
            _actions = nfas.Select((n, i) => ((NFA.State, (int, Func<string, T>?)?)?)(n.States.Single(f => f.AcceptState), (i,
                i < tokens.Count ? tokens[i].action : null))).ToArray();
        }

        protected override (int number, Func<string, T>? action)? GetAction(HashSet<NFA.State> state)
        {
            // If none of the included states are accepting states we will return null to signal that there is no appropriate
            // action to take
            if (!state.Any(f => f.AcceptState))
                return null;

            // Get the first applicable action. This returns null of there is no action defined but there are accepting
            // states. This is fine, this means an ignored token.
            (NFA.State, (int number, Func<string, T>? action)?)? action = _actions.FirstOrDefault(f => state.Contains(f?.Item1));

            if (action?.Item2?.action is { })
                return action.Value.Item2;

            return (int.MinValue, null);
        }

        protected override bool ReachedTermination(HashSet<NFA.State> nextState) => !nextState.Any();

        protected override HashSet<NFA.State> GetNextState(HashSet<NFA.State> state, char input)
        {
            HashSet<NFA.State> nextState = new HashSet<NFA.State>();

            nextState.UnionWith(_nfa.Closure(
                (from t in _nfa.Transitions
                 where t.ValidInput.ContainsChar(input)
                 where state.Contains(t.From)
                 select t.To).ToArray()
            ));

            return nextState;
        }

        protected override HashSet<NFA.State> GetInitialState()
        {
            if (_nfa.StartState is NFA.State start)
            {
                HashSet<NFA.State> initialState = new HashSet<NFA.State>();

                initialState.UnionWith(_nfa.Closure(new[] { start }));

                return initialState;
            }
            else
                throw new InvalidOperationException("The start state must not be null.");
        }
    }
}