using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Lexer;

namespace TestParser
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
            lexer.Source = new StringReader(input);
            Assert.AreEqual(regEx, lexer.Next().Item2);
        }

        [TestMethod]
        public void TestEscapedCharacters()
        {
            string regEx = "\\++";
            string input = "++++";

            CheckMatch(input, regEx);
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
    }
}
