using System.IO;
using NUnit.Framework;
using Piglet.Lexer;

namespace Piglet.Tests.Lexer.Construction
{
    [TestFixture]
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

        [Test]
        public void TestMatchingQuotesWithEscapes()
        {
            CheckMatch("\" A quoted string with \\\" inside\"", @"""(\\.|[^""])*""");
        }

        [Test]
        public void TestStuff()
        {
            CheckMatch("absc", "a(bs|e)*c");
        }

        [Test]
        public void TestDeepNestedParenthesis()
        {
            CheckMatch("abcde", "(a(b)(c(de)))");
        }

        [Test]
        public void TestEscapedCharacters()
        {
            CheckMatch("++++", "\\++");
        }

        [Test]
        public void TestDigit()
        {
            CheckMatch("123", "\\d+");
        }

        [Test]
        public void TestRange()
        {
            CheckMatch("abcde", "[a-e]+");
        }

        [Test]
        public void TestMultipleRanges()
        {
            CheckMatch("abcdePOPP", "[a-eA-Z]+");
        }

        [Test]
        public void TestAnyCharacter()
        {
            CheckMatch("XHXas!!a.A", "X.X.*\\.A");
        }

        [Test]
        public void TestZeroOrOnce()
        {
            CheckMatch("Color", "Colou?r");
            CheckMatch("Colour", "Colou?r");
        }

        [Test]
        public void TestEscapedParenthesis()
        {
            CheckMatch("(b)", "\\((a|b)\\)");
        }

        [Test]
        public void TestParenthesis()
        {
            CheckMatch("a", "(a)");
        }

        [Test]
        public void TestParenthesisWithRepeat()
        {
            CheckMatch("a", "(a)+");
        }

        [Test]
        public void TestParenthesisWithAlternate()
        {
            CheckMatch("a", "(a|b)");
            CheckMatch("b", "(a|b)");
        }

        [Test]
        public void TestNegateCharacterClass()
        {
            CheckMatch("abcd", "[^ABCD]+");
        }

        [Test]
        public void TestNegateInWrongPosition()
        {
            CheckMatch("^", "[x^]");
        }

        [Test]
        public void SpecialCharactersAreNotThatSpecialInsideAClass()
        {
            CheckMatch("+", "[+]");
            CheckMatch("*", "[*]");
        }

        [Test]
        public void TestNonDigitEscaped()
        {
            CheckMatch("abcde", "\\D+");
        }

        [Test]
        public void TestMatchWhitespace()
        {
            CheckMatch(" \t\n\r", "\\s+");
        }

        [Test]
        public void TestMatchNonWhitespace()
        {
            CheckMatch("jfsdhsd", "\\S+");
        }

        [Test]
        public void TestMatchAlphanumeric()
        {
            CheckMatch("abcdef90210", "\\w+");
        }

        [Test]
        public void TestGreedyOr()
        {
            CheckMatch("heavy", "heavy|metal");
            CheckMatch("metal", "heavy|metal");
        }

        [Test]
        public void TestMatchNonAlphanumeric()
        {
            CheckMatch(" \n!@#", "\\W+");
        }

        [Test]
        public void TestMatchLiteral()
        {
            CheckMatch("ABC", "ABC");
        }

        [Test]
        public void TestEscapedSlash()
        {
            CheckMatch("\\\\", "\\\\+");
        }

        [Test]
        public void TestBracketInCharacterClass()
        {
            CheckMatch("[][][]", "[][ab]+");
        }

        [Test]
        public void TestNumberedRepetition()
        {
            CheckMatch("aaa", "a{3}");
        }

        [Test]
        public void TestMultipleParenthesis()
        {
            CheckMatch("abcd", "(ab)(cd)");
        }

        [Test]
        public void TestNumberedRepetitionWithParenthesis()
        {
            CheckMatch("coolcoolcool", "(cool){3}");
        }

        [Test]
        public void TestNumberedRepetitionWithMaxValue()
        {
            CheckMatch("coolcoolcoolcoolcool", "(cool){3:5}");
            CheckMatch("coolcoolcoolcool", "(cool){3:5}");
            CheckMatch("coolcoolcool", "(cool){3:5}");
        }
    }
}
