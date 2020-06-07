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
            var tuple = li.Next();

            Assert.AreEqual(1, tuple.Item1);
            Assert.AreEqual("ABB", tuple.token.SymbolValue);
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
            var tuple = li.Next();

            Assert.AreEqual(1, tuple.Item1);
            Assert.AreEqual("ABB", tuple.token.SymbolValue);
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
            var tuple = li.Next();

            Assert.AreEqual(1, tuple.Item1);
            Assert.AreEqual("ABB", tuple.token.SymbolValue);
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

            Assert.AreEqual("aa", li.Next().token.SymbolValue);
            Assert.AreEqual("a+", li.Next().token.SymbolValue);
            Assert.AreEqual("aa", li.Next().token.SymbolValue);
            Assert.AreEqual("a+", li.Next().token.SymbolValue);
            Assert.AreEqual("aa", li.Next().token.SymbolValue);
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
            var tuple = li.Next();
            Assert.AreEqual("ABB", tuple.token.SymbolValue);
            tuple = li.Next();
            Assert.AreEqual("A*B+", tuple.token.SymbolValue);
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
            var lexVal = li.Next();

            Assert.AreEqual(0, lexVal.Item1);
            Assert.AreEqual("aaa", lexVal.token.SymbolValue);
            lexVal = li.Next();
            Assert.AreEqual(0, lexVal.Item1);
            Assert.AreEqual("aaaaa", lexVal.token.SymbolValue);
            lexVal = li.Next();
            Assert.AreEqual(-1, lexVal.Item1);
            Assert.AreEqual(null, lexVal.token.SymbolValue);
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
            var tuple = li.Next();

            Assert.AreEqual(123, tuple.token.SymbolValue);
            tuple = li.Next();
            Assert.AreEqual(42, tuple.token.SymbolValue);
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
        //	foreach (var runtime in Enum.GetValues(typeof(LexerRuntime)))
            {
            //	Console.WriteLine(runtime.ToString());
        //		var ticks = System.DateTime.Now.Ticks;

                var lexer = LexerFactory<int>.Configure(configurator =>
                {
                    configurator.Runtime = LexerRuntime.Tabular;
                    configurator.Token("\\w[0-9]", null);
                    configurator.Token("\\d\\D\\W", null);
                    configurator.Token("abcdefghijklmnopqrstuvxyz", null);
                    configurator.Token("01234567890&%#", null);
                });

        //		Console.WriteLine(System.DateTime.Now.Ticks - ticks);	
            }
        }

        [Test]
        public void TestLexLargeText()
        {
            const string text = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit, sed diam nonummy " +
                                "nibh euismod tincidunt ut laoreet dolore magna aliquam erat volutpat. Ut wisi enim ad " +
                                "minim veniam, quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut aliquip " +
                                "ex ea commodo consequat. Duis autem vel eum iriure dolor in hendrerit in vulputate velit " +
                                "esse molestie consequat, vel illum dolore eu feugiat nulla facilisis at vero eros et " +
                                "accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue " +
                                "duis dolore te feugait nulla facilisi. Nam liber tempor cum soluta nobis eleifend " +
                                "option congue nihil imperdiet doming id quod mazim placerat facer possim assum. " +
                                "Typi non habent claritatem insitam; est usus legentis in iis qui facit eorum " +
                                "claritatem. Investigationes demonstraverunt lectores legere me lius quod ii " +
                                "legunt saepius. Claritas est etiam processus dynamicus, qui sequitur mutationem " +
                                "consuetudium lectorum. Mirum est notare quam littera gothica, quam nunc putamus parum claram, " +
                                "anteposuerit litterarum formas humanitatis per seacula quarta decima et quinta decima. " +
                                "Eodem modo typi, qui nunc nobis videntur parum clari, fiant sollemnes in futurum.";

            int numWords = 0;
            int numPunctuation = 0;
            var lexer = LexerFactory<int>.Configure(c =>
                {
                    c.Token("\\w+", s => ++numWords);
                    c.Token("[.,]", s => ++numPunctuation);
                    c.Ignore("\\s+");
                });
            int numTokens = 0;
            foreach (var token in lexer.Tokenize(text))
            {
                numTokens++;
            }
            Assert.AreEqual(172, numWords);
            Assert.AreEqual(18, numPunctuation);
            Assert.AreEqual(190, numTokens);

            Console.WriteLine("asas");
        }

        [Test]
        public void TestCreateDFA()
        {
            NFA nfa = NfaBuilder.Create(new ShuntingYard(new RegexLexer(new StringReader("a|b*cd")), false));
            DFA dfa = DFA.Create(nfa);
        }

        [Test]
        public void TestCreateDFA2()
        {
            DFA dfa = DFA.Create(NfaBuilder.Create(new ShuntingYard(new RegexLexer(new StringReader("a|b|c")), false)));
        }

        [Test]
        public void TestOneOrMoreDFA()
        {
            NFA nfa = NfaBuilder.Create(new ShuntingYard(new RegexLexer(new StringReader("a+")), false));
            DFA dfa = DFA.Create(nfa);
        }
    }
}
