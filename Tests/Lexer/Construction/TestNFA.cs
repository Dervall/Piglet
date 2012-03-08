using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Lexer.Construction;

namespace Piglet.Tests.Lexer.Construction
{
    [TestClass]
    public class TestNFA
    {
        [TestMethod]
        public void TestConstructWithDigit()
        {
            NFA nfa = NFACreate("\\d+");
            Assert.AreEqual(3, nfa.States.Count());
            Assert.AreEqual(3, nfa.Transitions.Count());
        }

        private NFA NFACreate(string s)
        {
            return NfaBuilder.Create(new ShuntingYard(new RegExLexer(new StringReader(s))));
        }

        [TestMethod]
        public void TestRepeat()
        {
            NFA nfa = NFACreate("a*");
            Assert.AreEqual(4, nfa.States.Count());
            Assert.AreEqual(4, nfa.Transitions.Count());
        }

        [TestMethod]
        public void TestOneOrMore()
        {
            NFA nfa = NFACreate("a+");
            Assert.AreEqual(3, nfa.States.Count());
            Assert.AreEqual(3, nfa.Transitions.Count());
        }

        [TestMethod]
        public void TestOneOrOnce()
        {
            NFA nfa = NFACreate("a?");
            Assert.AreEqual(2, nfa.States.Count());
            Assert.AreEqual(2, nfa.Transitions.Count());
        }

        [TestMethod]
        public void TestConnectThreeNFA()
        {
            // Dragon book example
            NFA nfa = NFACreate("a+");
            NFA nfa2 = NFACreate("abb");
            NFA nfa3 = NFACreate("a*b+");

            int numStatesPreMerge = nfa.States.Count() + nfa2.States.Count() + nfa3.States.Count();

            NFA final = NFA.Merge(new List<NFA> { nfa, nfa2, nfa3 });
            Assert.AreEqual(numStatesPreMerge + 1, final.States.Count);
        }

        [TestMethod]
        public void TestAlternate()
        {
            NFA nfa = NFACreate("a|b");

            // This forms a little elongated diamond, tests the number of states
            Assert.AreEqual(6, nfa.States.Count());
            Assert.AreEqual(6, nfa.Transitions.Count());
        }

        [TestMethod]
        public void TestClosure()
        {
            NFA nfa = NFACreate("(a|b)*cd");
            IList<NFA.State> s0Closure = nfa.Closure(new[] { nfa.StartState }).ToList();

            // In this sample 6 stats are reachable
            Assert.AreEqual(6, s0Closure.Count());

            // The same state is never in the closure twice. This checks that
            Assert.AreEqual(6, s0Closure.Select(f => f.StateNumber).Distinct().Count());
        }

        [TestMethod]
        public void TestConcatenate()
        {
            NFA nfa = NFACreate("abc");
            Assert.AreEqual(4, nfa.States.Count());
            Assert.AreEqual(3, nfa.Transitions.Count());
        }

        [TestMethod]
        public void TestComplex()
        {
            NFA nfa = NFACreate("a|b*a&b&a*&*ab|&ab|&");
            Assert.AreEqual(30, nfa.States.Count());
            Assert.AreEqual(35, nfa.Transitions.Count());
        }

        [TestMethod]
        public void TestOtherComplex()
        {
            //  (a|b)*a -> ab|*a&a&
            NFA nfa = NFACreate("(a|b)*cd");
            Assert.AreEqual(10, nfa.States.Count());
            Assert.AreEqual(11, nfa.Transitions.Count());
        }
    }
}
