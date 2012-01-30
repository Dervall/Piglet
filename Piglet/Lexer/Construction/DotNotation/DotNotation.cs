using System.Linq;
using System.Text;

namespace Piglet.Lexer.Construction.DotNotation
{
    internal static class DotNotation
    {
        /// <summary>
        /// Print the state machine as DOT notation suitable for drawing graphs.
        /// This is a useful debug functionality!!
        /// 
        /// http://hughesbennett.co.uk/Graphviz copy and paste your text to view graph
        /// 
        /// </summary>
        /// <param name="automata"></param>
        /// <param name="graphName"></param>
        /// <returns></returns>
        public static string AsDotNotation<TState>(this FiniteAutomata<TState> automata, string graphName = "automata") where TState : FiniteAutomata<TState>.BaseState
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

            foreach (var state in automata.States.Where(f=>f.AcceptState))
            {
                sb.Append(string.Format("{0} [shape=\"doublecircle\"]\n", state.StateNumber));
            }
            
            foreach (var transition in automata.Transitions)
            {
                sb.Append(string.Format("\t{0} -> {1} [label={2}]\n", 
                    transition.From.StateNumber,
                    transition.To.StateNumber,
                    transition.TransitionLabel() ));
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
        public static string TransitionLabel<T>(this Transition<T> transition )
        {
            if ( !transition.ValidInput.Any()) return "ε";

            if (transition.ValidInput.Count == 1)
                return transition.ValidInput.First().ToString();
            return string.Join(", ", transition.ValidInput);
        }
    }
}
