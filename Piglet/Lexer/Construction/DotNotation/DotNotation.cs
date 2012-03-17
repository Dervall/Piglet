using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Piglet.Lexer.Construction.DotNotation
{
    public static class DotNotation
    {
        public static void GetDfaAndNfaGraphs(string regex, out string nfaString, out string dfaString)
        {
            // Make sure it does not crash and does not return null.
            var nfa = NfaBuilder.Create(new ShuntingYard(new RegExLexer(new StringReader(regex))));
            nfaString = nfa.AsDotNotation();
            var dfa = DFA.Create(nfa);
            dfaString = dfa.AsDotNotation();
        }

        /// <summary>
        /// Print the state machine as DOT notation suitable for drawing graphs.
        /// This is a useful debug functionality!!
        /// 
        /// http://hughesbennett.co.uk/Graphviz copy and paste your text to view graph
        /// 
        /// </summary>
        /// <param name="automata">Automata to generate graph for</param>
        /// <param name="graphName">Graph name as specified in notation</param>
        /// <returns></returns>
        internal static string AsDotNotation<TState>(this FiniteAutomata<TState> automata, string graphName = "automata") where TState : FiniteAutomata<TState>.BaseState
        {
            // Draw the *FA as a directed graph with the state numbers in circles
            // Use a double circle for accepting states
            //
            // digraph graphname {
            // digraph G {
            //   [node shape="circle"]
            //   1 [shape="doublecircle"]
            //  1 -> 2 [label=a]
            //}

            var sb = new StringBuilder();

            sb.Append("digraph " + graphName + " {\n");
            sb.Append("\t[node shape=\"circle\"]\n");
            sb.Append("\tgraph [rankdir=\"LR\"]\n");

            foreach (var state in automata.States.Where(f=>f.AcceptState))
            {
                sb.Append(string.Format("\t{0} [shape=\"doublecircle\"]\n", state.StateNumber));
            }
            
            foreach (var transition in automata.Transitions)
            {
                sb.Append(string.Format("\t{0} -> {1} [label=\"{2}\"]\n", 
                    transition.From.StateNumber,
                    transition.To.StateNumber,
                    transition.TransitionLabel().Replace("\\", "\\\\").Replace("\"", "\\\"")));
            }
            sb.Append("}");

            return sb.ToString();
        }


        /// <summary>
        /// DOT language label name for transitions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transition"></param>
        /// <returns></returns>
        internal static string TransitionLabel<T>(this Transition<T> transition )
        {
            if ( !transition.ValidInput.Any()) return "ε";

            if (transition.ValidInput.Count == 1)
                return transition.ValidInput.First().ToString();

            return string.Join(", ", ValidInputToTransitionLabel(transition.ValidInput));
        }

        private static string ToGraphSafeString(this char c)
        {
            return c >= 33 && c <= 0x7e
                ? c.ToString()
                : string.Format("0x{0:x2}", (int) c);
        }

        private static IEnumerable<string> ValidInputToTransitionLabel(IEnumerable<char> validInput)
        {
            var input = validInput.OrderBy(f => f).ToArray();
            char start = input[0];
            for (int i = 1; i < input.Length + 1; ++i)
            {
                if (i == input.Length || input[i] != input[i - 1] + 1)
                {
                    char end = input[i - 1];
                    if ((end - start) > 0)
                    {
                        yield return string.Format("{0}-{1}",
                                                   start.ToGraphSafeString(),
                                                   end.ToGraphSafeString());
                    }
                    else
                    {
                        yield return start.ToGraphSafeString();
                    }
                    if(i != input.Length) 
                        start = input[i];
                }
            }
        }
    }
}
