using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet;

namespace TestParser
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestACalculator()
        {
            var configurator = ParserConfiguratorFactory.CreateConfigurator<int>();

            ITerminal<int> number = configurator.Terminal("d+", int.Parse);
            number.DebugName = "number";

            INonTerminal<int> start = configurator.NonTerminal();
            start.DebugName = "start";
            INonTerminal<int> term = configurator.NonTerminal();
            term.DebugName = "term";
            INonTerminal<int> expr = configurator.NonTerminal();
            expr.DebugName = "expr";
            INonTerminal<int> factor = configurator.NonTerminal();
            factor.DebugName = "factor";

            start.Productions(p => p.Production(expr, "\n").OnReduce(s => s[0]));
            
            expr.Productions(p => {
                                      p.Production(expr, "+", term).OnReduce(s => s[0] + s[3]);
                                      p.Production(term).OnReduce(s => s[0]);
                                  });

            term.Productions(p =>
                                 {
                                     p.Production(term, "*", factor).OnReduce(s => s[0] * s[2]);
                                     p.Production(factor).OnReduce(s => s[0]);
                                 });

            factor.Productions(p =>
                                {
                                    p.Production(number).OnReduce(s => s[0]);
                                    p.Production("(", expr, ")").OnReduce(s => s[1]);
                                });

            configurator.OnAccept(start, s => s);

            IParser<int> parser = configurator.CreateParser();

            // This is what will actually RUN once we get this stuff up to par

   //         int result = parser.Parse("7+8*2\n");
 //           Assert.AreEqual(23, result);
        }
    }
}
