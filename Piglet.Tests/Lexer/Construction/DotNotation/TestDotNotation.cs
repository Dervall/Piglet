using System.IO;
using NUnit.Framework;
using Piglet.Lexer.Construction;
using Piglet.Lexer.Construction.DotNotation;

namespace Piglet.Tests.Lexer.Construction.DotNotation
{
    [TestFixture]
    public class TestDotNotation
    {
        [Test]
        public void TestDotForNFA()
        {
            // Make sure it does not crash and does not return null.
            var nfa = NfaBuilder.Create(new ShuntingYard(new RegExLexer(new StringReader("(a|b)+bcd"))));
            string dotString = nfa.AsDotNotation(null);
            Assert.IsNotNull(dotString);
        }

        [Test]
        public void TestDotForDFA()
        {
            // Make sure it does not crash and does not return null.
            var dfa = DFA.Create(NfaBuilder.Create(new ShuntingYard(new RegExLexer(new StringReader("(a|b)+bcd")))));
            string dotString = dfa.AsDotNotation(null);
            Assert.IsNotNull(dotString);
        }
        
        [Test]
        public void Should_be_able_to_mark_the_last_step_as_active_for_DFA()
        {
            // Make sure it does not crash and does not return null.
            var dfa = DFA.Create(NfaBuilder.Create(new ShuntingYard(new RegExLexer(new StringReader("(a|b)+bcd")))));
            string dotString = dfa.AsDotNotation("abc");
            Assert.IsNotNull(dotString);
            Assert.IsTrue(dotString.Contains("4 [ fillcolor=\"green\" style=\"filled\"]"));
        }
        
        [Test]
        public void Should_be_able_to_mark_active_state_for_NFA()
        {
            // Make sure it does not crash and does not return null.
            var nfa = NfaBuilder.Create(new ShuntingYard(new RegExLexer(new StringReader("(a|b)+bcd"))));
            string dotString = nfa.AsDotNotation("bbc");
            Assert.IsNotNull(dotString);
            Assert.IsTrue(dotString.Contains("8 [ fillcolor=\"green\" style=\"filled\"]"));
        }

        [Test]
        public void Should_return_matched_string_when_successful()
        {
            // Make sure it does not crash and does not return null.
            var nfa = NfaBuilder.Create(new ShuntingYard(new RegExLexer(new StringReader("(a|b)+bcd"))));
            var result = nfa.Stimulate("bbc");

            Assert.AreEqual("bbc", result.Matched);
        }
        
        [Test]
        public void Should_only_return_successfully_matched_string()
        {
            // Make sure it will only return part of the string.
            var nfa = NfaBuilder.Create(new ShuntingYard(new RegExLexer(new StringReader("(a|b)+bcd"))));
            var result = nfa.Stimulate("bbcxxxx");

            Assert.AreEqual("bbc", result.Matched);
        }
    }
}
