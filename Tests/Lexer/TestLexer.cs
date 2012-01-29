using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Lexer;
using Piglet.Lexer.Construction;

namespace Piglet.Tests.Lexer
{
    [TestClass]
    public class TestLexer
    {
        [TestMethod]
        public void TestLexerConstruction()
        {
            ILexer<string> lexer = LexerFactory<string>.Configure(c =>
                                               {
                                                   c.Token("a+", f => "A+");
                                                   c.Token("abb", f => "ABB");
                                                   c.Token("a*b+", f => "A*B+");
                                               });
            lexer.SetSource(new StringReader("abb"));
            Tuple<int, string> tuple = lexer.Next();
            Assert.AreEqual(1, tuple.Item1);
            Assert.AreEqual("ABB", tuple.Item2);
        }

        [TestMethod]
        public void TestLexerConstructionWithWhitespaceIgnore()
        {
            ILexer<string> lexer = LexerFactory<string>.Configure(c =>
            {
                c.Token("a+", f => "A+");
                c.Token("abb", f => "ABB");
                c.Token("a*b+", f => "A*B+");
                c.Ignore(" *");
            });
            lexer.SetSource(new StringReader("    abb   bbbbbbbbb"));

            Tuple<int, string> tuple = lexer.Next();
            Assert.AreEqual("ABB", tuple.Item2);
            tuple = lexer.Next();
            Assert.AreEqual("A*B+", tuple.Item2);
        }

        [TestMethod]
        public void TestGetsEndOfInputTokenIfIgnoredStuffAtEnd()
        {
            ILexer<string> lexer = LexerFactory<string>.Configure(c =>
                                                                      {
                                                                          c.Token("a+", f => f);
                                                                          c.Ignore("b+");
                                                                          c.EndOfInputTokenNumber = -1;
                                                                      });
            lexer.SetSource("bbbbbbaaabbbaaaaabbbb");
            Tuple<int, string> lexVal = lexer.Next();
            Assert.AreEqual(0, lexVal.Item1);
            Assert.AreEqual("aaa", lexVal.Item2);
            lexVal = lexer.Next();
            Assert.AreEqual(0, lexVal.Item1);
            Assert.AreEqual("aaaaa", lexVal.Item2);
            lexVal = lexer.Next();
            Assert.AreEqual(-1, lexVal.Item1);
            Assert.AreEqual(null, lexVal.Item2);
        }

        [TestMethod]
        public void TestLexDigits()
        {
            ILexer<int> lexer = LexerFactory<int>.Configure(c =>
            {
                c.Token("\\d+", int.Parse);
                c.Ignore(" *");
            });
            lexer.SetSource(new StringReader("    123   42"));

            Tuple<int, int> tuple = lexer.Next();
            Assert.AreEqual(123, tuple.Item2);
            tuple = lexer.Next();
            Assert.AreEqual(42, tuple.Item2);
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

        

        [TestMethod]
        public void TestOneOrMoreDFA()
        {
            NFA nfa = NFA.Create("a+");
            DFA dfa = DFA.Create(nfa);
            Console.WriteLine("tjim");
        }
    }
}
