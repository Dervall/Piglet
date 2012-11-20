using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Parser;

namespace Piglet.Tests.Parser
{
	[TestClass]
	public class TestContextualParser
	{
		[TestMethod]
		public void TestContextualParse()
		{
			//var configurator = ParserFactory.Configure<List<string>, int>();
			//var a = configurator.CreateTerminal("a", (ctx, s) =>
			//    {
			//        ctx.Add("a");
			//        return 1; 
			//    });
			//var b = configurator.CreateTerminal("b", (ctx, s) =>
			//    {
			//        ctx.Add("b");
			//        return 1;
			//    });
			//var s = configurator.CreateNonTerminal();
			//s.AddProduction(a, b).SetReduceFunction((ctx, p) => p[0] + p[1]);

			//var parser = configurator.CreateParser();
			//var context = new List<string>();
			//Assert.AreEqual(3, parser.Parse("ab"));
			//Assert.AreEqual(2, context.Count);
			//Assert.IsTrue(context.Contains("a"));
			//Assert.IsTrue(context.Contains("b"));
		}
	}
}
