using NUnit.Framework;
using Piglet.Parser.Configuration;

namespace Piglet.Tests.Parser.Configuration
{
    [TestFixture]
    public class TestTerminal
    {
        [Test]
        public void TestToString()
        {
            // Create a terminal and call tostring on it
            var terminal = new Terminal<int>("abc", int.Parse) {DebugName = "ABC"};
            var stringValue = terminal.ToString();
            Assert.IsNotNull(stringValue);
        }

        [Test]
        public void TestMultipleDefinedTerminalWithDifferentRegEx()
        {
            try
            {
                var configurator = new ParserConfigurator<string>();
                configurator.CreateTerminal("abc", f => "abc");
                configurator.CreateTerminal("abc", f => "ABC");
                Assert.Fail("No exception for multiple definition of terminal with different regex");
            }
            catch (ParserConfigurationException)
            {
            }
        }
    }
}
