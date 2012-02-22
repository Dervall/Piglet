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
            var jsonObjectContents = config.Rule();
            var jsonElement = config.Rule();
            var jsonAttributeValue = config.Rule();
            var jsonArray = config.Rule();
            
            jsonObject.IsMadeUp.By("{").Followed.ByListOf(jsonObject).ThatIs.SeparatedBy(",").Optional.Followed.By("}");
            //   .WhenFound(f => {s,sdfbjksfdbj})
      //          .WhenFound.Pick<List<JsonElement>>(1).Return(elementList => { return new JsonObject {Elements = elementList}; });

        //    jsonObjectContents.IsMadeUp.ByListOf(jsonElement).ThatIs.SeparatedBy(",").Optional;

            jsonElement.IsMadeUp.By(config.QuotedString).Followed.By(":").Followed.By(jsonAttributeValue);
            //    .WhenFound.Pick<string,object>(0,2).Return((name, value) => { return new JsonElement {Name = name, Value = value}; );

            jsonAttributeValue.IsMadeUp.By(config.QuotedString)
                .Or.By<int>()
                .Or.By<double>()
                .Or.By(jsonObject)
                .Or.By(jsonArray)
                .Or.By<bool>()
                .Or.By("null");

            jsonArray.IsMadeUp.By("[").Followed.ByListOf(jsonAttributeValue).ThatIs.SeparatedBy(",").Optional.Followed.By("]");
            //       .WhenFound.Pick<object>(1);

            var parser = config.CreateParser();

            var jObject = (JsonObject)parser.Parse("{ \"Property1\":\"va\\\"lue\", \"IntegerProperty\" : 1234 }");
            
        }

        public object X(dynamic z)
        {
            var u = z.kalle;

            var y = z.Value;

            return new object();
        }
    }
}
