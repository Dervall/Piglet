using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Parser.Configuration;

namespace Piglet.Tests.Parser.Configuration
{
    [TestClass]
    public class TestNonTerminal
    {
        [TestMethod]
        public void TestToString()
        {
            var configurator = new ParserConfigurator<string>();
            var nt = configurator.CreateNonTerminal();
            nt.DebugName = "NT";
            Assert.IsNotNull(nt.ToString());
        }

        [TestMethod]
        public void TestBadProduction()
        {
            try
            {
                var configurator = new ParserConfigurator<string>();
                var nt = configurator.CreateNonTerminal();
                nt.AddProduction("abc", 123, 2.0, false);
                Assert.Fail("No exception for bad type in production rule list");
            }
            catch (ArgumentException)
            {
            }
        }
    }
}
