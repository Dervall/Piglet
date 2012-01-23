using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Lexer;
using Piglet.Lexer.Construction;

namespace TestParser
{
    [TestClass]
    public class TestPostFixConverter
    {

        [TestMethodAttribute]
        public void TestPostFix()
        {
            string regEx = "((a|b)*aba*)*(a|b)(a|b)";
            string postFix = PostFixConverter.ToPostFix(regEx);
            Assert.AreEqual("ab|*a&b&a*&*ab|&ab|&", postFix);
        }
    }
}
