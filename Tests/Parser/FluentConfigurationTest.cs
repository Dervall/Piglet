using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Parser;

namespace Piglet.Tests.Parser
{
    [TestClass]
    public class FluentConfigurationTest
    {
        public class JsonElement
        {
            public string Name { get; set; }
            public object Value { get; set; }
        };

        public class JsonObject
        {
            public IList<JsonElement> Elements { get; set; }
        };

        [TestMethod]
        public void TestFluentJsonParserConfiguration()
        {
            var config = ParserFactory.Fluent();

            var jsonObject = config.Rule();
            var jsonElement = config.Rule();
            var jsonValue = config.Rule();
            var jsonArray = config.Rule();

            jsonObject.IsMadeUp.By("{")
                      .Followed.ByListOf<JsonElement>(jsonElement).As("ElementList").ThatIs.SeparatedBy(",").And.Optional
                      .Followed.By("}")
                .WhenFound( o => new JsonObject { Elements = o.ElementList } );

            jsonElement.IsMadeUp.By(config.QuotedString).As("Name")
                       .Followed.By(":")
                       .Followed.By(jsonValue).As("Value")
                .WhenFound( o => new JsonElement { Name = o.Name, Value = o.Value } );

            jsonValue.IsMadeUp.By(config.QuotedString)
                .Or.By<int>()
                .Or.By<double>()
                .Or.By(jsonObject)
                .Or.By(jsonArray)
                .Or.By<bool>()
                .Or.By("null").WhenFound(o => null);

            jsonArray.IsMadeUp.By("[")
                     .Followed.ByListOf(jsonValue).As("Values").ThatIs.SeparatedBy(",").And.Optional
                     .Followed.By("]")
                   .WhenFound(o => o.Values);

            var parser = config.CreateParser();

            var jObject = (JsonObject)parser.Parse(
                @"{ 
                     ""Property1"":""va\""lue"", 
                     ""IntegerProperty"" : 1234, 
                     ""array"":[1,2,3,4,5],
                     ""another_object"" : {
                        ""another_property"":13.37
                     },
                     ""empty_object"" : {
                        
                     }
                }");
            Assert.AreEqual(5, jObject.Elements.Count);
        }

        [TestMethod]
        public void TestFluentCalculator()
        {
            var config = ParserFactory.Fluent();
            var expr = config.Rule();
            var term = config.Rule();
            var factor = config.Rule();
            var plusOrMinus = config.Rule();
            var mulOrDiv = config.Rule();

            plusOrMinus.IsMadeUp.By("+").WhenFound(f => '+')
                             .Or.By("-").WhenFound(f => '-');

            expr.IsMadeUp.By(expr).As("Left").Followed.By(plusOrMinus).As("Operator").Followed.By(term).As("Right")
                .WhenFound(f => f.Operator == '+' ? f.Left + f.Right : f.Left - f.Right)
                .Or.By(term);

            mulOrDiv.IsMadeUp.By("*").WhenFound(f => '*')
                          .Or.By("/").WhenFound(f => '/');

            term.IsMadeUp.By(term).As("Left").Followed.By(mulOrDiv).As("Operator").Followed.By(factor).As("Right")
                .WhenFound(f => f.Operator == '*' ? f.Left * f.Right : f.Left / f.Right)
                .Or.By(factor);

            factor.IsMadeUp.By<int>()
                .Or.By("(").Followed.By(expr).As("Expression").Followed.By(")")
                .WhenFound(f => f.Expression);

            var parser = config.CreateParser();

            int result = (int)parser.Parse("7+8*2-2+2");

            Assert.AreEqual(23, result);
        }
    }
}
