using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Lexer;

namespace Piglet.Tests.Lexer.Construction
{
    [TestClass]
    public class TestRegEx
    {
        private ILexer<string> CreateLexer(string regEx)
        {
            return LexerFactory<string>.Configure(c => c.Token(regEx, f => regEx));
        }

        private void CheckMatch(string input, string regEx)
        {
            ILexer<string> lexer = CreateLexer(regEx);
            lexer.SetSource(new StringReader(input));
            Assert.AreEqual(regEx, lexer.Next().Item2);
        }

        [TestMethod]
        public void TestMatchingQuotesWithEscapes()
        {
            CheckMatch("\" A quoted string with \\\" inside\"", @"""(\\.|[^""])*""");
        }

        [TestMethod]
        public void TestStuff()
        {
            CheckMatch("absc", "a(bs|e)*c");
        }

        [TestMethod]
        public void TestDeepNestedParenthesis()
        {
            CheckMatch("abcde", "(a(b)(c(de)))");
        }

        [TestMethod]
        public void TestEscapedCharacters()
        {
            CheckMatch("++++", "\\++");
        }

        [TestMethod]
        public void TestDigit()
        {
            CheckMatch("123", "\\d+");
        }

        [TestMethod]
        public void TestRange()
        {
            CheckMatch("abcde", "[a-e]+");
        }

        [TestMethod]
        public void TestMultipleRanges()
        {
            CheckMatch("abcdePOPP", "[a-eA-Z]+");
        }

        [TestMethod]
        public void TestAnyCharacter()
        {
            CheckMatch("XHXas!!a.A", "X.X.*\\.A");
        }

        [TestMethod]
        public void TestZeroOrOnce()
        {
            CheckMatch("Color", "Colou?r");
            CheckMatch("Colour", "Colou?r");
        }

        [TestMethod]
        public void TestEscapedParenthesis()
        {
            CheckMatch("(b)", "\\((a|b)\\)");
        }

        [TestMethod]
        public void TestParenthesis()
        {
            CheckMatch("a", "(a)");
        }

        [TestMethod]
        public void TestParenthesisWithRepeat()
        {
            CheckMatch("a", "(a)+");
        }

        [TestMethod]
        public void TestParenthesisWithAlternate()
        {
            CheckMatch("a", "(a|b)");
            CheckMatch("b", "(a|b)");
        }

        [TestMethod]
        public void TestNegateCharacterClass()
        {
            CheckMatch("abcd", "[^ABCD]+");
        }

        [TestMethod]
        public void TestNegateInWrongPosition()
        {
            CheckMatch("^", "[x^]");
        }

        [TestMethod]
        public void SpecialCharactersAreNotThatSpecialInsideAClass()
        {
            CheckMatch("+", "[+]");
            CheckMatch("*", "[*]");
        }

        [TestMethod]
        public void TestNonDigitEscaped()
        {
            CheckMatch("abcde", "\\D+");
        }

        [TestMethod]
        public void TestMatchWhitespace()
        {
            CheckMatch(" \t\n\r", "\\s+");
        }

        [TestMethod]
        public void TestMatchNonWhitespace()
        {
            CheckMatch("jfsdhsd", "\\S+");
        }

        [TestMethod]
        public void TestMatchAlphanumeric()
        {
            CheckMatch("abcdef90210", "\\w+");
        }

        [TestMethod]
        public void TestGreedyOr()
        {
            CheckMatch("heavy", "heavy|metal");
            CheckMatch("metal", "heavy|metal");
        }

        [TestMethod]
        public void TestMatchNonAlphanumeric()
        {
            CheckMatch(" \n!@#", "\\W+");
        }

        [TestMethod]
        public void TestMatchLiteral()
        {
            CheckMatch("ABC", "ABC");
        }

        [TestMethod]
        public void TestEscapedSlash()
        {
            CheckMatch("\\\\", "\\\\+");
        }

        [TestMethod]
        public void TestBracketInCharacterClass()
        {
            CheckMatch("[][][]", "[][ab]+");
        }

        [TestMethod]
        public void TestNumberedRepetition()
        {
            CheckMatch("aaa", "a{3}");
        }

        [TestMethod]
        public void TestMultipleParenthesis()
        {
            CheckMatch("abcd", "(ab)(cd)");
        }

        [TestMethod]
        public void TestNumberedRepetitionWithParenthesis()
        {
            CheckMatch("coolcoolcool", "(cool){3}");
        }

        [TestMethod]
        public void TestNumberedRepetitionWithMaxValue()
        {
            CheckMatch("coolcoolcoolcoolcool", "(cool){3:5}");
            CheckMatch("coolcoolcoolcool", "(cool){3:5}");
            CheckMatch("coolcoolcool", "(cool){3:5}");
        }
    }
}
