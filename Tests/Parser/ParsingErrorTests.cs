using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Parser;

namespace Piglet.Tests.Parser
{
    [TestClass]
    public class ParsingErrorTests
    {
        [TestMethod]
        public void TestBadToken()
        {
            var c = ParserFactory.Configure<int>();
            var a = c.NonTerminal();
            var b = c.Terminal("b");
            a.Productions(p =>
                            {
                                p.AddProduction(a, "a");
                                p.AddProduction("a");
                            });
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
