using System.Collections.Generic;
using System.Text;

namespace Piglet.Parser.Construction.Debug
{
    internal static class DotNotation
    {
        internal static string AsDotNotation<T>(this IEnumerable<ParserBuilder<T>.GotoSetTransition> transitions, List<Lr1ItemSet<T>> itemSets)
        {
            StringBuilder graph = new StringBuilder();
            graph.Append("digraph goto {");

            foreach (ParserBuilder<T>.GotoSetTransition transition in transitions)
                graph.Append($"\t\"I{itemSets.IndexOf(transition.From)}\" -> \"I{itemSets.IndexOf(transition.To)}\" [label=\"{(transition.OnSymbol.DebugName ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"")}\"]\n");

            graph.Append("}");

            return graph.ToString();
        }
    }
}
