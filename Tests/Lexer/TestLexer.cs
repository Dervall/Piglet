using System;
using System.IO;
using NUnit.Framework;
using Piglet.Lexer;
using Piglet.Lexer.Construction;

namespace Piglet.Tests.Lexer
{
    [TestFixture]
    public class TestLexer
    {
        [Test]
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

        [Test]
        public void TestMinimizationWontMessUpLexing()
        {
            var lexer = LexerFactory<string>.Configure(c =>
                                                           {
                                                               c.MinimizeDfa = true;
                                                               c.Token("aa", f=> "aa");
                                                               c.Token("a+", f => "a+");
                                                               c.Ignore(" ");
                                                           });
            lexer.SetSource("aa aaaaaaa aa aaaa aa");
            Assert.AreEqual("aa", lexer.Next().Item2);
            Assert.AreEqual("a+", lexer.Next().Item2);
            Assert.AreEqual("aa", lexer.Next().Item2);
            Assert.AreEqual("a+", lexer.Next().Item2);
            Assert.AreEqual("aa", lexer.Next().Item2);
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void TestLexErrorOnThirdLine()
        {
            ILexer<string> lexer = LexerFactory<string>.Configure(c =>
                {
                    c.Token("a+", s => s);
                    c.Ignore("( |\\n)+");
                });
            lexer.SetSource("aaa         aa  \n   aaa    aa\n   error \n   aaaa");
            try
            {
                for (int i = 0; i < 10; ++i)
                {
                    lexer.Next();
                }
                Assert.Fail();
            }
            catch (LexerException e)
            {
                Assert.AreEqual(3, e.LineNumber);
                Assert.AreEqual("   ", e.LineContents);
            }
        }

        [Test]
        public void TestCreateDFA()
        {
            NFA nfa = NfaBuilder.Create(new ShuntingYard(new RegExLexer(new StringReader("a|b*cd"))));
            DFA dfa = DFA.Create(nfa);
        }

        [Test]
        public void TestCreateDFA2()
        {
            DFA dfa = DFA.Create(NfaBuilder.Create(new ShuntingYard(new RegExLexer(new StringReader("a|b|c")))));
        }

        [Test]
        public void TestOneOrMoreDFA()
        {
            NFA nfa = NfaBuilder.Create(new ShuntingYard(new RegExLexer(new StringReader("a+"))));
            DFA dfa = DFA.Create(nfa);
        }
    }
}
