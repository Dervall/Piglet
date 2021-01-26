using System;
using System.Linq;
using NUnit.Framework;
using Piglet.Parser;

namespace Piglet.Tests.Parser
{
    [TestFixture]
    public class ErrorRecoveryTests
    {
        [Test]
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
            a.AddProduction("a").SetReduceFunction((int[] _) => 1);

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

        [Test]
        public void TestErrorRecoveryInFluentConfiguration()
        {
            int caughtErrors = 0;
            var configurator = ParserFactory.Fluent();

            var listOfA = configurator.Rule();
            var a = configurator.Rule();
            var terminatedA = configurator.Rule();

            a.IsMadeUp.By(a).As("Init").Followed.By("a").WhenFound(f => f.Init + 1)
                   .Or.By("a").WhenFound( f => 1);

            terminatedA.IsMadeUp.By(a).As("A").Followed.By(";").WhenFound(f => f.A)
                .Or.By(configurator.Error).Followed.By(";").WhenFound(f =>
                                                                          {
                                                                              Console.WriteLine(f.Error);
                                                                              ++caughtErrors;
                                                                              return 0;
                                                                          });
            
            listOfA.IsMadeUp.By(terminatedA).As("A").WhenFound(f => f.A)
                .Or.By(listOfA).As("A").Followed.By(terminatedA).As("TA").WhenFound(f => f.A + f.TA);

            // Bogus terminal that isn't in any sort of use
            var b = configurator.Expression();
            b.ThatMatches("b").AndReturns(f => null);

            var parser = configurator.CreateParser();

            const string legal = "aaaaaaa;aaaaaaaaaa;aaa;aa;a;aaaaa;aaaaaaaa;";
            var legalAs = (int)parser.Parse(legal);
            Assert.AreEqual(legal.Aggregate(0, (i, c) => c == 'a' ? i + 1 : i), legalAs);
            Assert.AreEqual(0, caughtErrors);

            const string withErrors = "aaaaaaa;aaaaaabaaa;aaa;aa;a;aaaaa;aaaaaaaa;";
            legalAs = (int)parser.Parse(withErrors);
            Assert.AreEqual("aaaaaaa;THESEAREEATEN;aaa;aa;a;aaaaa;aaaaaaaa;".Aggregate(0, (i, c) => c == 'a' ? i + 1 : i), legalAs);
            Assert.AreEqual(1, caughtErrors);
        }

        [Test]
        public void TestExpectedInput()
        {
            var configurator = ParserFactory.Configure<int>();

            var word = configurator.CreateTerminal("[a-z]+");
            word.DebugName = "word";
            var illegalWord = configurator.CreateTerminal("[A-Z]+"); // Never legal, used to cause errors
            illegalWord.DebugName = "illegalWord";

            var list = configurator.CreateNonTerminal();
            list.DebugName = "list";
            var element = configurator.CreateNonTerminal();
            element.DebugName = "element";

            element.AddProduction("{", list, "}");
            element.AddProduction(word);

            list.AddProduction(list, ",", element);
            list.AddProduction(element);

            var parser = configurator.CreateParser();
            
            // This should parse
            parser.Parse("a, b, {c, d, e, f, {g}, h, i}, {j, k}");

            // You shall not parse
            try
            {
                parser.Parse("a, b, {c, d, e, F, {g}, h, i}, {j, k}");    
            }
            catch (ParseException e)
            {
                // This will fail on the F
                // The exception should contain that the expected input is word or {
                var expectedTokens = e.ExpectedTokens;
                Assert.AreEqual(2, expectedTokens.Length);
                Assert.IsTrue(expectedTokens.Contains("word"));
                Assert.IsTrue(expectedTokens.Contains("{"));
            }

            // You shall not parse
            try
            {
                parser.Parse("a, b, {c, d, e, f f, {g}, h, i}, {j, k}");
            }
            catch (ParseException e)
            {
                // This will fail on the second f since there is no comma
                // The exception should contain that the expected input is , or }
                var expectedTokens = e.ExpectedTokens;

                // EOF is here, since this is a LALR1 parser, even though it not strictly ballroom to use that token here
                // the error actually appears after a reduce. This is OK
                Assert.AreEqual(3, expectedTokens.Length); 
                Assert.IsTrue(expectedTokens.Contains(","));
                Assert.IsTrue(expectedTokens.Contains("}"));
            }
        }
    }
}
