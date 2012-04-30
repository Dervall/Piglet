using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
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
        public void TestDistinguishRanges()
        {
            var r1 = new CharSet();
            r1.AddRange('a', 'k');
            var r2 = new CharSet();
            r2.AddRange('g', 'z');

            r1.DistinguishRanges(r2);
            r2.DistinguishRanges(r1);

            Assert.AreEqual(2, r1.Ranges.Count());
            Assert.AreEqual(2, r2.Ranges.Count());
            
            r1.DistinguishRanges(r2);
            r2.DistinguishRanges(r1);

            Assert.AreEqual(2, r1.Ranges.Count());
            Assert.AreEqual(2, r2.Ranges.Count());
        }

        [Test]
        public void TestDistinguishRangesWithSingleChar()
        {
            var r1 = new CharSet();
            r1.AddRange('a', 'k');
            var r2 = new CharSet();
            r2.Add('a');
            r2.Add('k');

            r1.DistinguishRanges(r2);
            r2.DistinguishRanges(r1);

            r1.DistinguishRanges(r2);
            r2.DistinguishRanges(r1);

            Assert.IsFalse(r1.DistinguishRanges(r2));
            Assert.IsFalse(r2.DistinguishRanges(r1));
        }

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
