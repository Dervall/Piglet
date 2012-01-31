using System.Collections.Generic;
using System.IO;
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

        private enum LexerState
        {
            Normal,
            BeginCharacterClass,
            NormalEscaped,
            InsideCharacterClass,
            RangeEnd,
            NumberedRepetition
        }

        private class CharacterClassState
        {
            public CharacterClassState()
            {
                ClassChars = new List<char>();
            }

            public IList<char> ClassChars { get; private set; }
            public bool Negated { get; set; }
        }

        private class NumberedRepetitionState
        {
            public NumberedRepetitionState()
            {
                MinRepetitions = -1;
                MaxRepetitions = -1;
                Chars = new List<char>();
            }

            public int MaxRepetitions { get; set; }
            public int MinRepetitions { get; set; }
            public List<char> Chars { get; private set; }
            public int CurrentPart { get; set; }
        }

        public static NFA Create(string postfixRegex)
        {
            var stack = new Stack<NFA>();
            var stringStream = new StringReader(postfixRegex);

            LexerState state = LexerState.Normal;
            var classState = new CharacterClassState();
            var numberedRepetitionState = new NumberedRepetitionState();


            while (stringStream.Peek() != -1)
            {
                var c = (char)stringStream.Read();

                switch (state)
                {
                    case LexerState.Normal:
                        switch (c)
                        {
                            case '\\':
                                state = LexerState.NormalEscaped;
                                break;
                            case '[':
                                state = LexerState.BeginCharacterClass;
                                classState = new CharacterClassState();
                                break;
                            case '{':
                                state = LexerState.NumberedRepetition;
                                numberedRepetitionState = new NumberedRepetitionState();
                                break;
                            case '|':
                                stack.Push(Or(stack.Pop(), stack.Pop()));
                                break;
                            case '+':
                                stack.Push(RepeatOnceOrMore(stack.Pop()));
                                break;
                            case '*':
                                stack.Push(RepeatZeroOrMore(stack.Pop()));
                                break;
                            case '?':
                                stack.Push(RepeatZeroOrOnce(stack.Pop()));
                                break;
                            case '&':
                                // & is not commutative, and the stack is reversed.
                                var second = stack.Pop();
                                var first = stack.Pop();
                                stack.Push(And(first, second));
                                break;
                            case '.':
                                stack.Push(AcceptAnything());
                                break;
                            default:
                                // Normal character
                                stack.Push(AcceptSingle(c));
                                break;
                        }
                        break;
                    case LexerState.NormalEscaped:
                        switch (c)
                        {
                            case 'd':   // Shorthand for [0-9]
                                stack.Push(AcceptRange('0', '9'));
                                break;

                            case 'D':   // Shorthand for [^0-9]
                                stack.Push(Accept(AllCharactersExceptNull.Except(CharRange('0', '9'))));
                                break;

                            case 's':
                                stack.Push(Accept(AllWhitespaceCharacters));
                                break;

                            case 'S':
                                stack.Push(Accept(AllCharactersExceptNull.Except(AllWhitespaceCharacters)));
                                break;

                            case 'w':
                                stack.Push(Accept(CharRange('0', '9').Union(CharRange('a', 'z')).Union(CharRange('A', 'Z'))));
                                break;

                            case 'W':
                                stack.Push(Accept(AllCharactersExceptNull.Except(
                                    CharRange('0', '9').Union(CharRange('a', 'z')).Union(CharRange('A', 'Z')))));
                                break;

                            case 'n':
                                stack.Push(Accept(new [] { '\n' }));
                                break;

                            case 'r':
                                stack.Push(Accept(new[] { '\r' }));
                                break;

                            case '.':
                            case '*':
                            case '|':
                            case '[':
                            case ']':
                            case '+':
                            case '(':
                            case ')':
                            case '\\':
                                stack.Push(AcceptSingle(c));
                                break;

                            default:
                                throw new LexerConstructionException(string.Format("Unknown escaped character '{0}'", c));
                        }
                        state = LexerState.Normal;
                        break;

                    case LexerState.BeginCharacterClass:
                        switch (c)
                        {
                            case '^':
                                if (classState.Negated)
                                {
                                    // If the classstate is ALREADY negated
                                    // Readd the ^ to the expression
                                    classState.ClassChars.Add('^');
                                    state = LexerState.InsideCharacterClass;
                                }
                                classState.Negated = true;
                                break;
                            case '[':
                            case ']':
                            case '-':
                                // This does not break the character class TODO: I THINK!!!
                                classState.ClassChars.Add(c);
                                break;
                            default:
                                classState.ClassChars.Add(c);
                                state = LexerState.InsideCharacterClass;
                                break;
                        }
                        break;
                    
                    case LexerState.InsideCharacterClass:
                        switch (c)
                        {
                            case '-':
                                state = LexerState.RangeEnd;
                                break;
                            case '[':
                                throw new LexerConstructionException("Opening new character class inside an already open one");
                            case ']':
                                // Ending class
                                stack.Push(classState.Negated
                                           ? Accept(AllCharactersExceptNull.Except(classState.ClassChars))
                                           : Accept(classState.ClassChars.Distinct()));
                                state = LexerState.Normal;
                                break;
                            default:
                                classState.ClassChars.Add(c);
                                break;
                        }
                        break;

                    case LexerState.RangeEnd:
                        switch (c)
                        {
                            case ']':
                                // We found the - at the position BEFORE the end of the class
                                // which means we should handle it as a litteral and end the class
                                stack.Push(classState.Negated
                                           ? Accept(AllCharactersExceptNull.Except(classState.ClassChars))
                                           : Accept(classState.ClassChars.Distinct()));
                                state = LexerState.Normal;
                                break;
                            default:
                                char lastClassChar = classState.ClassChars.Last();
                                char from = lastClassChar < c ? lastClassChar : c;
                                char to = lastClassChar < c ? c : lastClassChar;
                                for (; from <= to; ++from)
                                {
                                    classState.ClassChars.Add(from);
                                }
                                break;
                        }
                        break;
                    case LexerState.NumberedRepetition:
                        switch (c)
                        {
                            case '0':   // Is it really OK to start with a 0. It is now.
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                numberedRepetitionState.Chars.Add(c);
                                break;
                            case '}':
                            case ':':
                                // Parse whatever is in Chars
                                int reps;
                                if (!int.TryParse(new string(numberedRepetitionState.Chars.ToArray()), out reps ))
                                {
                                    throw new LexerConstructionException("Numbered repetition operator contains operand that is not a number");
                                }
                                numberedRepetitionState.Chars.Clear();

                                // Set the right value
                                if (numberedRepetitionState.CurrentPart == 0)
                                {
                                    numberedRepetitionState.MinRepetitions = reps;
                                }
                                else
                                {
                                    numberedRepetitionState.MaxRepetitions = reps;
                                }

                                if (c == ':')
                                {
                                    ++numberedRepetitionState.CurrentPart;
                                    if (numberedRepetitionState.CurrentPart > 1)
                                        throw new LexerConstructionException("More than one : in numbered repetition.");
                                }
                                else
                                {
                                    stack.Push(NumberedRepeat(stack.Pop(), numberedRepetitionState.MinRepetitions, numberedRepetitionState.MaxRepetitions));
                                    state = LexerState.Normal;
                                }
                                break;
                            default:
                                throw new LexerConstructionException(
                                    string.Format("Illegal character {0} in numbered repetition", c));
                        }
                        break;
                }
            }

            // We should end up with only ONE NFA on the stack or the expression
            // is malformed.
            if (stack.Count() != 1)
            {
                throw new LexerConstructionException("Malformed postfix regexp expression");
            }

            // Pop it from the stack, and assign each state a number, primarily for debugging purposes,
            // they dont _really_ need it. The state numbers actually used are the one used in the DFA.
            var nfa = stack.Pop();
            nfa.AssignStateNumbers();
            return nfa;
        }

        protected static IEnumerable<char> AllWhitespaceCharacters
        {
            get { return new[] { ' ', '\t', '\n', '\r' }; }
        }

        private static NFA AcceptAnything()
        {
            return Accept(AllCharactersExceptNull);
        }

        protected static IEnumerable<char> AllCharactersExceptNull
        {
            get
            {
                for (var c = (char)1; c < 256; ++c)
                    yield return c;
            }
        }

        private static NFA RepeatOnceOrMore(NFA nfa)
        {
            // Add an epsilon transition from the accept state back to the start state
            State oldAcceptState = nfa.States.First(f => f.AcceptState);
            nfa.Transitions.Add(new Transition<State>(oldAcceptState, nfa.StartState));

            // Add a new accept state, since we cannot have edges exiting the accept state
            var newAcceptState = new State { AcceptState = true };
            nfa.Transitions.Add(new Transition<State>(oldAcceptState, newAcceptState));
            nfa.States.Add(newAcceptState);

            // Clear the accept flag of the old accept state 
            oldAcceptState.AcceptState = false;

            return nfa;
        }

        public static NFA AcceptRange(char start, char end)
        {
            var chars = CharRange(start, end);
            return Accept(chars);
        }

        private static IEnumerable<char> CharRange(char start, char end)
        {
            for (var c = start; c <= end; ++c)
            {
                yield return c;
            }
        }

        public static NFA AcceptSingle(char c)
        {
            var acceptCharacters = new[] { c };

            return Accept(acceptCharacters);
        }

        private static NFA Accept(IEnumerable<char> acceptCharacters)
        {
            // Generate a NFA with a simple path with one state transitioning into an accept state.
            var nfa = new NFA();
            var state = new State();
            nfa.States.Add(state);

            var acceptState = new State { AcceptState = true };
            nfa.States.Add(acceptState);

            nfa.Transitions.Add(new Transition<State>(state, acceptState, acceptCharacters));

            nfa.StartState = state;

            return nfa;
        }

        public static NFA And(NFA first, NFA second)
        {
            // Create a new NFA and use the first NFAs start state as the starting point
            var nfa = new NFA { StartState = first.StartState };

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

            nfa.Transitions.Add(new Transition<State>(nfa.StartState, a.StartState));
            nfa.Transitions.Add(new Transition<State>(nfa.StartState, b.StartState));

            // Add a new accept state, link all old accept states to the new accept
            // state with an epsilon link and remove the accept flag
            var newAcceptState = new State { AcceptState = true };
            foreach (var oldAcceptState in nfa.States.Where(f => f.AcceptState))
            {
                oldAcceptState.AcceptState = false;
                nfa.Transitions.Add(new Transition<State>(oldAcceptState, newAcceptState));
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
            nfa.Transitions.Add(new Transition<State>(nfa.StartState, oldAcceptState));

            // Add epsilon link from old accept state of input to start, to allow for repetition
            nfa.Transitions.Add(new Transition<State>(oldAcceptState, input.StartState));

            // Create new accept state, link old accept state to new accept state with epsilon
            var acceptState = new State { AcceptState = true };
            nfa.States.Add(acceptState);
            oldAcceptState.AcceptState = false;
            nfa.Transitions.Add(new Transition<State>(oldAcceptState, acceptState));
            return nfa;
        }

        private static NFA RepeatZeroOrOnce(NFA nfa)
        {
            // Easy enough, add an epsilon transition from the start state
            // to the end state. Done
            nfa.Transitions.Add(new Transition<State>(nfa.StartState, nfa.States.First(f => f.AcceptState)));
            return nfa;
        }

        private static NFA NumberedRepeat(NFA nfa, int minRepetitions, int maxRepetitions)
        {
            if (maxRepetitions < minRepetitions)
            {
                maxRepetitions = minRepetitions;
            }

            // Copy the NFA max repetitions times, link them together.
            NFA output = nfa.Copy();
            var epsilonLinkStates = new Stack<State>();
            for (int i = 1; i < maxRepetitions; ++i)
            {
                NFA newNfa = nfa.Copy();
                if (i >= minRepetitions)
                {
                    epsilonLinkStates.Push(newNfa.StartState);
                }
                output = And(output, newNfa);
            }

            // Add epsilon transitions from accept to beginning states of NFAs in the chain
            var acceptState = output.States.Single(f => f.AcceptState);
            while (epsilonLinkStates.Any())
            {
                output.Transitions.Add(new Transition<State>(epsilonLinkStates.Pop(),
                                                             acceptState));
            }

            return output;
        }

        private NFA Copy()
        {
            var newNFA = new NFA();
            var stateMap = new Dictionary<State, State>();

            foreach (var state in States)
            {
                var newState = new State {AcceptState = state.AcceptState, StateNumber = state.StateNumber};
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
    }
}

