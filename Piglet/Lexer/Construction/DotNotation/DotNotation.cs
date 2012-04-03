using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Piglet.Lexer.Construction.DotNotation
{
    /// <summary>
    /// This is a debug class for obtaining dot notation graphviz graphs for lexer components.
    /// </summary>
    public static class DotNotation
    {
        /// <summary>
        /// Get the DFA and NFA graphs for a given regular expression
        /// </summary>
        /// <param name="regex">Regular expression</param>
        /// <param name="minimize">Minimize the resulting DFA</param>
        /// <param name="nfaString">Dot notation NFA graph</param>
        /// <param name="dfaString">Dot notation DFA graph</param>
        public static void GetDfaAndNfaGraphs(string regex, bool minimize, out string nfaString, out string dfaString)
        {
            GetDfaAndNfaGraphs(regex, null, minimize, out nfaString, out dfaString);
        }

        /// <summary>
        /// Get the DFA and NFA graphs for a given regular expression and highlight active
        /// states for a given input string
        /// </summary>
        /// <param name="regex">Regular expression</param>
        /// <param name="input">Input string</param>
        /// <param name="minimize">Minimize the resulting DFA</param>
        /// <param name="nfaString">Dot notation NFA graph</param>
        /// <param name="dfaString">Dot notation DFA graph</param>
        public static void GetDfaAndNfaGraphs(string regex, string input, bool minimize, out string nfaString, out string dfaString)
        {
            var nfa = NfaBuilder.Create(new ShuntingYard(new RegExLexer(new StringReader(regex))));
            nfaString = nfa.AsDotNotation(input, "NFA");
            var dfa = DFA.Create(nfa);
            if (minimize)
            {
                dfa.Minimize();
            }
            dfaString = dfa.AsDotNotation(input, "DFA");
        }

        /// <summary>
        /// Print the state machine as DOT notation suitable for drawing graphs.
        /// This is a useful debug functionality!!
        /// </summary>
        /// <param name="automata">Automata to generate graph for</param>
        /// <param name="input">Input to highlight the current state with</param>
        /// <param name="graphName">Graph name as specified in notation</param>
        /// <returns></returns>
        internal static string AsDotNotation<TState>(this FiniteAutomata<TState> automata, string input, string graphName = "automata") where TState : FiniteAutomata<TState>.BaseState
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

            IEnumerable<TState> currentStates = Enumerable.Empty<TState>();

            bool matchSuccessful = false; 

            if (!string.IsNullOrEmpty(input))
            {
                var stimulateResult = automata.Stimulate(input);

                matchSuccessful = (input == stimulateResult.Matched);
                
                sb.AppendFormat("\tlabel=\"Matched: {0}\"\n", stimulateResult.Matched.Replace("\"", "\\\""));
                sb.Append("\tlabelloc=top;\n");
                sb.Append("\tlabeljust=center;\n");

                currentStates = stimulateResult.ActiveStates;
            }

            foreach (var transition in automata.Transitions)
            {
                sb.Append(string.Format("\t{0} -> {1} [label=\"{2}\"]\n", 
                    transition.From.StateNumber,
                    transition.To.StateNumber,
                    transition.TransitionLabel().Replace("\\", "\\\\").Replace("\"", "\\\"")));
            }

            foreach (var state in automata.States.Where(f => f.AcceptState || currentStates.Contains(f)))
            {
                sb.AppendFormat("\t{0} [{1}{2}]\n",
                    state.StateNumber,
                    state.AcceptState ? "shape=\"doublecircle\"" : "",
                    currentStates.Contains(state) ?
                    string.Format(" fillcolor=\"{0}\" style=\"filled\"", matchSuccessful ? "green" : "red")
                    : "");
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
