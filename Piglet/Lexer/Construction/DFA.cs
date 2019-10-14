using System.Collections.Generic;
using System.Linq;
using System;

namespace Piglet.Lexer.Construction
{
    internal sealed class DFA
        : FiniteAutomata<DFA.State>
    {
        public sealed class State
            : BaseState
        {
            public ISet<NFA.State> NfaStates { get; private set; }
            public bool Mark { get; set; }


            public State(ISet<NFA.State> nfaStates) => NfaStates = nfaStates;

            public IEnumerable<CharRange> LegalMoves(Transition<NFA.State>[] fromTransitions) =>
                fromTransitions.SelectMany(f => f.ValidInput.Ranges).Distinct();

            // Purely for debugging purposes
            public override string ToString() => $"{StateNumber} {{{string.Join(", ", NfaStates)}}}";

            public override bool AcceptState
            {
                get => NfaStates.Any(f => f.AcceptState);
                set { }  // Do nothing, cannot set
            }
        }

        public static DFA Create(NFA nfa)
        {
            IDictionary<NFA.State, ISet<NFA.State>> closures = nfa.GetAllClosures();

            // The valid input ranges that the NFA contains will need to be split up so that
            // the smallest possible units which NEVER overlaps will be contained in each of the
            // states
            nfa.DistinguishValidInputs();

            // Get the closure set of S0
            DFA dfa = new DFA();

            dfa.States.Add(new State(closures[nfa.StartState]));
            
            while (true)
            {
                // Get an unmarked state in dfaStates
                State t = dfa.States.FirstOrDefault(f => !f.Mark);

                if (t is null)
                    break; // We're done!

                t.Mark = true;

                // Get the move states by stimulating this DFA state with
                // all possible characters.
                Transition<NFA.State>[] fromTransitions = nfa.Transitions.Where(f => t.NfaStates.Contains(f.From)).ToArray();
                Dictionary<CharRange, List<NFA.State>> moveDestinations = new Dictionary<CharRange, List<NFA.State>>();

                foreach (Transition<NFA.State> fromTransition in fromTransitions)
                    foreach (CharRange range in fromTransition.ValidInput.Ranges)
                    {
                        if (!moveDestinations.TryGetValue(range, out List<NFA.State> destList))
                        {
                            destList = new List<NFA.State>();
                            moveDestinations.Add(range, destList);
                        }

                        destList.Add(fromTransition.To);
                    }

                foreach (CharRange c in t.LegalMoves(fromTransitions))
                {
                    List<NFA.State> moveSet = moveDestinations[c];

                    if (moveSet.Any())
                    {
                        // Get the closure of the move set. This is the NFA states that will form the new set
                        ISet<NFA.State> moveClosure = new HashSet<NFA.State>();

                        foreach (NFA.State moveState in moveSet)
                            moveClosure.UnionWith(closures[moveState]);

                        State newState = new State(moveClosure);

                        // See if the new state already exists. If so change the reference to point to 
                        // the already created object, since we will need to add a transition back to the same object
                        State oldState = dfa.States.FirstOrDefault(f => f.NfaStates.SetEquals(newState.NfaStates));/* f.NfaStates.Count == newState.NfaStates.Count && 
                                                                      !f.NfaStates.Except(newState.NfaStates).Any() &&
                                                                      !newState.NfaStates.Except(f.NfaStates).Any());*/
                        if (oldState is null)
                            dfa.States.Add(newState);
                        else
                            // New state wasn't that new. We already have one exacly like it in the DFA. Set 
                            // netstate to oldstate so that the created transition will be correct (still need to
                            // create a transition)
                            newState = oldState;

                        // See if there already is a transition. In that case, add our character to the list
                        // of valid values
                        Transition<State> transition = dfa.Transitions.SingleOrDefault(f => f.From == t && f.To == newState);

                        if (transition is null) 
                        {
                            // No transition has been found. Create a new one.
                            transition = new Transition<State>(t, newState);
                            dfa.Transitions.Add(transition);                     
                        }

                        transition.ValidInput.AddRange(c.From, c.To, false);
                    }
                }
            }

            dfa.StartState = dfa.States[0];
            dfa.AssignStateNumbers();

            return dfa;
        }

        public void Minimize()
        {
            TriangularTable<int, State> distinct = new TriangularTable<int, State>(States.Count, f => f.StateNumber );
            distinct.Fill(-1); // Fill with empty states
            
            // Create a function for the distinct state pairs and performing an action on them
            Action<Action<State, State>> distinctStatePairs = action =>
            {
                for (int i = 0; i < States.Count; ++i)
                {
                    State p = States[i];

                    for (int j = i + 1; j < States.Count; ++j)
                    {
                        State q = States[j];

                        action(p, q);
                    }
                }        
            };

            // Get a set of all valid input ranges that we have in the DFA
            ISet<CharRange> allValidInputs = new HashSet<CharRange>();

            foreach (Transition<State> transition in Transitions)
                allValidInputs.UnionWith(transition.ValidInput.Ranges);

            // For every distinct pair of states, if one of them is an accepting state and the other one is not set the distinct 
            distinctStatePairs((p, q) =>
            {
                bool pIsAcceptState = p.AcceptState;
                bool bIsAcceptState = q.AcceptState;

                if (bIsAcceptState && pIsAcceptState)
                {
                    // If both are accepting states, then we might have an issue merging them.
                    // this is because we use multiple regular expressions with different endings when
                    // constructing lexers.
                    List<NFA.State> pAcceptStates = p.NfaStates.Where(f => f.AcceptState).ToList();
                    List<NFA.State> qAcceptStates = q.NfaStates.Where(f => f.AcceptState).ToList();

                    if (pAcceptStates.Count() == qAcceptStates.Count())
                    {
                        foreach (NFA.State pAcceptState in pAcceptStates)
                            if (!qAcceptStates.Contains(pAcceptState))
                                distinct[p, q] = int.MaxValue; // Since the accepting states differ, its not cool to merge these two states.
                    }
                    else
                        distinct[p, q] = int.MaxValue; // Not the same number of states, not cool to merge
                }

                if (pIsAcceptState ^ bIsAcceptState)
                    distinct[p, q] = int.MaxValue;
            });

            // Make a dictionary of from transitions. This is well worth the time, since this gets accessed lots of times.
            Dictionary<State, Dictionary<CharRange, State>> targetDict = new Dictionary<State, Dictionary<CharRange, State>>();
            foreach (Transition<State> transition in Transitions)
            {
                targetDict.TryGetValue(transition.From, out Dictionary<CharRange, State>? toDict);

                if (toDict is null)
                {
                    toDict = new Dictionary<CharRange, State>();

                    targetDict.Add(transition.From, toDict);
                }

                foreach (CharRange range in transition.ValidInput.Ranges)
                    toDict.Add(range, transition.To);
            }

            // Start iterating
            bool changes;

            do
            {
                changes = false;

                distinctStatePairs((p, q) =>
                {
                    if (distinct[p, q] == -1) 
                    {
                        State? targetState(State state, CharRange c) => targetDict.TryGetValue(state, out Dictionary<CharRange, State>? charDict) &&
                                                                        charDict.TryGetValue(c, out State? toState) ? toState : null;

                        foreach (CharRange a in allValidInputs)
                        {
                            State? qa = targetState(q, a);
                            State? pa = targetState(p, a);

                            if (pa is null ^ qa is null)
                            {
                                // If one of them has a transition on this character range but the other one doesn't then
                                // they are separate.
                                distinct[p, q] = a.GetHashCode();
                                changes = true;

                                break;
                            }
                            
                            // If both are null, then we carry on. The other one is null implictly since we have XOR checked it earlier
                            if (qa is null)
                                continue;

                            if (distinct[qa, pa] != -1)
                            {
                                distinct[p, q] = a.GetHashCode();
                                changes = true;

                                break;
                            }
                        }                           
                    }
                });
            }
            while (changes);

            // Merge states that still have blank square
            // To make this work we have to bunch states together since the indices will be screwed up
            List<ISet<State>> mergeSets = new List<ISet<State>>();
            Func<State, ISet<State>>? findMergeList = s => mergeSets.FirstOrDefault(m => m.Contains(s));

            distinctStatePairs((p, q) =>
            {
                // No need to check those that we have already determined to be distinct
                if (distinct[p, q] != -1)
                    return;

                // These two states are supposed to merge! See if p or q is already part of a merge list!
                ISet<State>? pMergeSet = findMergeList(p);
                ISet<State>? qMergeSet = findMergeList(q);

                if (pMergeSet is null && qMergeSet is null)
                    mergeSets.Add(new HashSet<State> { p, q }); // No previous set for either. Add a new merge set
                else if (pMergeSet != null && qMergeSet is null)
                    pMergeSet.Add(q); // Add q to pMergeSet
                else if (pMergeSet is null)
                    qMergeSet.Add(p); // Add p to qMergeSet
                else if (pMergeSet != qMergeSet)
                {
                    // Both previously have merge sets
                    // If its not the same set (which it shoudln't be) then add their union

                    // Union everything into the pMergeSet
                    pMergeSet.UnionWith(qMergeSet);
                            
                    // Remove the qMergeSet
                    mergeSets.Remove(qMergeSet);
                }
            });

            // Armed with the merge sets, we can now do the actual merge
            foreach (ISet<State> mergeSet in mergeSets)
            {
                // The lone state that should remain is the FIRST set in the mergeset
                List<State> stateList = mergeSet.ToList();
                State outputState = stateList[0];

                // If this statelist contains the startstate, the new startstate will have to be the new output state
                if (stateList.Contains(StartState))
                    StartState = outputState;

                // Iterate over all the states in the merge list except for the one we have decided to merge everything into.
                for (int i = 1; i < stateList.Count; ++i)
                {
                    State toRemove = stateList[i];
                    // Find all transitions that went to this state
                    List<Transition<State>> toTransitions = Transitions.Where(f => f.To == toRemove).ToList();

                    foreach (Transition<State> transition in toTransitions)
                    {
                        // There can be two cases here, either there already is a new transition to be found, in
                        // which case we can merge the valid input instead. The alternative is that there is no prior
                        // transition, in which case we repoint our transition to the output state.
                        Transition<State> existingTransition = Transitions.FirstOrDefault(f => f.From == transition.From && f.To == outputState);
                    
                        if (existingTransition != null)
                        {
                            existingTransition.ValidInput.UnionWith(transition.ValidInput);
                            Transitions.Remove(transition); // Remove the old transition
                        }
                        else
                            transition.To = outputState;
                    }

                    // Find all transitions that went from this state
                    List<Transition<State>> fromTransitions = Transitions.Where(f => f.From == toRemove).ToList();

                    foreach (Transition<State> transition in fromTransitions)
                    {
                        // Same two cases as the code above
                        Transition<State> existingTransition = Transitions.FirstOrDefault(f => f.From == outputState && f.To == transition.To);
                    
                        if (existingTransition != null)
                        {
                            existingTransition.ValidInput.UnionWith(transition.ValidInput);
                            Transitions.Remove(transition); // Remove the old transition
                        }
                        else
                            transition.From = outputState;
                    }

                    // Since before removing this state, we need to merge the list of NFA states that created both of these states
                    foreach (NFA.State nfaState in toRemove.NfaStates)
                        if (!outputState.NfaStates.Contains(nfaState))
                            outputState.NfaStates.Add(nfaState);

                    // There should be no more references to this state. It can thus be removed.
                    States.Remove(toRemove);
                }
            }

            // The states now need to be renumbered
            AssignStateNumbers();
        }

        public override IEnumerable<State> Closure(State[] states, ISet<State>? visitedStates = null) => states;
    }
}
