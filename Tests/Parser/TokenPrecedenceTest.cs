using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Parser;

namespace Piglet.Tests.Parser
{
    [TestClass]
    public class TokenPrecedenceTest
    {
        [TestMethod]
        public void TestLeftAssociative()
        {
            var parser = ParserFactory.Configure<int>(configurator =>
            {
                var number = configurator.Terminal(@"\d+", int.Parse);
                var plus = configurator.Terminal("\\+");
                var minus = configurator.Terminal("-");
                var mul = configurator.Terminal("\\*");
                var div = configurator.Terminal("/");

                configurator.LeftAssociative(plus, minus);
                configurator.LeftAssociative(mul, div);

                var exp = configurator.NonTerminal();
                exp.Productions(p =>
                {
                    p.Production(exp, plus, exp).OnReduce(s => s[0] + s[2]);
                    p.Production(exp, minus, exp).OnReduce(s => s[0] - s[2]);
                    p.Production(exp, mul, exp).OnReduce(s => s[0] * s[2]);
                    p.Production(exp, div, exp).OnReduce(s => s[0] / s[2]);
                    p.Production("(", exp, ")").OnReduce(s => s[1]);
                    p.Production(number).OnReduce(s => s[0]);
                });
            });

            Assert.AreEqual(0, parser.Parse("234 - 245 + 1"));
        }
    }
}
