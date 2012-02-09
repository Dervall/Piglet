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
            IGrammar<int> grammar = null;
            var parser = (LRParser<int>)ParserFactory.Configure<int>(configurator =>
            {
                var a = configurator.NonTerminal();
                a.DebugName = "a";
                var b = configurator.NonTerminal();
                b.DebugName = "b";
                var c = configurator.NonTerminal();
                c.DebugName = "c";
                a.Productions(p =>
                {
                    p.Production(b);
                    p.Production(c);
                });
                b.Productions(p => p.Production("b"));
                c.Productions(p => p.Production("c"));

                grammar = (IGrammar<int>) configurator;
            });
           
            string debugString = parser.Table.ToDebugString(grammar, 3);
            Assert.IsNotNull(debugString);
        }

    }
}
