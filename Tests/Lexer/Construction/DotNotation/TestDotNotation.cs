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
            var nfa = NFA.Create(PostFixConverter.ToPostFix("((hej)|(tjo))+hopp"));
            string dotString = nfa.AsDotNotation();
            Assert.IsNotNull(dotString);
        }

        [TestMethod]
        public void TestDotForDFA()
        {
            // Make sure it does not crash and does not return null.
            var dfa = DFA.Create(NFA.Create(PostFixConverter.ToPostFix("((hej)|(tjo))+hopp")));
            string dotString = dfa.AsDotNotation();
            Assert.IsNotNull(dotString);
        }
    }
}
