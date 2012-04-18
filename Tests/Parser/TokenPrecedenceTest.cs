using System;
using NUnit.Framework;
using Piglet.Parser;
using Piglet.Parser.Construction;

namespace Piglet.Tests.Parser
{
    [TestFixture]
    public class TokenPrecedenceTest
    {
        [Test]
        public void TestLeftAssociative()
        {
            var configurator = ParserFactory.Configure<int>();
            var number = configurator.CreateTerminal(@"\d+", int.Parse);
            var plus = configurator.CreateTerminal("\\+");
            var minus = configurator.CreateTerminal("-");
            var mul = configurator.CreateTerminal("\\*");
            var div = configurator.CreateTerminal("/");

            configurator.LeftAssociative(plus, minus);
            configurator.LeftAssociative(mul, div);

            var exp = configurator.CreateNonTerminal();

            exp.AddProduction(exp, plus, exp).SetReduceFunction(s =>
            {
                Console.WriteLine("{0} + {1}", s[0], s[2]);
                return s[0] + s[2];
            });
            exp.AddProduction(exp, minus, exp).SetReduceFunction(s =>
            {
                Console.WriteLine("{0} - {1}", s[0], s[2]);
                return s[0] - s[2];
            });
            exp.AddProduction(exp, mul, exp).SetReduceFunction(s =>
            {
                Console.WriteLine("{0} * {1}", s[0], s[2]);
                return s[0] * s[2];
            });
            exp.AddProduction(exp, div, exp).SetReduceFunction(s =>
            {
                Console.WriteLine("{0} / {1}", s[0], s[2]);
                return s[0] / s[2];
            });
            exp.AddProduction("(", exp, ")").SetReduceFunction(s =>
            {
                Console.WriteLine("Paranthesis ({0})", s[1]);
                return s[1];
            });
            exp.AddProduction(number).SetReduceFunction(s => s[0]);

            var parser = configurator.CreateParser();
            Assert.AreEqual(-2, parser.Parse("1 - (1 + (1 * 2))"));
            Assert.AreEqual(2, parser.Parse("(((1 - 1) + 1) * 2)"));
            Assert.AreEqual(4 - 7 - 3, parser.Parse("4 - 7 - 3"));
            Assert.AreEqual(1 + 2 - 3 * 4, parser.Parse("1 + 2 - 3 * 4"));
            Assert.AreEqual(5 * 5 / 5, parser.Parse("5*5/5"));

            Assert.AreEqual(1 + 2 - 3 * 4 / 5 + 124 * 8, parser.Parse("1 + 2 - 3 * 4 / 5 + 124 * 8"));
        }

        [Test]
        public void TestRightAssociativity()
        {
            var configurator = ParserFactory.Configure<int>();
            var number = configurator.CreateTerminal(@"\d+",
                                                int.Parse);
            var minus = configurator.CreateTerminal("-");

            configurator.RightAssociative(minus);

            var exp = configurator.CreateNonTerminal();

            exp.AddProduction(exp, minus, exp).SetReduceFunction(f => f[0] - f[2]);
            exp.AddProduction(number).SetReduceFunction(f => f[0]);

            var parser = configurator.CreateParser();
            Assert.AreEqual(4 - (7 - 3), parser.Parse("4 - 7 - 3"));
        }

        [Test]
        public void TestNonAssociativity()
        {
            try
            {

                // Configure an illegal rule
                var configurator = ParserFactory.Configure<int>();

                var number = configurator.CreateTerminal(@"\d+",
                                                        int.Parse);
                var equals = configurator.CreateTerminal("=");

                configurator.NonAssociative(equals);


                var exp = configurator.CreateNonTerminal();

                exp.AddProduction(exp, equals, exp)
                    .SetReduceFunction(f => f[0] - f[2]);
                exp.AddProduction(number).SetReduceFunction(
                    f => f[0]);

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
