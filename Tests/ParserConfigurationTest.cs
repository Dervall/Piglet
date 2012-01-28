using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet;
using Piglet.Lexer;
using Piglet.Parser;
using Piglet.Parser.Configuration;

namespace TestParser
{
    [TestClass]
    public class ParserIntegrationTest
    {
        [TestMethod]
        public void TestACalculator()
        {
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

//            configurator.AutoEscapeLiterals = true;
            configurator.OnAccept(expr, s => s);
            configurator.AugmentGrammar();

            ILexer<int> lexer = configurator.CreateLexer();
            IParser<int> parser = configurator.CreateParser();
            lexer.Source = new StringReader("7+8*2");
            int result = parser.Parse(lexer);

            Assert.AreEqual(23, result);
        }
    }
}
