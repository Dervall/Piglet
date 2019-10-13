using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Piglet.Lexer.Construction;
using Piglet.Lexer.Runtime;

namespace Piglet.Lexer.Configuration
{
    internal class LexerConfigurator<T> : ILexerConfigurator<T>
    {
        private readonly List<Tuple<string, Func<string, T>>> tokens;
        private readonly List<string> ignore;

        public LexerConfigurator()
        {
            tokens = new List<Tuple<string, Func<string, T>>>();
            ignore = new List<string>();
            EndOfInputTokenNumber = -1;
            MinimizeDfa = true;
            Runtime = LexerRuntime.Tabular;
        }

        public ILexer<T> CreateLexer()
        {
            // For each token, create a NFA
            IList<NFA> nfas = tokens.Select(token =>
            {
                try
                {
                    return NfaBuilder.Create(new ShuntingYard(new RegexLexer(new StringReader(token.Item1))));
                }
                catch (Exception ex)
                {
                    throw new LexerConstructionException($"Malformed regex '{token.Item1}'.", ex);
                }
            }).ToList();
            foreach (string ignoreExpr in ignore)
            {
                nfas.Add(NfaBuilder.Create(new ShuntingYard(new RegexLexer(new StringReader(ignoreExpr)))));
            }

            // Create a merged NFA
            NFA mergedNfa = NFA.Merge(nfas);

            // If we desire a NFA based lexer, stop now
            if (Runtime == LexerRuntime.Nfa)
            {
                return new NfaLexer<T>(mergedNfa, nfas, tokens, EndOfInputTokenNumber);
            }

            // Convert the NFA to a DFA
            DFA dfa = DFA.Create(mergedNfa);

            // Minimize the DFA if required
            dfa.Minimize();

            // If we desire a DFA based lexer, stop
            if (Runtime == LexerRuntime.Dfa)
            {
                // TODO:
                // The input ranges which will have been previously split into the smallest distinct
                // units will need to be recombined in order for this to work as fast as possible.
                //dfa.CombineInputRanges();
                return new DfaLexer<T>(dfa, nfas, tokens, EndOfInputTokenNumber);
            }

            // Convert the dfa to table form
            TransitionTable<T> transitionTable = new TransitionTable<T>(dfa, nfas, tokens);

            return new TabularLexer<T>(transitionTable, EndOfInputTokenNumber);
        }

        public void Token(string regEx, Func<string, T> action) => tokens.Add(new Tuple<string, Func<string, T>>(regEx, action));

        public void Ignore(string regEx) => ignore.Add(regEx);

        public int EndOfInputTokenNumber { get; set; }
        public bool MinimizeDfa { get; set; }
        public LexerRuntime Runtime { get; set; }
    }
}