using System;
using NUnit.Framework;
using Piglet.Parser.Configuration;

namespace Piglet.Tests.Parser.Configuration
{
    [TestFixture]
    public class TestNonTerminal
    {
        [Test]
        public void TestToString()
        {
            var configurator = new ParserConfigurator<string>();
            var nt = configurator.CreateNonTerminal();
            nt.DebugName = "NT";
            Assert.IsNotNull(nt.ToString());
        }

        [Test]
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
