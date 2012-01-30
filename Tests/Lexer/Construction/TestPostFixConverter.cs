using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Lexer.Construction;

namespace Piglet.Tests.Lexer.Construction
{
    [TestClass]
    public class TestPostFixConverter
    {

        [TestMethod]
        public void TestPreservesEscapes()
        {
            Assert.AreEqual("\\p\\e&\\r&+", PostFixConverter.ToPostFix("(\\p\\e\\r)+"));
        }

        [TestMethod]
        public void TestPostFix()
        {
            string regEx = "((a|b)*aba*)*(a|b)(a|b)";
            string postFix = PostFixConverter.ToPostFix(regEx);
            Assert.AreEqual("ab|*a&b&a*&*ab|&ab|&", postFix);
        }

        [TestMethod]
        public void TestRangePostFix()
        {
            // Postfix should preserve ranges as=is
            Assert.AreEqual("[a-b][xyz]|", PostFixConverter.ToPostFix("[a-b]|[xyz]"));
        }
    }
}
