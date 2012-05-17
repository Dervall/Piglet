using System;
using System.Collections.Generic;
using System.Linq;
using Piglet.Lexer.Construction;

namespace Piglet.Lexer.Runtime
{
    internal class NfaLexer<T> : LexerBase<T, HashSet<NFA.State>>
    {
        private readonly NFA nfa;
        private readonly Tuple<NFA.State, Tuple<int, Func<string, T>>>[] actions;

        public NfaLexer(NFA nfa, IEnumerable<NFA> nfas, List<Tuple<string, Func<string, T>>> tokens, int endOfInputTokenNumber)
            : base(endOfInputTokenNumber)
        {
            this.nfa = nfa;
            actions = nfas.Select((n, i) => new Tuple<NFA.State, Tuple<int, Func<string, T>>>(n.States.Single(f => f.AcceptState), new Tuple<int, Func<string, T>>( i,
                i < tokens.Count ? tokens[i].Item2 : null))).ToArray();
        }

        protected override Tuple<int, Func<string, T>> GetAction()
        {
            // If none of the included states are accepting states we will return null to signal that there is no appropriate
            // action to take
            if (!State.Any(f => f.AcceptState))
            {
                return null;
            }

            // Get the first applicable action. This returns null of there is no action defined but there are accepting
            // states. This is fine, this means an ignored token.
            var action = actions.FirstOrDefault(f => State.Contains(f.Item1));
            return action != null && action.Item2.Item2 != null ? action.Item2 : new Tuple<int, Func<string, T>>(int.MinValue, null);
        }

        protected override bool ReachedTermination(HashSet<NFA.State> nextState)
        {
            return !nextState.Any();
        }

        protected override HashSet<NFA.State> GetNextState(char input)
        {
            var nextState = new HashSet<NFA.State>();
            nextState.UnionWith(nfa.Closure(
                nfa.Transitions.Where(t => t.ValidInput.ContainsChar(input) && State.Contains(t.From)).Select(f => f.To).
                    ToArray()));
            return nextState;
        }

        protected override void ResetState()
        {
            State = new HashSet<NFA.State>();
            State.UnionWith(nfa.Closure(new[] {nfa.StartState}));
        }
    }
}