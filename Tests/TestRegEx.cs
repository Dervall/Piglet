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
        ILexer<string> CreateLexer(string regEx)
        {
            return LexerFactory<string>.Configure(c => c.Token(regEx, f => regEx));
        }
            
        [TestMethod]
        public void TestEscapedCharacters()
        {
            ILexer<string> lexer = CreateLexer("\\++");
            lexer.Source = new StringReader("++++");
            Assert.AreEqual("\\++", lexer.Next().Item2);
        }
    }
}
