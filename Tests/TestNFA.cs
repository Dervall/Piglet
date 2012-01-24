using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Lexer.Construction;

namespace TestParser
{
    [TestClass]
    public class TestNFA
    {
        [TestMethod]
        public void TestConstructWithDigit()
        {
            NFA nfa = NFA.Create("\\d+");
            Console.WriteLine();
        }

        [TestMethod]
        public void TestAcceptRange()
        {
            NFA nfa = NFA.AcceptRange('a', 'c');
            Assert.AreEqual(2, nfa.States.Count);
            Assert.AreEqual(1, nfa.Transitions.Count);
            Assert.AreEqual(3, nfa.Transitions[0].ValidInput.Count());
        }
    }
}
