using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Piglet.Parser.Construction.Debug
{
    public static class DotNotation
    {
        internal static string AsDotNotation<T>(this IEnumerable<ParserBuilder<T>.GotoSetTransition> transitions, List<Lr1ItemSet<T>> itemSets)
        {
            var graph = new StringBuilder();
            graph.Append("digraph goto {");

            foreach (var transition in transitions)
            {
                graph.Append(string.Format("\t\"I{0}\" -> \"I{1}\" [label=\"{2}\"]\n",
                    itemSets.IndexOf(transition.From),
                    itemSets.IndexOf(transition.To),
                    (transition.OnSymbol.DebugName??"").Replace("\\", "\\\\").Replace("\"", "\\\"")));
            }

            graph.Append("}");

            return graph.ToString();
        }
    }
}
