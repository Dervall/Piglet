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

            ITerminal<int> number = configurator.CreateTerminal("\\d+", int.Parse);
            number.DebugName = "number";

            INonTerminal<int> expr = configurator.CreateNonTerminal();
            expr.DebugName = "expr";
            INonTerminal<int> term = configurator.CreateNonTerminal();
            term.DebugName = "term";
            INonTerminal<int> factor = configurator.CreateNonTerminal();
            factor.DebugName = "factor";

            expr.AddProduction(expr, "+", term).SetReduceFunction(s => s[0] + s[2]);
            expr.AddProduction(expr, "-", term).SetReduceFunction(s => s[0] - s[2]);
            expr.AddProduction(term).SetReduceFunction(s => s[0]);

            term.AddProduction(term, "*", factor).SetReduceFunction(s => s[0] * s[2]);
            term.AddProduction(term, "/", factor).SetReduceFunction(s => s[0] / s[2]);
            term.AddProduction(factor).SetReduceFunction(s => s[0]);

            factor.AddProduction(number).SetReduceFunction(s => s[0]);
            factor.AddProduction("(", expr, ")").SetReduceFunction(s => s[1]);

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
                var ident = configurator.CreateTerminal("[a-z]+");
                ident.DebugName = "ident";

                ifStatement = configurator.CreateNonTerminal();
                ifStatement.DebugName = "ifStatement";

                var statement = configurator.CreateNonTerminal();
                statement.DebugName = "statement";

                ifStatement.AddProduction("if", "\\(", ident, "\\)", "then", statement);
                ifStatement.AddProduction("if", "\\(", ident, "\\)", "then", statement, "else", statement);

                statement.AddProduction(ifStatement);
                statement.AddProduction(ident, "=", ident);

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
            INonTerminal<int> nonTerminal = configurator.CreateNonTerminal();
            nonTerminal.DebugName = "NonTerm";
            nonTerminal.AddProduction("this", "is", "a", "string");
            nonTerminal.AddProduction("this", "is", "a", "test");

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
                INonTerminal<object> x = configurator.CreateNonTerminal();
                x.DebugName = "X";

                t = configurator.CreateNonTerminal();
                t.DebugName = "T";

                y = configurator.CreateNonTerminal();
                y.DebugName = "Y";

                x.AddProduction(t);
                x.AddProduction(y);

                t.AddProduction("A");
                y.AddProduction("A");

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
                var t = configurator.CreateNonTerminal();
                t.AddProduction(t);
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
                var a = configurator.CreateNonTerminal();
                a.DebugName = "a";
                var b = configurator.CreateNonTerminal();
                b.DebugName = "b";
                var c = configurator.CreateNonTerminal();
                c.DebugName = "c";

                a.AddProduction(a);
                a.AddProduction(b);
                a.AddProduction(c);

                b.AddProduction("b");

                c.AddProduction("c");

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
            var a = configurator.CreateNonTerminal();
            var b = configurator.CreateNonTerminal();
            var c = configurator.CreateNonTerminal();
            var d = configurator.CreateNonTerminal();

            a.AddProduction(a, b, c, d);
            b.AddProduction("b");
            c.AddProduction(b);
            d.AddProduction("d");

            configurator.CreateParser();
        }

        [TestMethod]
        public void TestGrammarWithEpsilonTransitions()
        {
            var configurator = ParserFactory.Configure<int>();
            var func = configurator.CreateNonTerminal();
            func.DebugName = "FUNC";
            var paramList = configurator.CreateNonTerminal();
            paramList.DebugName = "PARAMLIST";
            var optionalParamList = configurator.CreateNonTerminal();
            optionalParamList.DebugName = "OPTIONALPARAMLIST";

            func.AddProduction("func", "(", optionalParamList, ")");
            paramList.AddProduction(paramList, ",", "ident");
            paramList.AddProduction("ident");

            optionalParamList.AddProduction(paramList);
            optionalParamList.AddProduction();

            var parser = configurator.CreateParser();
            parser.Parse("func(ident,ident,ident,ident)");
            parser.Parse("func()");
        }

        [TestMethod]
        public void TestDeepEpsilonChain()
        {
            var configurator = ParserFactory.Configure<int>();
            var a = configurator.CreateNonTerminal();
            var b = configurator.CreateNonTerminal();
            var c = configurator.CreateNonTerminal();
            var d = configurator.CreateNonTerminal();
            var e = configurator.CreateNonTerminal();

            a.AddProduction("a", b);
            b.AddProduction(c);
            b.AddProduction();

            c.AddProduction("d", d);

            d.AddProduction(e);
            d.AddProduction();

            e.AddProduction("e");

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
            var s = configurator.CreateNonTerminal();
            s.DebugName = "S";
            var l = configurator.CreateNonTerminal();
            l.DebugName = "L";
            var r = configurator.CreateNonTerminal();
            r.DebugName = "R";

            s.AddProduction(l, "=", r);
            s.AddProduction(r);

            l.AddProduction("*", r);
            l.AddProduction("id");

            r.AddProduction(l);
            configurator.CreateParser();
        }

        [TestMethod]
        public void TestMultipleEpsilonParametersInARow()
        {
            var configurator = ParserFactory.Configure<int>();
            var a = configurator.CreateNonTerminal();
            a.DebugName = "A";
            var b = configurator.CreateNonTerminal();
            b.DebugName = "B";
            var c = configurator.CreateNonTerminal();
            c.DebugName = "C";
            var d = configurator.CreateNonTerminal();
            d.DebugName = "D";

            a.AddProduction("a", b, c, d, "a");
            b.AddProduction("b");
            b.AddProduction();

            c.AddProduction("c");
            c.AddProduction();
            d.AddProduction("d");
            d.AddProduction();

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
            var s = configurator.CreateNonTerminal();
            s.DebugName = "S";
            var c = configurator.CreateNonTerminal();
            c.DebugName = "C";

            s.AddProduction(c, c);
            c.AddProduction("c", c);
            c.AddProduction("d");

            var parser = configurator.CreateParser();
            parser.Parse("ccccccccdd");

        }

        [TestMethod]
        public void TestSingleRuleTerminalGrammar()
        {
            var configurator = ParserFactory.Configure<int>();
            var s = configurator.CreateNonTerminal();
            s.DebugName = "S";
            s.AddProduction("a", "b", "c", "d");
            var parser = configurator.CreateParser();
            parser.Parse("abcd");
        }
    }
}
