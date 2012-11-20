using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Lexer;

namespace Piglet.Tests.Lexer
{
	[TestClass]
	public class TestContextualLexer
	{
		[TestMethod]
		public void TestMakeContextualLexer()
		{
			var lexer = LexerFactory<List<string>, int>.Configure(c =>
			{
				c.Token("abc", (ctx, s) =>
				{
					ctx.Add("abc");
					return 0;
				});
				c.Token("\\w+", (ctx, s) =>
					{
						ctx.Add("word");
						return 1;
					});
				c.Ignore(" +");
			});
			
			var context = new List<string>();
			var tokens = lexer.Tokenize(context, "abc jajaj abc").ToArray();
			Assert.AreEqual(2, context.Count(s => s == "abc"));
			Assert.AreEqual(1, context.Count(s => s == "word"));
		}
	}
}
