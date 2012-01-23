using System;
using System.Collections.Generic;
using System.Linq;
using Piglet.Lexer.Construction;

namespace Piglet.Lexer.Configuration
{
    public class LexerConfigurator<T> : ILexerConfigurator<T>
    {
        protected List<Tuple<string, Func<string, T>>> Tokens { get; set; }

        public ILexer<T> CreateLexer()
        {
            // For each token, create a NFA
            NFA[] nfas = Tokens.Select(token => NFA.Create(PostFixConverter.ToPostFix(token.Item1))).ToArray();

            // Create a merged NFA
            NFA mergedNfa = NFA.Merge(nfas);

            // Convert the NFA to a DFA
            DFA dfa = DFA.Create(mergedNfa);

            // Convert the dfa to table form
            var transitionTable = new TransitionTable<T>(dfa, nfas, Tokens);

            return new Lexer.Lexer<T>(transitionTable);
        }

        public LexerConfigurator()
        {
            Tokens = new List<Tuple<string, Func<string, T>>>();
        }

        public void Token(string regEx, Func<string, T> action)
        {
            Tokens.Add(new Tuple<string, Func<string, T>>(regEx, action));
        }
    }
}