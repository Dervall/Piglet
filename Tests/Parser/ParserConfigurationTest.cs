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
            var parser = ParserFactory.Configure<int>( configurator =>
            {
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
                    p.Production(expr, "+", term).OnReduce(s => s[0] + s[2]);
                    p.Production(expr, "-", term).OnReduce(s => s[0] - s[2]);
                    p.Production(term).OnReduce(s => s[0]);
                });

                term.Productions(p =>
                {
                    p.Production(term, "*", factor).OnReduce(s => s[0] * s[2]);
                    p.Production(term, "/", factor).OnReduce(s => s[0] / s[2]);
                    p.Production(factor).OnReduce(s => s[0]);
                });

                factor.Productions(p =>
                {
                    p.Production(number).OnReduce(s => s[0]);
                    p.Production("(", expr, ")").OnReduce(s => s[1]);
                });
            });

            int result = parser.Parse(new StringReader("7+8*2-2+2"));

            Assert.AreEqual(23, result);
        }

        [TestMethod]
        public void TestShiftReduceError()
        {
            INonTerminal<string> ifStatement = null;

            try
            {
                // This configuration is not a valid LR1 parser. It contains shift reduce conflicts
                // clasical dangling else case
                ParserFactory.Configure<string>(configurator =>
                {
                    var ident = configurator.Terminal("[a-z]+");
                    ident.DebugName = "ident";

                    ifStatement = configurator.NonTerminal();
                    ifStatement.DebugName = "ifStatement";

                    var statement = configurator.NonTerminal();
                    statement.DebugName = "statement";

                    ifStatement.Productions(p =>
                    {
                        p.Production("if", "\\(", ident, "\\)", "then", statement);
                        p.Production("if", "\\(", ident, "\\)", "then", statement, "else", statement);
                    });

                    statement.Productions(p =>
                    {
                        p.Production(ifStatement);
                        p.Production(ident, "=", ident);
                    });

                    configurator.LexerSettings.CreateLexer = false;
                });
            
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
            ParserFactory.Configure<int>( configurator =>
            {
                INonTerminal<int> nonTerminal = configurator.NonTerminal();
                nonTerminal.DebugName = "NonTerm";
                nonTerminal.Productions(p =>
                                            {
                                                p.Production("this", "is", "a", "string");
                                                p.Production("this", "is", "a", "test");
                                            });
            });
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
                ParserFactory.Configure<object>( configurator =>
                {
                    INonTerminal<object> x = configurator.NonTerminal();
                    x.DebugName = "X";

                    t = configurator.NonTerminal();
                    t.DebugName = "T";
                
                    y = configurator.NonTerminal();
                    y.DebugName = "Y";

                    x.Productions(p =>
                    {
                        p.Production(t);
                        p.Production(y);
                    });
                    t.Productions(p => p.Production("A"));
                    y.Productions(p => p.Production("A"));
                });
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
                ParserFactory.Configure<byte>(configurator =>
                        {
                            var t = configurator.NonTerminal();
                            t.Productions(f => f.Production(t));
                        }
                    );
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
                ParserFactory.Configure<int>( configurator =>
                {
                    var a = configurator.NonTerminal();
                    a.DebugName = "a";
                    var b = configurator.NonTerminal();
                    b.DebugName = "b";
                    var c = configurator.NonTerminal();
                    c.DebugName = "c";
                    a.Productions(p =>
                    {
                        p.Production(a);
                        p.Production(b);
                        p.Production(c);
                    });
                    b.Productions(p => p.Production("b"));
                    c.Productions(p => p.Production("c"));                                      
                });

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
            ParserFactory.Configure<int>(configurator =>
                                             {
                                                 var a = configurator.NonTerminal();
                                                 var b = configurator.NonTerminal();
                                                 var c = configurator.NonTerminal();
                                                 var d = configurator.NonTerminal();
                                                 a.Productions(p => p.Production(a, b, c, d));
                                                 b.Productions(p => p.Production("b"));
                                                 c.Productions(p => p.Production(b));
                                                 d.Productions(p => p.Production("d"));
                                             });
        }

        [TestMethod]
        public void TestGrammarWithEpsilonTransitions()
        {
            var parser = ParserFactory.Configure<int>(configurator =>
            {
                var func = configurator.NonTerminal();
                func.DebugName = "FUNC";
                var paramList = configurator.NonTerminal();
                paramList.DebugName = "PARAMLIST";
                var optionalParamList = configurator.NonTerminal();
                optionalParamList.DebugName = "OPTIONALPARAMLIST";

                func.Productions(p => p.Production("func", "(", optionalParamList, ")"));
                paramList.Productions(p =>
                {
                    p.Production(paramList, ",", "ident");
                    p.Production("ident");
                });

                optionalParamList.Productions(p =>
                {
                    p.Production(paramList);
                    p.Production();
                });
            });
            parser.Parse("func(ident,ident,ident,ident)");
            parser.Parse("func()");
        }

        [TestMethod]
        public void TestDeepEpsilonChain()
        {
            var parser = ParserFactory.Configure<int>(configurator =>
            {
                var a = configurator.NonTerminal();
                var b = configurator.NonTerminal();
                var c = configurator.NonTerminal();
                var d = configurator.NonTerminal();
                var e = configurator.NonTerminal();

                a.Productions(p => p.Production("a", b));
                b.Productions(p =>
                {
                    p.Production(c);
                    p.Production();

                });
                c.Productions(p => p.Production("d", d));
                d.Productions(p =>
                {
                    p.Production(e);
                    p.Production();
                });
                e.Productions(p => p.Production("e"));
            });

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
            ParserFactory.Configure<int>(configurator =>
            {
                var s = configurator.NonTerminal();
                s.DebugName = "S";
                var l = configurator.NonTerminal();
                l.DebugName = "L";
                var r = configurator.NonTerminal();
                r.DebugName = "R";

                s.Productions(p =>
                {
                    p.Production(l, "=", r);
                    p.Production(r);
                });

                l.Productions(p =>
                {
                    p.Production("*", r);
                    p.Production("id");
                });

                r.Productions(p => p.Production(l));
            });
        }

        [TestMethod]
        public void TestMultipleEpsilonParametersInARow()
        {
            var parser = ParserFactory.Configure<int>(configurator =>
            {
                var a = configurator.NonTerminal();
                a.DebugName = "A";
                var b = configurator.NonTerminal();
                b.DebugName = "B";
                var c = configurator.NonTerminal();
                c.DebugName = "C";
                var d = configurator.NonTerminal();
                d.DebugName = "D";

                a.Productions(p => p.Production("a", b, c, d, "a"));
                b.Productions(p =>
                {
                    p.Production("b");
                    p.Production();
                });

                c.Productions(p =>
                {
                    p.Production("c");
                    p.Production();
                });
                d.Productions(p =>
                {
                    p.Production("d");
                    p.Production();
                });                                     
            });
            
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
            var parser = ParserFactory.Configure<int>(configurator =>
                                             {
                                                 var s = configurator.NonTerminal();
                                                 s.DebugName = "S";
                                                 var c = configurator.NonTerminal();
                                                 c.DebugName = "C";

                                                 s.Productions(p => p.Production(c, c));
                                                 c.Productions(p => p.Production("c", c));
                                                 c.Productions(p => p.Production("d"));
                                             });
            parser.Parse("ccccccccdd");

        }

        [TestMethod]
        public void TestSingleRuleTerminalGrammar()
        {
            var parser = ParserFactory.Configure<int>(configurator =>
                                                          {
                                                              var s = configurator.NonTerminal();
                                                              s.DebugName = "S";
                                                              s.Productions(p => p.Production("a", "b", "c", "d"));
                                                          });
            parser.Parse("abcd");
        }
    }
}
