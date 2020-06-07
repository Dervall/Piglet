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
        /// <param name="ignoreCase">Determines whether the regular expression is case-insensitive</param>
        /// <param name="nfaString">Dot notation NFA graph</param>
        /// <param name="dfaString">Dot notation DFA graph</param>
        public static void GetDfaAndNfaGraphs(string regex, bool minimize, bool ignoreCase, out string nfaString, out string dfaString) => GetDfaAndNfaGraphs(regex, null, minimize, ignoreCase, out nfaString, out dfaString);

        /// <summary>
        /// Get the DFA and NFA graphs for a given regular expression and highlight active
        /// states for a given input string
        /// </summary>
        /// <param name="regex">Regular expression</param>
        /// <param name="input">Input string</param>
        /// <param name="minimize">Minimize the resulting DFA</param>
        /// <param name="ignoreCase">Determines whether the regular expression is case-insensitive</param>
        /// <param name="nfaString">Dot notation NFA graph</param>
        /// <param name="dfaString">Dot notation DFA graph</param>
        public static void GetDfaAndNfaGraphs(string regex, string? input, bool minimize, bool ignoreCase, out string nfaString, out string dfaString)
        {
            NFA nfa = NfaBuilder.Create(new ShuntingYard(new RegexLexer(new StringReader(regex)), ignoreCase));
            nfaString = nfa.AsDotNotation(input, "NFA");
            DFA dfa = DFA.Create(nfa);

            if (minimize)
                dfa.Minimize();

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
        internal static string AsDotNotation<TState>(this FiniteAutomata<TState> automata, string? input, string graphName = "automata")
            where TState : FiniteAutomata<TState>.BaseState
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

            StringBuilder sb = new StringBuilder();

            sb.Append("digraph " + graphName + " {\n");
            sb.Append("\t[node shape=\"circle\"]\n");
            sb.Append("\tgraph [rankdir=\"LR\"]\n");

            IEnumerable<TState>? currentStates = Enumerable.Empty<TState>();

            bool matchSuccessful = false; 

            if (!string.IsNullOrEmpty(input))
            {
                StimulateResult<TState> stimulateResult = automata.Stimulate(input);

                matchSuccessful = input == stimulateResult.Matched;
                
                sb.AppendFormat("\tlabel=\"Matched: {0}\"\n", stimulateResult.Matched?.Replace("\"", "\\\""));
                sb.Append("\tlabelloc=top;\n");
                sb.Append("\tlabeljust=center;\n");

                currentStates = stimulateResult.ActiveStates;
            }

            foreach (Transition<TState> transition in automata.Transitions)
                sb.Append($"\t{transition.From.StateNumber} -> {transition.To.StateNumber} [label=\"{transition.TransitionLabel().Replace("\\", "\\\\").Replace("\"", "\\\"")}\"]\n");

            foreach (TState state in automata.States.Where(f => f.AcceptState || (currentStates?.Contains(f) ?? false)))
                sb.AppendFormat("\t{0} [{1}{2}]\n",
                    state.StateNumber,
                    state.AcceptState ? "shape=\"doublecircle\"" : "",
                    (currentStates?.Contains(state) ?? false) ?
                    $" fillcolor=\"{(matchSuccessful ? "green" : "red")}\" style=\"filled\"" : "");

            sb.Append("}");

            return sb.ToString();
        }

        /// <summary>
        /// DOT language label name for transitions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transition"></param>
        /// <returns></returns>
        internal static string TransitionLabel<T>(this Transition<T> transition) => transition.ValidInput.ToString();
    }
}
