using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Piglet.Lexer.Construction
{
    internal abstract class FiniteAutomata<TState>
        where TState : FiniteAutomata<TState>.BaseState
    {
        public IList<TState> States { get; set; }
        public IList<Transition<TState>>  Transitions { get; set; }
        public TState StartState { get; set; }


        protected FiniteAutomata()
        {
            States = new List<TState>();
            Transitions = new List<Transition<TState>>();
        }

        public abstract IEnumerable<TState> Closure(TState[] states, ISet<TState> visitedStates = null);

        public void AssignStateNumbers()
        {
            int i = 0;

            foreach (TState state in States)
                if (state != StartState)
                    state.StateNumber = ++i;

            // Always use 0 as the start state
            StartState.StateNumber = 0;
        }

        public void DistinguishValidInputs()
        {
            List<CharRange> ranges = new List<CharRange>(Transitions.SelectMany(f => f.ValidInput.Ranges));
            char[] beginningsAndEnds = ranges.Select(f => f.From).Concat(ranges.Select(f => f.To == char.MaxValue ? f.To : (char)(f.To+1))).ToArray();
            Array.Sort(beginningsAndEnds);
            int pivot = 0;

            for (int i = 1; i < beginningsAndEnds.Length; ++i)
                if (beginningsAndEnds[i] != beginningsAndEnds[pivot])
                    beginningsAndEnds[++pivot] = beginningsAndEnds[i];

            ++pivot;

            List<CharRange> distinguishedRanges = new List<CharRange>(pivot * 2);

            for(int i = 1; i < pivot; ++i)
                distinguishedRanges.Add(new CharRange
                {
                    From = beginningsAndEnds[i - 1],
                    To = beginningsAndEnds[i]
                });

            foreach (Transition<TState> transition in Transitions)
                transition.ValidInput = new CharSet(transition.ValidInput.Ranges.SelectMany(range => FindNewRanges(range, distinguishedRanges)));
        }

        private IEnumerable<CharRange> FindNewRanges(CharRange range, List<CharRange> distinguishedRanges)
        {
            int a = 0;
            int b = distinguishedRanges.Count;
            int startIndex;

            while (true)
            {
                int pivot = a + (b - a) / 2;
                int cmp = range.From - distinguishedRanges[pivot].From;

                if (cmp == 0)
                {
                    startIndex = pivot;

                    break;
                }
                
                if (cmp < 0)
                    b = pivot;
                else
                    a = pivot;
            }

            int a2 = startIndex;
            int b2 = distinguishedRanges.Count;
            char c = range.To == char.MaxValue ? range.To : (char) (range.To + 1);

            while (true)
            {
                int pivot = a2 + (b2 - a2) / 2;
                int cmp = c - distinguishedRanges[pivot].To;

                if (cmp == 0)
                {
                    for (int i = startIndex; i <= pivot; ++i)
                    {
                        CharRange f = distinguishedRanges[i];

                        yield return new CharRange
                        {
                            From = f.From,
                            To = f.To == char.MaxValue ? f.To : (char)(f.To - 1)
                        };
                    }

                    yield break;
                }

                if (cmp < 0)
                    b2 = pivot;
                else
                    a2 = pivot;
            }
        }

        public StimulateResult<TState> Stimulate(string input)
        {
            List<TState> activeStates = Closure(new[] {StartState}).ToList();
            StringBuilder matchedString = new StringBuilder();

            foreach (char c in input)
            {
                HashSet<TState> toStates = new HashSet<TState>();

                foreach (TState activeState in activeStates)
                    toStates.UnionWith(from t in Transitions
                                       where t.From == activeState
                                       where t.ValidInput.ContainsChar(c)
                                       select t.To);

                if (toStates.Any())
                {
                    matchedString.Append(c);
                    activeStates = Closure(toStates.ToArray()).ToList();
                }
                else
                    break;
            }

            return new StimulateResult<TState>
            {
                Matched = matchedString.ToString(),
                ActiveStates = activeStates
            };
        }

        public abstract class BaseState
        {
            public abstract bool AcceptState { get; set; }
            public int StateNumber { get; set; }
        }
    }

    internal class StimulateResult<TState>
        where TState : FiniteAutomata<TState>.BaseState
    {
        public string Matched { get; set; }
        public IEnumerable<TState> ActiveStates { get; set; }
    }
}
