using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Lexer.Construction;
using Piglet.Lexer.Construction.DotNotation;

namespace Piglet.Tests.Lexer.Construction.DotNotation
{
    [TestClass]
    public class TestDotNotation
    {
        [TestMethod]
        public void TestDotForNFA()
        {
            // Make sure it does not crash and does not return null.
            var nfa = NfaBuilder.Create(new ShuntingYard(new RegExLexer(new StringReader("(a|b)+bcd"))));
            string dotString = nfa.AsDotNotation(null);
            Assert.IsNotNull(dotString);
        }

        [TestMethod]
        public void TestDotForDFA()
        {
            // Make sure it does not crash and does not return null.
            var dfa = DFA.Create(NfaBuilder.Create(new ShuntingYard(new RegExLexer(new StringReader("(a|b)+bcd")))));
            string dotString = dfa.AsDotNotation(null);
            Assert.IsNotNull(dotString);
        }
    }
}
