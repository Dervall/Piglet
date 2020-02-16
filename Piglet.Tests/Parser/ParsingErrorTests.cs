using NUnit.Framework;
using Piglet.Parser;

namespace Piglet.Tests.Parser
{
    [TestFixture]
    public class ParsingErrorTests
    {
        [Test]
        public void TestBadToken()
        {
            var c = ParserFactory.Configure<int>();
            var a = c.CreateNonTerminal();
            c.CreateTerminal("b");

            a.AddProduction(a, "a");
            a.AddProduction("a");

            var parser = c.CreateParser();
            try
            {
                parser.Parse("aa    aaa\naa a a a\na a a b");
                Assert.Fail("No error for bad token");
            }
            catch (ParseException e)
            {
                Assert.AreEqual(3, e.LexerState.CurrentLineNumber);
                Assert.AreEqual("a a a b", e.LexerState.CurrentLine);
            }
        }
    }
}
