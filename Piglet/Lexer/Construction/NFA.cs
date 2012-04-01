using System.Collections.Generic;
using System.Linq;

namespace Piglet.Lexer.Construction
{
    internal class NFA : FiniteAutomata<NFA.State>
    {
        internal class State : BaseState
        {
            public override bool AcceptState { get; set; }

            public override string ToString()
            {
                return string.Format("{0} {1}", StateNumber, AcceptState ? "ACCEPT" : "");
            }
        }

        protected internal void AddAll(NFA nfa)
        {
            foreach (var state in nfa.States)
            {
                States.Add(state);
            }
            foreach (var edge in nfa.Transitions)
            {
                Transitions.Add(edge);
            }
        }

        protected internal NFA Copy()
        {
            var newNFA = new NFA();
            var stateMap = new Dictionary<State, State>();

            foreach (var state in States)
            {
                var newState = new State { AcceptState = state.AcceptState, StateNumber = state.StateNumber };
                stateMap.Add(state, newState);
                newNFA.States.Add(newState);
            }

            foreach (var transition in Transitions)
            {
                // Hard copy the valid input
                var newTransition = new Transition<State>(stateMap[transition.From], stateMap[transition.To],
                    transition.ValidInput == null ? null : transition.ValidInput.ToArray());
                newNFA.Transitions.Add(newTransition);
            }

            newNFA.StartState = stateMap[StartState];

            return newNFA;
        }
        

        public override IEnumerable<State> Closure(State[] states, ISet<State> visitedStates = null)
        {
            if (visitedStates == null)
            {
                visitedStates = new HashSet<State>();
            }

            foreach (var state in states)
            {
                visitedStates.Add(state);
            }

            // Find all states reachable by following only epsilon edges.
            State[] closureStates =
                (from e in Transitions.Where(f => states.Contains(f.From) && !f.ValidInput.Any() && !visitedStates.Contains(f.To)) select e.To).ToArray();

            if (closureStates.Length > 0)
            {
                foreach (var state1 in Closure(closureStates, visitedStates))
                {
                    yield return state1;
                }
            }

            foreach (var state in states)
            {
                yield return state;
            }
        }

        public static NFA Merge(IList<NFA> nfas)
        {
            // Create a new NFA, add everything to it.
            var merged = new NFA();
            foreach (var nfa in nfas)
            {
                merged.AddAll(nfa);
            }

            // Add a new start state
            var state = new State();
            merged.States.Add(state);
            merged.StartState = state;

            // Add epsilon transiontions from the start state to all the previous start states
            foreach (var nfa in nfas)
            {
                merged.Transitions.Add(new Transition<State>(state, nfa.StartState));
            }

            return merged;
        }

        public IDictionary<State, ISet<State>> GetAllClosures()
        {
            var output = new Dictionary<State, ISet<State>>();

            foreach (var state in States)
            {
                ISet<State> set = new HashSet<State>();
                set.UnionWith(Closure(new[] {state}));
                output.Add(state, set);
            }

            return output;
        }
    }
}

