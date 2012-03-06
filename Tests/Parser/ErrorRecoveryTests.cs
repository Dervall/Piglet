using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Parser;

namespace Piglet.Tests.Parser
{
    [TestClass]
    public class ErrorRecoveryTests
    {
        [TestMethod]
        public void TestRecoverFromErrors()
        {
            // Valid inputs are long lists of "a" followed by a ;.
            // "b" however is a legal token, and the error recovery should 
            // skip tokens until it gets another ;.
            // The error recovery routine should also be called with the exception that is generated at that point
            int caughtErrors = 0;
            var configurator = ParserFactory.Configure<int>();
            
            var listOfA = configurator.CreateNonTerminal();
            var a = configurator.CreateNonTerminal();
            var terminatedA = configurator.CreateNonTerminal();

            a.AddProduction(a, "a").SetReduceFunction(f => f[0] + 1);
            a.AddProduction("a").SetReduceFunction(f => 1);

            terminatedA.AddProduction(a, ";").SetReduceFunction( f => f[0]);
            terminatedA.AddProduction(configurator.ErrorToken, ";").SetErrorFunction((e, f) =>
            {
                Console.WriteLine(e);
                ++caughtErrors;
                return 0;
            });
            
            listOfA.AddProduction(terminatedA).SetReduceFunction(f => f[0]);
            listOfA.AddProduction(listOfA, terminatedA).SetReduceFunction(f => f[0] + f[1]);

            // Bogus terminal that isn't in any sort of use
            configurator.CreateTerminal("b");

            var parser = configurator.CreateParser();

            const string legal = "aaaaaaa;aaaaaaaaaa;aaa;aa;a;aaaaa;aaaaaaaa;";
            int legalAs = parser.Parse(legal);
            Assert.AreEqual(legal.Aggregate(0, (i, c) =>  c == 'a' ? i + 1 : i), legalAs);
            Assert.AreEqual(0, caughtErrors);

            const string withErrors = "aaaaaaa;aaaaaabaaa;aaa;aa;a;aaaaa;aaaaaaaa;";
            legalAs = parser.Parse(withErrors);
            Assert.AreEqual("aaaaaaa;THESEAREEATEN;aaa;aa;a;aaaaa;aaaaaaaa;".Aggregate(0, (i, c) => c == 'a' ? i + 1 : i), legalAs);
            Assert.AreEqual(1, caughtErrors);
        }
    }
}
