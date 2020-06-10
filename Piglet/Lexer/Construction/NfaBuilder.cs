using System.Collections.Generic;
using System.Linq;

namespace Piglet.Lexer.Construction
{
    internal class NfaBuilder
    {
        public static NFA Create(ShuntingYard yard)
        {
            Stack<NFA> stack = new Stack<NFA>();

            foreach (RegexToken token in yard.ShuntedTokens())
            {
                switch (token.Type)
                {
                    case RegexTokenType.OperatorMul:
                        stack.Push(RepeatZeroOrMore(stack.Pop()));

                        break;
                    case RegexTokenType.OperatorQuestion:
                        stack.Push(RepeatZeroOrOnce(stack.Pop()));

                        break;
                    case RegexTokenType.OperatorOr:
                        stack.Push(Or(stack.Pop(), stack.Pop()));

                        break;
                    case RegexTokenType.OperatorPlus:
                        stack.Push(RepeatOnceOrMore(stack.Pop()));

                        break;
                    case RegexTokenType.Accept:
                        stack.Push(Accept(token.Characters));

                        break;
                    case RegexTokenType.OperatorConcat:
                        // & is not commutative, and the stack is reversed.
                        NFA second = stack.Pop();
                        NFA first = stack.Pop();

                        stack.Push(And(first, second));

                        break;
                    case RegexTokenType.NumberedRepeat:
                        stack.Push(NumberedRepeat(stack.Pop(), token.MinRepetitions, token.MaxRepetitions));

                        break;
                    default:
                        throw new LexerConstructionException("Unknown operator!");
                }
            }

            // We should end up with only ONE NFA on the stack or the expression is malformed.
            if (stack.Count() != 1)
                throw new LexerConstructionException("Malformed regex expression.");

            // Pop it from the stack, and assign each state a number, primarily for debugging purposes,
            // they dont _really_ need it. The state numbers actually used are the one used in the DFA.
            NFA nfa = stack.Pop();

            nfa.AssignStateNumbers();

            return nfa;
        }

        private static NFA RepeatOnceOrMore(NFA nfa)
        {
            // Add an epsilon transition from the accept state back to the start state
            NFA.State oldAcceptState = nfa.States.First(f => f.AcceptState);
            nfa.Transitions.Add(new Transition<NFA.State>(oldAcceptState, nfa.StartState));

            // Add a new accept state, since we cannot have edges exiting the accept state
            NFA.State newAcceptState = new NFA.State { AcceptState = true };
            nfa.Transitions.Add(new Transition<NFA.State>(oldAcceptState, newAcceptState));
            nfa.States.Add(newAcceptState);

            // Clear the accept flag of the old accept state 
            oldAcceptState.AcceptState = false;

            return nfa;
        }

        private static NFA Accept(CharSet acceptCharacters)
        {
            // Generate a NFA with a simple path with one state transitioning into an accept state.
            NFA nfa = new NFA();
            NFA.State state = new NFA.State();

            nfa.States.Add(state);

            NFA.State acceptState = new NFA.State { AcceptState = true };

            nfa.States.Add(acceptState);
            nfa.Transitions.Add(new Transition<NFA.State>(state, acceptState, acceptCharacters));
            nfa.StartState = state;

            return nfa;
        }

        public static NFA And(NFA first, NFA second)
        {
            // Create a new NFA and use the first NFAs start state as the starting point
            NFA nfa = new NFA { StartState = first.StartState };

            // Change all links in to first acceptstate to go to seconds start state
            foreach (Transition<NFA.State> edge in first.Transitions.Where(f => f.To?.AcceptState ?? false))
                edge.To = second.StartState;

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
            NFA nfa = new NFA();

            // Composite NFA contains all the and all edges in both NFAs
            nfa.AddAll(a);
            nfa.AddAll(b);

            // Add a start state, link to both NFAs old start state with epsilon links
            nfa.StartState = new NFA.State();
            nfa.States.Add(nfa.StartState);

            nfa.Transitions.Add(new Transition<NFA.State>(nfa.StartState, a.StartState));
            nfa.Transitions.Add(new Transition<NFA.State>(nfa.StartState, b.StartState));

            // Add a new accept state, link all old accept states to the new accept state with an epsilon link and remove the accept flag
            NFA.State newAcceptState = new NFA.State { AcceptState = true };

            foreach (NFA.State oldAcceptState in nfa.States.Where(f => f.AcceptState))
            {
                oldAcceptState.AcceptState = false;
                nfa.Transitions.Add(new Transition<NFA.State>(oldAcceptState, newAcceptState));
            }

            nfa.States.Add(newAcceptState);

            return nfa;
        }

        public static NFA RepeatZeroOrMore(NFA input)
        {
            NFA nfa = new NFA();

            // Add everything from the input
            nfa.AddAll(input);

            // Create a new starting state, link it to the old accept state with Epsilon
            nfa.StartState = new NFA.State();
            nfa.States.Add(nfa.StartState);

            NFA.State oldAcceptState = input.States.First(f => f.AcceptState);

            nfa.Transitions.Add(new Transition<NFA.State>(nfa.StartState, oldAcceptState));

            // Add epsilon link from old accept state of input to start, to allow for repetition
            nfa.Transitions.Add(new Transition<NFA.State>(oldAcceptState, input.StartState));

            // Create new accept state, link old accept state to new accept state with epsilon
            NFA.State acceptState = new NFA.State { AcceptState = true };

            nfa.States.Add(acceptState);
            oldAcceptState.AcceptState = false;
            nfa.Transitions.Add(new Transition<NFA.State>(oldAcceptState, acceptState));

            return nfa;
        }

        private static NFA RepeatZeroOrOnce(NFA nfa)
        {
            // Easy enough, add an epsilon transition from the start state to the end state. Done
            nfa.Transitions.Add(new Transition<NFA.State>(nfa.StartState, nfa.States.First(f => f.AcceptState)));

            return nfa;
        }

        private static NFA NumberedRepeat(NFA nfa, int minRepetitions, int maxRepetitions)
        {
            // To create a suitable expression, the special case of infinite max repetitions must be separately handled.
            bool infiniteMax = false;

            if (maxRepetitions == int.MaxValue)
            {
                infiniteMax = true;
                maxRepetitions = minRepetitions;
            }
            else if (maxRepetitions < minRepetitions)
                maxRepetitions = minRepetitions;

            // Copy the NFA max repetitions times, link them together.
            NFA output = nfa.Copy();
            Stack<NFA.State> epsilonLinkStates = new Stack<NFA.State>();

            for (int i = 1; i < maxRepetitions; ++i)
            {
                NFA newNfa = nfa.Copy();

                if (i >= minRepetitions || (infiniteMax && i == maxRepetitions - 1))
                    epsilonLinkStates.Push(newNfa.StartState);

                output = And(output, newNfa);
            }

            if (infiniteMax)
            {
                // Use Single to force an exception if this has gone astray
                NFA.State finalState = epsilonLinkStates.Single();

                // Make a little epsilon loop from the final accept state to the start state of the final state
                output.Transitions.Add(new Transition<NFA.State>(output.States.Single(f => f.AcceptState), finalState));
            }
            else
            {
                // Add epsilon transitions from accept to beginning states of NFAs in the chain
                NFA.State acceptState = output.States.Single(f => f.AcceptState);

                while (epsilonLinkStates.Any())
                    output.Transitions.Add(new Transition<NFA.State>(epsilonLinkStates.Pop(), acceptState));
            }

            return output;
        }
    }
}