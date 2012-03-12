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

            var a = configurator.CreateNonTerminal();
            a.DebugName = "a";
            var b = configurator.CreateNonTerminal();
            b.DebugName = "b";
            var c = configurator.CreateNonTerminal();
            c.DebugName = "c";

            a.AddProduction(b);
            a.AddProduction(c);

            b.AddProduction("b");
            c.AddProduction("c");

            var grammar = (IGrammar<int>)configurator;
            var parser = configurator.CreateParser();

            string debugString = parser.ParseTable.ToDebugString(grammar, 3);
            Assert.IsNotNull(debugString);
        }

    }
}
