using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Parser;
using Piglet.Parser.Configuration;
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
            var configurator = ParserConfiguratorFactory.CreateConfigurator<int>();
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

            configurator.SetStartSymbol(a);
            var parser = (LRParser<int>)configurator.CreateParser();
            var grammar = (IGrammar<int>) configurator; // Since this is normally NOT exposed
            string debugString = parser.Table.ToDebugString(grammar);
            Assert.IsNotNull(debugString);
        }

    }
}
