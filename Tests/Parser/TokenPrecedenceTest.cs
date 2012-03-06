using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Parser;
using Piglet.Parser.Construction;

namespace Piglet.Tests.Parser
{
    [TestClass]
    public class TokenPrecedenceTest
    {
        [TestMethod]
        public void TestLeftAssociative()
        {
            var configurator = ParserFactory.Configure<int>();
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
                p.AddProduction(exp, plus, exp).SetReduceFunction(s =>
                {
                    Console.WriteLine("{0} + {1}", s[0], s[2]);
                    return s[0] + s[2];
                });
                p.AddProduction(exp, minus, exp).SetReduceFunction(s =>
                {
                    Console.WriteLine("{0} - {1}", s[0], s[2]);
                    return s[0] - s[2];
                });
                p.AddProduction(exp, mul, exp).SetReduceFunction(s =>
                {
                    Console.WriteLine("{0} * {1}", s[0], s[2]);
                    return s[0] * s[2];
                });
                p.AddProduction(exp, div, exp).SetReduceFunction(s =>
                {
                    Console.WriteLine("{0} / {1}", s[0], s[2]);
                    return s[0] / s[2];
                });
                p.AddProduction("(", exp, ")").SetReduceFunction(s =>
                {
                    Console.WriteLine("Paranthesis ({0})", s[1]);
                    return s[1];
                });
                p.AddProduction(number).SetReduceFunction(s => s[0]);
            });

            var parser = configurator.CreateParser();
            Assert.AreEqual(-2, parser.Parse("1 - (1 + (1 * 2))"));
            Assert.AreEqual(2, parser.Parse("(((1 - 1) + 1) * 2)"));
            Assert.AreEqual(4 - 7 - 3, parser.Parse("4 - 7 - 3"));
            Assert.AreEqual(1 + 2 - 3 * 4, parser.Parse("1 + 2 - 3 * 4"));
            Assert.AreEqual(5 * 5 / 5, parser.Parse("5*5/5"));

            Assert.AreEqual(1 + 2 - 3 * 4 / 5 + 124 * 8, parser.Parse("1 + 2 - 3 * 4 / 5 + 124 * 8"));
        }

        [TestMethod]
        public void TestRightAssociativity()
        {
            var configurator = ParserFactory.Configure<int>();
            var number = configurator.Terminal(@"\d+",
                                                int.Parse);
            var minus = configurator.Terminal("-");

            configurator.RightAssociative(minus);

            var exp = configurator.NonTerminal();

            exp.Productions(p =>
                                {
                                    p.AddProduction(exp, minus, exp).SetReduceFunction(f => f[0] - f[2]);
                                    p.AddProduction(number).SetReduceFunction(f => f[0]);
                                });

            var parser = configurator.CreateParser();
            Assert.AreEqual(4 - (7 - 3), parser.Parse("4 - 7 - 3"));
        }

        [TestMethod]
        public void TestNonAssociativity()
        {
            try
            {

                // Configure an illegal rule
                var configurator = ParserFactory.Configure<int>();

                var number = configurator.Terminal(@"\d+",
                                                        int.Parse);
                var equals = configurator.Terminal("=");

                configurator.NonAssociative(equals);


                var exp = configurator.NonTerminal();

                exp.Productions(p =>
                                    {
                                        p.AddProduction(exp, equals, exp)
                                            .SetReduceFunction(f => f[0] - f[2]);
                                        p.AddProduction(number).SetReduceFunction(
                                            f => f[0]);
                                    });

                configurator.CreateParser();
                Assert.Fail("You shall not parse!");
            }
            catch (ShiftReduceConflictException<int>)
            {
                // We cool
            }
        }
    }
}
