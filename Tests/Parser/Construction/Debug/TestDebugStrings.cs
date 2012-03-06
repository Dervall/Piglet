using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Parser;
using Piglet.Parser.Construction;
using Piglet.Parser.Construction.Debug;

namespace Piglet.Tests.Parser.Construction.Debug
{
    [TestClass]
    public class TestDebugStrings
    {
        [TestMethod]
        public void TestParseTableToString()
        {
            // Need to get our hands on a parse table, which isn't the easiest thing for a normal user
            // since its encapsulated and destroyed. However, we have magic casts at our disposal.
            // We are simply going to test that it doesn't break, not really what it contains.
            var configurator = ParserFactory.Configure<int>();

            var a = configurator.NonTerminal();
            a.DebugName = "a";
            var b = configurator.NonTerminal();
            b.DebugName = "b";
            var c = configurator.NonTerminal();
            c.DebugName = "c";
            a.Productions(p =>
            {
                p.AddProduction(b);
                p.AddProduction(c);
            });
            b.Productions(p => p.AddProduction("b"));
            c.Productions(p => p.AddProduction("c"));

            var grammar = (IGrammar<int>)configurator;
            var parser = (LRParser<int>)configurator.CreateParser();

            string debugString = parser.Table.ToDebugString(grammar, 3);
            Assert.IsNotNull(debugString);
        }

    }
}
