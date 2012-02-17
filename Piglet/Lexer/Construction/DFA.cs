using System;
using System.Collections.Generic;
using System.Linq;
using Piglet.Lexer.Construction.DotNotation;

namespace Piglet.Lexer.Construction
{
    internal class DFA : FiniteAutomata<DFA.State>
    {
        public class State : BaseState
        {
            public IList<NFA.State> NfaStates { get; private set; }
            public bool Mark { get; set; }

            public State(IEnumerable<NFA.State> nfaStates)
            {
                NfaStates = nfaStates.ToList();
            }

            public IEnumerable<char> LegalMoves(NFA nfa)
            {
                return (from e in nfa.Transitions.Where(f => NfaStates.Contains(f.From)) select e.ValidInput).SelectMany(f => f).Distinct();
            }

            public NFA.State[] Move(NFA nfa, char c)
            {
                // Find transitions going OUT from this state that is valid with an input c
                return (from e in nfa.Transitions.Where(f => NfaStates.Contains(f.From) && f.ValidInput.Contains(c)) select e.To).ToArray();
            }

            public override string ToString()
            {
                // Purely for debugging purposes
                return string.Format( "{0} {{{1}}}", StateNumber, String.Join( ", ", NfaStates));
            }

            public override bool AcceptState
            {
                get { return NfaStates.Any(f=>f.AcceptState); }
                set {}  // Do nothing, cannot set
            }
        }

        public static DFA Create(NFA nfa)
        {
            // Get the closure set of S0
            var dfa = new DFA();
            dfa.States.Add(new State(nfa.Closure(new[] { nfa.StartState })));

            while (true)
            {
                // Get an unmarked state in dfaStates
                var t = dfa.States.FirstOrDefault(f => !f.Mark);
                if (null == t)
                {
                    // We're done!
                    break;
                }

                t.Mark = true;

                // Get the move states by stimulating this DFA state with
                // all possible characters.
                foreach (char c in t.LegalMoves(nfa))
                {
                    NFA.State[] moveSet = t.Move(nfa, c);
                    if (moveSet.Any())
                    {
                        // Get the closer of the move set. This is the NFA states that will form the new set
                        IEnumerable<NFA.State> moveClosure = nfa.Closure(moveSet);
                        var newState = new State(moveClosure);

                        // See if the new state already exists. If so change the reference to point to 
                        // the already created object, since we will need to add a transition back to the same object
                        var oldState = dfa.States.FirstOrDefault(f => !f.NfaStates.Except(newState.NfaStates).Any() &&
                                                                      !newState.NfaStates.Except(f.NfaStates).Any());
                        if (oldState == null)
                        {
                            dfa.States.Add(newState);
                        } 
                        else
                        {
                            // New state wasn't that new. We already have one exacly like it in the DFA. Set 
                            // netstate to oldstate so that the created transition will be correct (still need to
                            // create a transition)
                            newState = oldState;
                        }

                        // See if there already is a transition. In that case, add our character to the list
                        // of valid values
                        var transition = dfa.Transitions.SingleOrDefault(f => f.From == t && f.To == newState);
                        if (transition == null) 
                        {
                            // No transition has been found. Create a new one.
                            transition = new Transition<State>(t, newState);
                            dfa.Transitions.Add(transition);                     
                        }

                        transition.ValidInput.Add(c);
                    }
                }
            }

            dfa.StartState = dfa.States[0];
            dfa.AssignStateNumbers();

            return dfa;
        }
    }
}