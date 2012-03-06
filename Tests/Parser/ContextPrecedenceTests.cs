using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Parser;

namespace Piglet.Tests.Parser
{
    [TestClass]
    public class ContextPrecedenceTests
    {
        [TestMethod]
        public void TestUnaryMinus()
        {
            var configurator = ParserFactory.Configure<int>();
            var expr = configurator.CreateNonTerminal();
            var mul = configurator.CreateTerminal(@"\*");
            var minus = configurator.CreateTerminal("-");
            var number = configurator.CreateTerminal(@"\d+", int.Parse);

            configurator.LeftAssociative(minus);
            configurator.LeftAssociative(mul);
            var highPrecgroup = configurator.LeftAssociative();

            expr.AddProduction(expr, minus, expr).SetReduceFunction(f =>
            {
                Console.WriteLine("{0} - {1}", f[0], f[2]);
                return f[0] - f[2];
            });
            
            expr.AddProduction(expr, mul, expr).SetReduceFunction(f =>
            {
                Console.WriteLine("{0} * {1}", f[0], f[2]);
                return f[0] * f[2];
            });

            var uMinusProduction = expr.AddProduction(minus, expr);
            uMinusProduction.SetReduceFunction( f =>
            {
                Console.WriteLine("-{0}", f[1]);
                return -f[1];
            });
            uMinusProduction.SetPrecedence(highPrecgroup);

            expr.AddProduction(number).SetReduceFunction(f => f[0]);

            var parser = configurator.CreateParser();

            int result = parser.Parse("9 - -5*-4 - 2");
            Assert.AreEqual(9 - -5*-4 - 2, result);
        }
    }
}
