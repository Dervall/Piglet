using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Piglet.Lexer.Construction
{
    public class NFA : FiniteAutomata<NFA.State>
    {
        // This represents a state transfer that requires no input. These will never be transferred into the DFA
        public const char Epsilon = (char)0;

        public class State : BaseState
        {
            public override bool AcceptState { get; set; }

            public override string ToString()
            {
                return string.Format("{0} {1}", StateNumber, AcceptState ? "ACCEPT" : "");
            }
        }

        public static NFA Create(string postfixRegex)
        {
            var stack = new Stack<NFA>();
            var stringStream = new StringReader(postfixRegex);
            var escaped = false;

            while (stringStream.Peek() != -1)
            {
                var c = (char)stringStream.Read();
                if (escaped)
                {
                    switch (c)
                    {
                        case 'd':
                            // Shorthand for [0-9]
                            stack.Push(AcceptRange('0', '9'));   
                            break;
                        
                        default:
                            stack.Push(AcceptSingle(c));
                            break;
                    }
                    escaped = false;
                }
                else
                {
                    switch (c)
                    {
                        case '|':
                            stack.Push(Or(stack.Pop(), stack.Pop()));
                            break;

                        case '+':
                            stack.Push(RepeatOnceOrMore(stack.Pop()));
                            break;

                        case '*':
                            stack.Push(RepeatZeroOrMore(stack.Pop()));
                            break;

                        case '&':
                            // & is not commutative, and the stack is reversed.
                            var second = stack.Pop();
                            var first = stack.Pop();
                            stack.Push(And(first, second));
                            break;
                        case '\\':
                            // Set escaped state
                            escaped = true;
                            break;
                        default:
                            // Normal character
                            stack.Push(AcceptSingle(c));
                            break;
                    }
                }
            }

            // We should end up with only ONE NFA on the stack or the expression
            // is malformed.
            if (stack.Count() != 1)
            {
                throw new Exception("Malformed postfix regexp expression");
            }

            // Pop it from the stack, and assign each state a number, primarily for debugging purposes,
            // they dont _really_ need it. The state numbers actually used are the one used in the DFA.
            var nfa = stack.Pop();
            nfa.AssignStateNumbers();
            return nfa;
        }

        private static NFA RepeatOnceOrMore(NFA nfa)
        {
            // Add an epsilon transition from the accept state back to the start state
            State oldAcceptState = nfa.States.First(f => f.AcceptState);
            nfa.Transitions.Add(new Transition<State>(oldAcceptState, null, nfa.StartState));

            // Add a new accept state, since we cannot have edges exiting the accept state
            var newAcceptState = new State {AcceptState = true};
            nfa.Transitions.Add(new Transition<State>(oldAcceptState, null, newAcceptState));
            nfa.States.Add(newAcceptState);

            // Clear the accept flag of the old accept state 
            oldAcceptState.AcceptState = false;

            return nfa;
        }

        public static NFA AcceptRange(char start, char end)
        {
            var chars = new char[(end - start) + 1];
            var i = 0;
            for (var c = start; c <= end; ++c )
            {
                chars[i++] = c;
            }
            return Accept(chars);
        }

        public static NFA AcceptSingle(char c)
        {
            var acceptCharacters = new[] { c };

            return Accept(acceptCharacters);
        }

        private static NFA Accept(char[] acceptCharacters)
        {
            // Generate a NFA with a simple path with one state transitioning into an accept state.
            var nfa = new NFA();
            var state = new State();
            nfa.States.Add(state);

            var acceptState = new State {AcceptState = true};
            nfa.States.Add(acceptState);

            nfa.Transitions.Add(new Transition<State>(state, acceptCharacters, acceptState));

            nfa.StartState = state;

            return nfa;
        }

        public static NFA And(NFA first, NFA second)
        {
            // Create a new NFA and use the first NFAs start state as the starting point
            var nfa = new NFA {StartState = first.StartState};

            // Change all links in to first acceptstate to go to seconds 
            // start state
            foreach (var edge in first.Transitions.Where(f => f.To.AcceptState))
            {
                edge.To = second.StartState;
            }

            // Remove acceptstate from first
            first.States.Remove(first.States.First(f => f.AcceptState));

            // Add all states and edges in both NFAs
            // Second NFA already has an accept state, there is no need to create another one
            nfa.AddAll(first);
            nfa.AddAll(second);

            return nfa;
        }

        public static NFA Or(NFA a, NFA b)
        {
            var nfa = new NFA();

            // Composite NFA contains all the and all edges in both NFAs
            nfa.AddAll(a);
            nfa.AddAll(b);

            // Add a start state, link to both NFAs old start state with
            // epsilon links
            nfa.StartState = new State();
            nfa.States.Add(nfa.StartState);

            nfa.Transitions.Add(new Transition<State>(nfa.StartState, null, a.StartState));
            nfa.Transitions.Add(new Transition<State>(nfa.StartState, null, b.StartState));

            // Add a new accept state, link all old accept states to the new accept
            // state with an epsilon link and remove the accept flag
            var newAcceptState = new State { AcceptState = true };
            foreach (var oldAcceptState in nfa.States.Where(f => f.AcceptState))
            {
                oldAcceptState.AcceptState = false;
                nfa.Transitions.Add(new Transition<State>(oldAcceptState, null, newAcceptState));
            }
            nfa.States.Add(newAcceptState);

            return nfa;
        }

        public static NFA RepeatZeroOrMore(NFA input)
        {
            var nfa = new NFA();

            // Add everything from the input
            nfa.AddAll(input);

            // Create a new starting state, link it to the old accept state with Epsilon
            nfa.StartState = new State();
            nfa.States.Add(nfa.StartState);
            State oldAcceptState = input.States.First(f => f.AcceptState);
            nfa.Transitions.Add(new Transition<State>(nfa.StartState, null, oldAcceptState));

            // Add epsilon link from old accept state of input to start, to allow for repetition
            nfa.Transitions.Add(new Transition<State>(oldAcceptState, null, input.StartState));

            // Create new accept state, link old accept state to new accept state with epsilon
            var acceptState = new State { AcceptState = true };
            nfa.States.Add(acceptState);
            oldAcceptState.AcceptState = false;
            nfa.Transitions.Add(new Transition<State>(oldAcceptState, null, acceptState));
            return nfa;
        }

        private void AddAll(NFA nfa)
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

        public IEnumerable<State> Closure(State[] states, ISet<State> visitedStates = null)
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
                (from e in Transitions.Where(f => states.Contains(f.From) && f.ValidInput == null && !visitedStates.Contains(f.To)) select e.To).ToArray();

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
                merged.Transitions.Add(new Transition<State>(state, null, nfa.StartState));
            }

            return merged;
        }
    }
}

