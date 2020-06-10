using System.Linq;
using System.Text;

using Piglet.Parser.Configuration;

namespace Piglet.Parser.Construction.Debug
{
    internal static class ParseTableToString
    {
        internal static string ToDebugString<T>(this IParseTable<T> table, IGrammar<T> grammar, int numStates)
        {
            int numTokens = grammar.AllSymbols.Count() - 1;
            int numTerminals = grammar.AllSymbols.OfType<Terminal<T>>().Count();

            StringBuilder formatString = new StringBuilder("{0,8}|");

            for (int i = 0; i < numTokens; ++i)
            {
                if (i == numTerminals)
                    formatString.Append("|"); // Extra bar to separate actions and gotos

                formatString.Append("|{" + (i + 1) + ",8}");
            }

            formatString.Append("|\n");

            string format = formatString.ToString();
            StringBuilder sb = new StringBuilder();

            sb.Append(string.Format(format, new[] { "STATE" }.Concat(grammar.AllSymbols.Select(f => f.DebugName ?? "")).ToArray<object>()));

            for (int i = 0; i < numStates; ++i)
            {
                object[] formatParams = new[] { i.ToString() }.Concat(grammar.AllSymbols.OfType<Terminal<T>>().Select(f =>
                {
                    int actionValue = table.Action?[i, f.TokenNumber] ?? short.MinValue;

                    if (actionValue == short.MaxValue)
                        return "acc";
                    else if (actionValue == short.MinValue)
                        return "";
                    else if (actionValue < 0)
                        return "r" + -(actionValue + 1);
                    else
                        return "s" + actionValue;
                }).Concat(grammar.AllSymbols
                                 .OfType<NonTerminal<T>>()
                                 .Where(f => f.ProductionRules.All(p => p.ResultSymbol != grammar.AcceptSymbol))
                                 .Select(f => table.Goto[i, f.TokenNumber - numTerminals] == short.MinValue
                                                                                          ? ""
                                                                                          : table.Goto[i, f.TokenNumber - numTerminals].ToString()))).ToArray<object>();

                // If formatparams is all empty, we have run out of table to process.
                // This is perhaps not the best way to determine if the table has ended but the grammar has no idea of the
                // number of states, and I'd rather not mess up the interface with methods to get the number of states.
                if (formatParams.Distinct().Count() == 2)
                    break; // All empty strings and one state.

                sb.Append(string.Format(format, formatParams));
            }

            return sb.ToString();
        }
    }
}
