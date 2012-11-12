using System;
using System.IO;
using NUnit.Framework;
using Piglet.Lexer;
using Piglet.Lexer.Configuration;
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
            var li = lexer.Begin(new StringReader("abb"));
            Tuple<int, string> tuple = li.Next();
            Assert.AreEqual(1, tuple.Item1);
            Assert.AreEqual("ABB", tuple.Item2);
        }

        [Test]
        public void TestLexerConstructionUsingDfaEngine()
        {
            ILexer<string> lexer = LexerFactory<string>.Configure(c =>
            {
                c.Token("a+", f => "A+");
                c.Token("abb", f => "ABB");
                c.Token("a*b+", f => "A*B+");
                c.Runtime = LexerRuntime.Dfa;
            });
            var li = lexer.Begin(new StringReader("abb"));
            Tuple<int, string> tuple = li.Next();
            Assert.AreEqual(1, tuple.Item1);
            Assert.AreEqual("ABB", tuple.Item2);
        }

        [Test]
        public void TestLexerConstructionUsingNfaEngine()
        {
            ILexer<string> lexer = LexerFactory<string>.Configure(c =>
            {
                c.Token("a+", f => "A+");
                c.Token("abb", f => "ABB");
                c.Token("a*b+", f => "A*B+");
                c.Runtime = LexerRuntime.Nfa;
            });
            var li = lexer.Begin(new StringReader("abb"));
            Tuple<int, string> tuple = li.Next();
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
            var li = lexer.Begin("aa aaaaaaa aa aaaa aa");
            Assert.AreEqual("aa", li.Next().Item2);
            Assert.AreEqual("a+", li.Next().Item2);
            Assert.AreEqual("aa", li.Next().Item2);
            Assert.AreEqual("a+", li.Next().Item2);
            Assert.AreEqual("aa", li.Next().Item2);
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
            var li = lexer.Begin(new StringReader("    abb   bbbbbbbbb"));

            Tuple<int, string> tuple = li.Next();
            Assert.AreEqual("ABB", tuple.Item2);
            tuple = li.Next();
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
            var li = lexer.Begin("bbbbbbaaabbbaaaaabbbb");
            Tuple<int, string> lexVal = li.Next();
            Assert.AreEqual(0, lexVal.Item1);
            Assert.AreEqual("aaa", lexVal.Item2);
            lexVal = li.Next();
            Assert.AreEqual(0, lexVal.Item1);
            Assert.AreEqual("aaaaa", lexVal.Item2);
            lexVal = li.Next();
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
            var li = lexer.Begin(new StringReader("    123   42"));

            Tuple<int, int> tuple = li.Next();
            Assert.AreEqual(123, tuple.Item2);
            tuple = li.Next();
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
            var li = lexer.Begin("aaa         aa  \n   aaa    aa\n   error \n   aaaa");
            try
            {
                for (int i = 0; i < 10; ++i)
                {
                    li.Next();
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
		public void TestPerformanceWhenHandlingVeryLargeCharRanges()
		{
			foreach (var runtime in Enum.GetValues(typeof(LexerRuntime)))
			{
				Console.WriteLine(runtime.ToString());
				var ticks = System.DateTime.Now.Ticks;

				var lexer = LexerFactory<int>.Configure(configurator =>
				{
					configurator.Runtime = (LexerRuntime) runtime;

					configurator.Token("\\w[0-9]", null);
					configurator.Token("\\d\\D\\W", null);
					configurator.Token("abcdefghijklmnopqrstuvxyz", null);
					configurator.Token("01234567890&%#", null);
				});

				Console.WriteLine(System.DateTime.Now.Ticks - ticks);	
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
