using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Piglet.Lexer.Construction;

namespace Piglet.Tests.Lexer.Construction
{
    [TestFixture]
    public class TestStateMinimization
    {
        private static DFA CreateDfa(string expression)
        {
            return DFA.Create(NfaBuilder.Create(new ShuntingYard(new RegExLexer(new StringReader(expression)))));
        }

        [Test]
        public void TestSimpleMinimization()
        {
            var dfa = CreateDfa("a+|a");
            Assert.AreEqual(3, dfa.States.Count);

            dfa.Minimize();
            Assert.AreEqual(2, dfa.States.Count);
        }

        [Test]
        public void TestSemiComplexMinimization()
        {
            var dfa = CreateDfa("(a|b)+|ab");
            Assert.AreEqual(5, dfa.States.Count);

            dfa.Minimize();
            Assert.AreEqual(2, dfa.States.Count);
        }

        [Test]
        public void Test2ComplexMinimization()
        {
            var dfa = CreateDfa("ac|bc|ef");
            Assert.AreEqual(7, dfa.States.Count);

            dfa.Minimize();
            Assert.AreEqual(4, dfa.States.Count);
        }

        [Test]
        public void TestMoreComplexMinimization()
        {
            var dfa = CreateDfa("a+b+c+|abc|aabbcc");
            Assert.AreEqual(12, dfa.States.Count);

            dfa.Minimize();
            Assert.AreEqual(4, dfa.States.Count);
        }
    }
}
