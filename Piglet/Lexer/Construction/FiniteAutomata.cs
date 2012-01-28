using System.Collections.Generic;

namespace Piglet.Lexer.Construction
{
    public abstract class FiniteAutomata<TState> where TState : FiniteAutomata<TState>.BaseState
    {
        public abstract class BaseState
        {
            public abstract bool AcceptState { get; set; }
            public int StateNumber { get; set; }
        }

        public IList<TState> States { get; set; }
        public IList<Transition<TState>>  Transitions { get; set; }
        public TState StartState { get; set; }

        protected FiniteAutomata()
        {
            States = new List<TState>();
            Transitions = new List<Transition<TState>>();
        }

        public void AssignStateNumbers()
        {
            int i = 0;
            foreach (var state in States)
            {
                if (state != StartState)
                    state.StateNumber = ++i;
            }
            // Always use 0 as the start state
            StartState.StateNumber = 0;
        }
    }
}
