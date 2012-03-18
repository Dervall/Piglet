using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Piglet.Lexer.Construction;

namespace Piglet.Lexer.Configuration
{
    internal class LexerConfigurator<T> : ILexerConfigurator<T>
    {
        private readonly List<Tuple<string, Func<string, T>>> tokens;
        private readonly List<string> ignore;

        public ILexer<T> CreateLexer()
        {
            // For each token, create a NFA
            IList<NFA> nfas = tokens.Select(token => NfaBuilder.Create(new ShuntingYard(new RegExLexer( new StringReader(token.Item1))))).ToList();
            foreach (var ignoreExpr in ignore)
            {
                nfas.Add(NfaBuilder.Create(new ShuntingYard(new RegExLexer(new StringReader(ignoreExpr)))));
            }

            // Create a merged NFA
            NFA mergedNfa = NFA.Merge(nfas);

            // Convert the NFA to a DFA
            DFA dfa = DFA.Create(mergedNfa);

            // Minimize the DFA if required
            dfa.Minimize();

            // Convert the dfa to table form
            var transitionTable = new TransitionTable<T>(dfa, nfas, tokens);

            return new Lexer<T>(transitionTable, EndOfInputTokenNumber);
        }

        public LexerConfigurator()
        {
            tokens = new List<Tuple<string, Func<string, T>>>();
            ignore = new List<string>();
            EndOfInputTokenNumber = -1;
            MinimizeDfa = true;
        }

        public void Token(string regEx, Func<string, T> action)
        {
            tokens.Add(new Tuple<string, Func<string, T>>(regEx, action));
        }

        public void Ignore(string regEx)
        {
            ignore.Add(regEx);
        }

        public int EndOfInputTokenNumber { get; set; }
        public bool MinimizeDfa { get; set; }
    }
}