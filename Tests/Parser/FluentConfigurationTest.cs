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
            var config = ParserFactory.Fluent<object>();

            var name = config.Expression();

            var jsonObject = config.Rule();
            var jsonElement = config.Rule();
            var jsonAttributeValue = config.Rule();
            var jsonArray = config.Rule();
            
            jsonObject.IsMadeUp.By(name).Followed.By("{").Followed.ByListOf(jsonElement).ThatIs.Optional.Followed.By("}");

            jsonElement.IsMadeUp.By("[").Followed.By(config.QuotedString).Followed.By(":").Followed.By(jsonAttributeValue);

            jsonAttributeValue.IsMadeUp.By(config.QuotedString)
                .Or.By<int>()
                .Or.By<double>()
                .Or.By(jsonObject)
                .Or.By(jsonArray)
                .Or.By("true")
                .Or.By("false")
                .Or.By("null");

            jsonArray.IsMadeUp.By("[").Followed.ByListOf(jsonAttributeValue).ThatIs.SeparatedBy(",").Optional.Followed.By("]");

            var parser = config.CreateParser();

            var jObject = (JsonObject)parser.Parse("{ \"Property1\":\"va\\\"lue\", \"IntegerProperty\" : 1234 }");
            
        }
    }
}
