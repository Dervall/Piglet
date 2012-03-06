using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Parser;
using Piglet.Parser.Configuration;
using Piglet.Parser.Construction;

namespace Piglet.Tests.Parser
{
    [TestClass]
    public class ParserIntegrationTest
    {
        [TestMethod]
        public void TestACalculator()
        {
            // This is a full on integration test that builds a parser and performs a simple calculation.
            var configurator = ParserFactory.Configure<int>();

            ITerminal<int> number = configurator.Terminal("\\d+", int.Parse);
            number.DebugName = "number";

            INonTerminal<int> expr = configurator.NonTerminal();
            expr.DebugName = "expr";
            INonTerminal<int> term = configurator.NonTerminal();
            term.DebugName = "term";
            INonTerminal<int> factor = configurator.NonTerminal();
            factor.DebugName = "factor";

            expr.Productions(p =>
            {
                p.AddProduction(expr, "+", term).SetReduceFunction(s => s[0] + s[2]);
                p.AddProduction(expr, "-", term).SetReduceFunction(s => s[0] - s[2]);
                p.AddProduction(term).SetReduceFunction(s => s[0]);
            });

            term.Productions(p =>
            {
                p.AddProduction(term, "*", factor).SetReduceFunction(s => s[0] * s[2]);
                p.AddProduction(term, "/", factor).SetReduceFunction(s => s[0] / s[2]);
                p.AddProduction(factor).SetReduceFunction(s => s[0]);
            });

            factor.Productions(p =>
            {
                p.AddProduction(number).SetReduceFunction(s => s[0]);
                p.AddProduction("(", expr, ")").SetReduceFunction(s => s[1]);
            });

            var parser = configurator.CreateParser();
            int result = parser.Parse(new StringReader("2-2-5"));

            Assert.AreEqual(-5, result);
        }

        [TestMethod]
        public void TestShiftReduceError()
        {
            INonTerminal<string> ifStatement = null;

            try
            {
                // This configuration is not a valid LR1 parser. It contains shift reduce conflicts
                // clasical dangling else case
                var configurator = ParserFactory.Configure<string>();
                var ident = configurator.Terminal("[a-z]+");
                ident.DebugName = "ident";

                ifStatement = configurator.NonTerminal();
                ifStatement.DebugName = "ifStatement";

                var statement = configurator.NonTerminal();
                statement.DebugName = "statement";

                ifStatement.Productions(p =>
                {
                    p.AddProduction("if", "\\(", ident, "\\)", "then", statement);
                    p.AddProduction("if", "\\(", ident, "\\)", "then", statement, "else", statement);
                });

                statement.Productions(p =>
                {
                    p.AddProduction(ifStatement);
                    p.AddProduction(ident, "=", ident);
                });

                configurator.LexerSettings.CreateLexer = false;
                configurator.CreateParser();

                Assert.Fail("No exception for ambiguous grammar");
            }
            catch (ShiftReduceConflictException<string> e)
            {
                Assert.AreEqual(ifStatement, e.ReduceSymbol);
                Assert.AreEqual("else", e.ShiftSymbol.DebugName);
            }
        }

        [TestMethod]
        public void TestCanMultiplyDefineTerminalStringsInConfiguration()
        {
            var configurator = ParserFactory.Configure<int>();
            INonTerminal<int> nonTerminal = configurator.NonTerminal();
            nonTerminal.DebugName = "NonTerm";
            nonTerminal.Productions(p =>
                                        {
                                            p.AddProduction("this", "is", "a", "string");
                                            p.AddProduction("this", "is", "a", "test");
                                        });
            var parser = configurator.CreateParser();
            Assert.IsNotNull(parser);
        }

        [TestMethod]
        public void TestReduceReduceConflict()
        {
            // Represents the grammar
            //
            // x := t | y
            // t := "A"
            // y := "A"
            //
            INonTerminal<object> y = null, t = null;
            try
            {
                var configurator = ParserFactory.Configure<object>();
                INonTerminal<object> x = configurator.NonTerminal();
                x.DebugName = "X";

                t = configurator.NonTerminal();
                t.DebugName = "T";

                y = configurator.NonTerminal();
                y.DebugName = "Y";

                x.Productions(p =>
                {
                    p.AddProduction(t);
                    p.AddProduction(y);
                });
                t.Productions(p => p.AddProduction("A"));
                y.Productions(p => p.AddProduction("A"));
                configurator.CreateParser();
                Assert.Fail();
            }
            catch (ReduceReduceConflictException<object> e)
            {
                Assert.AreEqual(y, e.NewReduceSymbol);
                Assert.AreEqual(t, e.PreviousReduceSymbol);
            }
        }

        [TestMethod]
        public void TestRetardedCyclicGrammar()
        {
            try
            {
                var configurator = ParserFactory.Configure<byte>();
                var t = configurator.NonTerminal();
                t.Productions(f => f.AddProduction(t));
                configurator.CreateParser();
                Assert.Fail();
            }
            catch (ShiftReduceConflictException<byte>)
            {
                // There is no real need to check this exception. It says something
                // about wanting to shift the augmented start symbol. We are cool that this
                // sillyness doesn't run on forever.
            }
        }

        [TestMethod]
        public void TestShiftReduceConflictWithAccept()
        {
            // For certain grammars you can produce this stuff. It is not a real world case
            // but the exception should still be helpful

            // Here's how this retarded grammar looks
            // a := a | b | c
            // b := "b" 
            // c := "c"
            //
            try
            {
                var configurator = ParserFactory.Configure<int>();
                var a = configurator.NonTerminal();
                a.DebugName = "a";
                var b = configurator.NonTerminal();
                b.DebugName = "b";
                var c = configurator.NonTerminal();
                c.DebugName = "c";
                a.Productions(p =>
                {
                    p.AddProduction(a);
                    p.AddProduction(b);
                    p.AddProduction(c);
                });
                b.Productions(p => p.AddProduction("b"));
                c.Productions(p => p.AddProduction("c"));
                configurator.CreateParser();

                Assert.Fail();
            }
            catch (ShiftReduceConflictException<int>)
            {
                // There is no real need to check this exception. It says something
                // about wanting to reduce on the $. We are cool that this
                // sillyness doesn't run on forever.
            }
        }

