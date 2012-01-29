using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Lexer;
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
            var configurator = ParserConfiguratorFactory.CreateConfigurator<int>();

            ITerminal<int> number = configurator.Terminal("\\d+", int.Parse);
            number.DebugName = "number";

            INonTerminal<int> term = configurator.NonTerminal();
            term.DebugName = "term";
            INonTerminal<int> expr = configurator.NonTerminal();
            expr.DebugName = "expr";
            INonTerminal<int> factor = configurator.NonTerminal();
            factor.DebugName = "factor";
            
            expr.Productions(p => {
                                      p.Production(expr, "\\+", term).OnReduce(s => s[0] + s[2]);
                                      p.Production(term).OnReduce(s => s[0]);
                                  });

            term.Productions(p =>
                                 {
                                     p.Production(term, "\\*", factor).OnReduce(s => s[0] * s[2]);
                                     p.Production(factor).OnReduce(s => s[0]);
                                 });

            factor.Productions(p =>
                                {
                                    p.Production(number).OnReduce(s => s[0]);
                                    p.Production("\\(", expr, "\\)").OnReduce(s => s[1]);
                                });

            configurator.SetStartSymbol(expr);
            configurator.AugmentGrammar();

            ILexer<int> lexer = configurator.CreateLexer();
            IParser<int> parser = configurator.CreateParser();
            lexer.SetSource(new StringReader("7+8*2"));
            int result = parser.Parse(lexer);

            Assert.AreEqual(23, result);
        }

        [TestMethod]
        public void TestShiftReduceError()
        {
            // This configuration is not a valid LR1 parser. It contains shift reduce conflicts
            // clasical dangling else case
            IParserConfigurator<string> configurator = ParserConfiguratorFactory.CreateConfigurator<string>();

            var ident = configurator.Terminal("[a-z]+");
            ident.DebugName = "ident";

            var statement = configurator.NonTerminal();
            statement.DebugName = "statement";

            var ifStatement = configurator.NonTerminal();
            ifStatement.DebugName = "ifStatement";

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
            configurator.SetStartSymbol(ifStatement);

            try
            {
                IParser<string> parser = configurator.CreateParser();
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
            var configurator = ParserConfiguratorFactory.CreateConfigurator<int>();
            INonTerminal<int> nonTerminal = configurator.NonTerminal();
            nonTerminal.DebugName = "NonTerm";
            nonTerminal.Productions(p =>
                                        {
                                            p.Production("this", "is", "a", "string");
                                            p.Production("this", "is", "a", "test");
                                        });
            configurator.SetStartSymbol(nonTerminal);

            configurator.CreateParser();
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
            var configurator = ParserConfiguratorFactory.CreateConfigurator<object>();
            var t = configurator.NonTerminal();
            t.DebugName = "T";
            t.Productions(p => p.Production("A"));
            var y = configurator.NonTerminal();
            y.DebugName = "Y";
            y.Productions(p => p.Production("A"));
            var x = configurator.NonTerminal();
            x.DebugName = "X";
            x.Productions(p =>
                              {
                                  p.Production(t);
                                  p.Production(y);
                              });

            configurator.SetStartSymbol(x);

            try
            {
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
            var configurator = ParserConfiguratorFactory.CreateConfigurator<byte>();
            var t = configurator.NonTerminal();
            t.Productions(f => f.Production(t));
            configurator.SetStartSymbol(t);

            try
            {
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
            var configurator = ParserConfiguratorFactory.CreateConfigurator<int>();
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

            configurator.SetStartSymbol(a);

            try
            {
                var parser = (LRParser<int>)configurator.CreateParser();
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
            var configurator = ParserConfiguratorFactory.CreateConfigurator<int>();
            var a = configurator.NonTerminal();
            var b = configurator.NonTerminal();
            var c = configurator.NonTerminal();
            var d = configurator.NonTerminal();
            a.Productions(p => p.Production(a, b, c, d));
            b.Productions(p => p.Production("b"));
            c.Productions(p => p.Production(b));
            d.Productions(p => p.Production("d"));
            configurator.SetStartSymbol(a);
            configurator.CreateParser();
            // Just make sure this doesn't crash for now
        }
    }
}
