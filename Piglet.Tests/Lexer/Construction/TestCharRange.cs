using NUnit.Framework;
using Piglet.Lexer.Construction;

namespace Piglet.Tests.Lexer.Construction
{
    /// <summary>
    /// Summary description for TestCharRange
    /// </summary>
    [TestFixture]
    public class TestCharRange
    {        
        [Test]
        public void TestExcept()
        {
            var r = new CharSet();
            r.AddRange('a', 'z');

            var r2 = new CharSet();
            r2.AddRange('b', 'k');
            r = r.Except(r2);
            Assert.IsTrue(r.ContainsChar('a'));
            Assert.IsTrue(r.ContainsChar('z'));
            Assert.IsFalse(r.ContainsChar('k'));
            Assert.IsFalse(r.ContainsChar('b'));
        }

        [Test]
        public void TestExceptLeftClip()
        {
            var r = new CharSet();
            r.AddRange('a', 'f');

            var r2 = new CharSet();
            r2.AddRange('c', 'f');
            r = r.Except(r2);
            Assert.IsTrue(r.ContainsChar('b'));
            Assert.IsFalse(r.ContainsChar('c'));
        }

        [Test]
        public void TestExceptRightClip()
        {
            var r = new CharSet();
            r.AddRange('a', 'f');

            var r2 = new CharSet();
            r2.AddRange('a', 'c');
            r = r.Except(r2);
            Assert.IsTrue(r.ContainsChar('d'));
            Assert.IsFalse(r.ContainsChar('c'));
        }
    }
}
