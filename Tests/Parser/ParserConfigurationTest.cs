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
                Assert.AreEqual(ident, e.ReduceSymbol);
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
    }
}
