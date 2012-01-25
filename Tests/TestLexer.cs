using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Lexer;
using Piglet.Lexer.Construction;

namespace TestParser
{
    [TestClass]
    public class TestLexer
    {


        [TestMethod]
        public void TestComplex()
        {
            NFA nfa = NFA.Create("ab|*a&b&a*&*ab|&ab|&");
            ;
        }

        [TestMethod]
        public void TestNFA()
        {
            //  (a|b)*a -> ab|*a&a&
            NFA nfa = NFA.Create("ab|*c&d&");
            ;
        }

        [TestMethod]
        public void TestRepeat()
        {
            NFA nfa = NFA.Create("a*");
            ;
        }

        [TestMethod]
        public void TestOneOrMore()
        {
            NFA nfa = NFA.Create("a+");
            Console.WriteLine("tjo");
        }

        [TestMethod]
        public void TestOneOrMoreDFA()
        {
            NFA nfa = NFA.Create("a+");
            DFA dfa = DFA.Create(nfa);
            Console.WriteLine("tjim");
        }

        [TestMethod]
        public void TestConnectThreeNFA()
        {
            // Dragon book example
            NFA nfa = NFA.Create(PostFixConverter.ToPostFix("a+"));
            NFA nfa2 = NFA.Create(PostFixConverter.ToPostFix("abb"));
            NFA nfa3 = NFA.Create(PostFixConverter.ToPostFix("a*b+"));

            NFA final = NFA.Merge(new List<NFA> {nfa, nfa2, nfa3});
            DFA finalDFA = DFA.Create(final);
            Console.WriteLine(123);
        }

        [TestMethod]
        public void TestLexerConstruction()
        {
            ILexer<string> lexer = LexerFactory<string>.Configure(c =>
                                               {
                                                   c.Token("a+", f => "A+");
                                                   c.Token("abb", f => "ABB");
                                                   c.Token("a*b+", f => "A*B+");
                                               });
            lexer.Source = new StringReader("abb");
            Tuple<int, string> tuple = lexer.Next();
            Console.WriteLine("stsd");
        }

        [TestMethod]
        public void TestLexerConstructionWithWhitespaceIgnore()
        {
            ILexer<string> lexer = LexerFactory<string>.Configure(c =>
            {
                c.Token("a+", f => "A+");
                c.Token("abb", f => "ABB");
                c.Token("a*b+", f => "A*B+");
                c.Ignore(" *"); // Bad regexp lol, fix
            });
            lexer.Source = new StringReader("    abb   bbbbbbbbb");

            Tuple<int, string> tuple = lexer.Next();
            Assert.AreEqual("ABB", tuple.Item2);
            tuple = lexer.Next();
            Assert.AreEqual("A*B+", tuple.Item2);
        }

        [TestMethod]
        public void TestAlternate()
        {
            NFA nfa = NFA.Create("ab|");
            ;
        }

        [TestMethod]
        public void TestConcatenate()
        {
            NFA nfa = NFA.Create("ab&c&");
            ;
        }

        [TestMethod]
        public void TestClosure()
        {
            NFA nfa = NFA.Create("ab|*c&d&");
            IList<NFA.State> s0Closure = nfa.Closure(new[] {nfa.StartState}).ToList();

            foreach (var state in s0Closure)
            {
                Console.WriteLine(state);
            }
        }

        [TestMethod]
        public void TestCreateDFA()
        {
            NFA nfa = NFA.Create("ab|*c&d&");

            DFA dfa = DFA.Create(nfa);
            Console.WriteLine("");
        }

        [TestMethod]
        public void TestCreateDFA2()
        {
            DFA dfa = DFA.Create(NFA.Create("ab|c|"));
            Console.WriteLine();
        }
    }
}
