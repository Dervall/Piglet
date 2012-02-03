using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Parser.Configuration;

namespace Piglet.Tests.Parser.Configuration
{
    [TestClass]
    public class TestTerminal
    {
        [TestMethod]
        public void TestToString()
        {
            // Create a terminal and call tostring on it
            var terminal = new Terminal<int>("abc", int.Parse) {DebugName = "ABC"};
            var stringValue = terminal.ToString();
            Assert.IsNotNull(stringValue);
        }

        [TestMethod]
        public void TestMultipleDefinedTerminalWithDifferentRegEx()
        {
            try
            {
                var configurator = new ParserConfigurator<string>();
                configurator.Terminal("abc", f => "abc");
                configurator.Terminal("abc", f => "ABC");
                Assert.Fail("No exception for multiple definition of terminal with different regex");
            }
            catch (ParserConfigurationException)
            {
            }
        }
    }
}
