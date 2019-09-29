using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Piglet.Lexer;
using Piglet.Lexer.Configuration;

namespace Piglet.Tests.Lexer.Construction
{
    [TestFixture]
    public class TestRegEx
    {
        private IEnumerable<Action<ILexerConfigurator<string>>> GetAllConfigurationOptions() => Enum.GetValues(typeof(LexerRuntime)).Cast<LexerRuntime>().Select(
                f => new Action<ILexerConfigurator<string>>(c => c.Runtime = f));

        private IEnumerable<ILexer<string>> CreateLexers(string regEx)
        {
            Action<ILexerConfigurator<string>> tokenAction = c => c.Token(regEx, f => f);
            return GetAllConfigurationOptions().Select(f => 
                LexerFactory<string>.Configure(
                c =>
                    {
                        f(c);
                        tokenAction(c);
                    }));
        }

        private void CheckMatch(string input, string regEx) => IsMatch(input, regEx, true);

        private void CheckMatch(string input, string regEx, string expectedMatch) => IsMatch(input, regEx, true, expectedMatch);

        private void IsMatch(string input, string regEx, bool shouldMatch) => IsMatch(input, regEx, shouldMatch, input);

        private void IsMatch(string input, string regEx, bool shouldMatch, string matchedInput)
        {
            foreach (ILexer<string> lexer in CreateLexers(regEx))
            {
                var lexerInstance = lexer.Begin(new StringReader(input));
                try
                {
                    Tuple<int, string> token = lexerInstance.Next();
                    Assert.AreEqual(0, token.Item1);
                    Assert.AreEqual(matchedInput, token.Item2);
                    Assert.IsTrue(shouldMatch);
                }
                catch (LexerException)
                {
                    Assert.False(shouldMatch);
                }
            }
        }

        private void CheckMatchFail(string input, string regEx) => IsMatch(input, regEx, false);

        [Test]
        public void TestMatchingQuotesWithEscapes() => CheckMatch("\" A quoted string with \\\" inside\"", @"""(\\.|[^""])*""");

        [Test]
        public void TestStuff() => CheckMatch("absc", "a(bs|e)*c");

        [Test]
        public void TestDeepNestedParenthesis() => CheckMatch("abcde", "(a(b)(c(de)))");

        [Test]
        public void TestEscapedCharacters() => CheckMatch("++++", "\\++");

        [Test]
        public void TestDigit() => CheckMatch("123", "\\d+");

        [Test]
        public void TestRange() => CheckMatch("abcde", "[a-e]+");

        [Test]
        public void TestMultipleRanges() => CheckMatch("abcdePOPP", "[a-eA-Z]+");

        [Test]
        public void TestAnyCharacter() => CheckMatch("XHXas!!a.A", "X.X.*\\.A");

        [Test]
        public void TestZeroOrOnce()
        {
            CheckMatch("Color", "Colou?r");
            CheckMatch("Colour", "Colou?r");
        }

        [Test]
        public void TestEscapedParenthesis() => CheckMatch("(b)", "\\((a|b)\\)");

        [Test]
        public void TestParenthesis() => CheckMatch("a", "(a)");

        [Test]
        public void TestParenthesisWithRepeat() => CheckMatch("a", "(a)+");

        [Test]
        public void TestParenthesisWithAlternate()
        {
            CheckMatch("a", "(a|b)");
            CheckMatch("b", "(a|b)");
        }

        [Test]
        public void TestNegateCharacterClass() => CheckMatch("abcd", "[^ABCD]+");

        [Test]
        public void TestNegateInWrongPosition() => CheckMatch("^", "[x^]");

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

            var lexerInstance = lexer.Begin(@"; this is a comment
nextLine");
            Assert.AreEqual("; this is a comment\r\n", lexerInstance.Next().Item2);
            Assert.AreEqual("nextLine%", lexerInstance.Next().Item2);
        }

        [Test]
        public void TestNonDigitEscaped() => CheckMatch("abcde", "\\D+");

        [Test]
        public void TestMatchWhitespace() => CheckMatch(" \t\n\r", "\\s+");

        [Test]
        public void TestMatchNonWhitespace() => CheckMatch("jfsdhsd", "\\S+");

        [Test]
        [TestCase("abcdefghijklmnopqrstuvwxyz")]
        [TestCase("ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        [TestCase("åäöÅÄÖ")]
        [TestCase("_")]
        [TestCase("01234567890")]
        [TestCase("\x16C8\x16C1\x16B7\x16DA\x16D6\x16CF")]//Piglet in Runic
        public void TestMatchWordCharactersInclude(string input) => CheckMatch(input, "\\w+");

        [Test]
        [TestCase("-!\\\"#€%&/()='|<>,.*^¨`´?+;:@$")]
        public void TestMatchWordCharactersExclude(string input) => CheckMatchFail(input, "\\w+");

        [Test]
        public void TestGreedyOr()
        {
            CheckMatch("heavy", "heavy|metal");
            CheckMatch("metal", "heavy|metal");
        }

        [Test]
        public void TestMatchNonAlphanumeric() => CheckMatch(" \n!@#", "\\W+");

        [Test]
        public void TestMatchLiteral() => CheckMatch("ABC", "ABC");

        [Test]
        public void TestEscapedSlash() => CheckMatch("\\\\", "\\\\+");

        [Test]
        public void TestBracketInCharacterClass() => CheckMatch("[][][]", @"[\]\[ab]+");

        [Test]
        public void TestExactNumberedRepetition()
        {
            CheckMatch("aaa", "a{3}");
            CheckMatchFail("a", "a{3}");
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
        public void TestMultipleParenthesis() => CheckMatch("abcd", "(ab)(cd)");

        [Test]
        public void TestNumberedRepetitionWithParenthesis() => CheckMatch("coolcoolcool", "(cool){3}");

        [Test]
        public void TestNumberedRepetitionWithMaxValue()
        {
            CheckMatch("coolcoolcoolcoolcoolcool", "(cool){3:5}", "coolcoolcoolcoolcool");
            CheckMatch("coolcoolcoolcoolcool", "(cool){3:5}");
            CheckMatch("coolcoolcoolcool", "(cool){3:5}");
            CheckMatch("coolcoolcool", "(cool){3:5}");
            CheckMatchFail("coolcool", "(cool){3:5}");
        }
    }
}
