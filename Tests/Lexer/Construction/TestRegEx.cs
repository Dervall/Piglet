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
            IsMatch(input, regEx, true);
        }

        private void IsMatch(string input, string regEx, bool shouldMatch)
        {
            ILexer<string> lexer = CreateLexer(regEx);
            lexer.SetSource(new StringReader(input));
            try
            {
                Assert.AreEqual(regEx, lexer.Next().Item2);
            }
            catch (LexerException)
            {
                Assert.False(shouldMatch);
            }
        }

        private void CheckMatchFail(string input, string regEx)
        {
            IsMatch(input, regEx, false);
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
        public void TestCommentRegex()
        {
            var lexer = LexerFactory<string>.Configure(
                f =>
                    {
                        f.Token(@";[^\n]*\n", a => a);
                        f.Token("nextLine", a => a + "%" );
                    });

            lexer.SetSource(@"; this is a comment
nextLine");
            Assert.AreEqual("; this is a comment\r\n", lexer.Next().Item2);
            Assert.AreEqual("nextLine%", lexer.Next().Item2);
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
            CheckMatch("[][][]", @"[\]\[ab]+");
        }

        [Test]
        public void TestExactNumberedRepetition()
        {
            CheckMatch("aaa", "a{3}");
            CheckMatchFail("aaaa", "a{3}");
            CheckMatchFail("aa", "a{3}");
        }

        [Test]
        public void TestAtLeastNumberedRepetition()
        {
            CheckMatch("aaa", "a{3,}");
            CheckMatch("aaaaaaaaaaaaaaaaaaaaaaaaa", "a{3,}");
            CheckMatchFail("aa", "a{3,}");
        }

        [Test]
        public void TestAtLeastComplexRepetition()
        {
            CheckMatch("coolcoolcool", "(cool){3,}");
            CheckMatch("coolcoolcoolcoolcoolcoolcoolcoolcoolcoolcoolcoolcoolcool", "(cool){3,}");
            CheckMatchFail("coolcool", "cool{3,}");
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
            CheckMatchFail("coolcoolcoolcoolcoolcool", "(cool){3:5}");
            CheckMatch("coolcoolcoolcoolcool", "(cool){3:5}");
            CheckMatch("coolcoolcoolcool", "(cool){3:5}");
            CheckMatch("coolcoolcool", "(cool){3:5}");
            CheckMatchFail("coolcool", "(cool){3:5}");
        }
    }
}
