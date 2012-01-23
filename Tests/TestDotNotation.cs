using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Lexer.Construction;
using Piglet.Lexer.Construction.DotNotation;

namespace TestParser
{
    [TestClass]
    public class TestDotNotation
    {
        [TestMethod]
        public void TestDotForNFA()
        {
            var nfa = NFA.Create(PostFixConverter.ToPostFix("((hej)|(tjo))+hopp"));
            string dotString = nfa.AsDotNotation();
            Console.WriteLine(dotString);
            Console.WriteLine();
        }

        [TestMethod]
        public void TestDotForDFA()
        {
            var dfa = DFA.Create(NFA.Create(PostFixConverter.ToPostFix("((hej)|(tjo))+hopp")));
            string dotString = dfa.AsDotNotation();
            Console.WriteLine(dotString);
            Console.WriteLine();
        }
    }
}