        [TestMethod]
        public void TestNonTerminalFollow()
        {
            // This grammar will require the parser factory to perform an aggregated FOLLOW
            // which it doesn't do if there aren't two nonteminals in a row
            var configurator = ParserFactory.Configure<int>();
            var a = configurator.NonTerminal();
            var b = configurator.NonTerminal();
            var c = configurator.NonTerminal();
            var d = configurator.NonTerminal();

            a.Productions(p => p.AddProduction(a, b, c, d));
            b.Productions(p => p.AddProduction("b"));
            c.Productions(p => p.AddProduction(b));
            d.Productions(p => p.AddProduction("d"));

            configurator.CreateParser();
        }

        [TestMethod]
        public void TestGrammarWithEpsilonTransitions()
        {
            var configurator = ParserFactory.Configure<int>();
            var func = configurator.NonTerminal();
            func.DebugName = "FUNC";
            var paramList = configurator.NonTerminal();
            paramList.DebugName = "PARAMLIST";
            var optionalParamList = configurator.NonTerminal();
            optionalParamList.DebugName = "OPTIONALPARAMLIST";

            func.Productions(p => p.AddProduction("func", "(", optionalParamList, ")"));
            paramList.Productions(p =>
            {
                p.AddProduction(paramList, ",", "ident");
                p.AddProduction("ident");
            });

            optionalParamList.Productions(p =>
            {
                p.AddProduction(paramList);
                p.AddProduction();
            });
            var parser = configurator.CreateParser();
            parser.Parse("func(ident,ident,ident,ident)");
            parser.Parse("func()");
        }

        [TestMethod]
        public void TestDeepEpsilonChain()
        {
            var configurator = ParserFactory.Configure<int>();
            var a = configurator.NonTerminal();
            var b = configurator.NonTerminal();
            var c = configurator.NonTerminal();
            var d = configurator.NonTerminal();
            var e = configurator.NonTerminal();

            a.Productions(p => p.AddProduction("a", b));
            b.Productions(p =>
            {
                p.AddProduction(c);
                p.AddProduction();

            });
            c.Productions(p => p.AddProduction("d", d));
            d.Productions(p =>
            {
                p.AddProduction(e);
                p.AddProduction();
            });
            e.Productions(p => p.AddProduction("e"));

            var parser = configurator.CreateParser();
            parser.Parse("ade");
            parser.Parse("a");
            parser.Parse("ad");
        }

        [TestMethod]
        public void TestNonSLRGrammar()
        {
            // 1. S’ ::= S     4. L ::= * R
            // 2. S ::= L = R  5. L ::= id
            // 3. S ::= R      6. R ::= L
            var configurator = ParserFactory.Configure<int>();
            var s = configurator.NonTerminal();
            s.DebugName = "S";
            var l = configurator.NonTerminal();
            l.DebugName = "L";
            var r = configurator.NonTerminal();
            r.DebugName = "R";

            s.Productions(p =>
            {
                p.AddProduction(l, "=", r);
                p.AddProduction(r);
            });

            l.Productions(p =>
            {
                p.AddProduction("*", r);
                p.AddProduction("id");
            });

            r.Productions(p => p.AddProduction(l));
            configurator.CreateParser();
        }

        [TestMethod]
        public void TestMultipleEpsilonParametersInARow()
        {
            var configurator = ParserFactory.Configure<int>();
            var a = configurator.NonTerminal();
            a.DebugName = "A";
            var b = configurator.NonTerminal();
            b.DebugName = "B";
            var c = configurator.NonTerminal();
            c.DebugName = "C";
            var d = configurator.NonTerminal();
            d.DebugName = "D";

            a.Productions(p => p.AddProduction("a", b, c, d, "a"));
            b.Productions(p =>
            {
                p.AddProduction("b");
                p.AddProduction();
            });

            c.Productions(p =>
            {
                p.AddProduction("c");
                p.AddProduction();
            });
            d.Productions(p =>
            {
                p.AddProduction("d");
                p.AddProduction();
            });

            var parser = configurator.CreateParser();
            parser.Parse("aa");
            parser.Parse("aba");
            parser.Parse("abca");
            parser.Parse("abcda");
            parser.Parse("aca");
            parser.Parse("ada");
            parser.Parse("abda");
        }

        [TestMethod]
        public void TestLr1Harness()
        {
            // Duplicate grammar from dragon book, for comparison
            //
            // S -> CC
            // C -> cC | d
            var configurator = ParserFactory.Configure<int>();
            var s = configurator.NonTerminal();
            s.DebugName = "S";
            var c = configurator.NonTerminal();
            c.DebugName = "C";

            s.Productions(p => p.AddProduction(c, c));
            c.Productions(p => p.AddProduction("c", c));
            c.Productions(p => p.AddProduction("d"));

            var parser = configurator.CreateParser();
            parser.Parse("ccccccccdd");

        }

        [TestMethod]
        public void TestSingleRuleTerminalGrammar()
        {
            var configurator = ParserFactory.Configure<int>();
            var s = configurator.NonTerminal();
            s.DebugName = "S";
            s.Productions(p => p.AddProduction("a", "b", "c", "d"));
            var parser = configurator.CreateParser();
            parser.Parse("abcd");
        }
    }
}
