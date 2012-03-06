using System.Collections.Generic;
using Piglet.Parser;

namespace Piglet.Demo.Parser
{
    public class JsonParser
    {
        public class JsonElement
        {
            public string Name { get; set; }
            public object Value { get; set; }
        };

        public class JsonObject
        {
            public List<JsonElement> Elements { get; set; }
        };

        public static void Run()
        {
            var configurator = ParserFactory.Configure<object>();

            var quotedString = configurator.CreateTerminal("\"(\\\\.|[^\"])*\"", f => f.Substring(1, f.Length - 2));
            var doubleValue = configurator.CreateTerminal(@"\d+\.\d+", f => double.Parse(f));
            var integerValue = configurator.CreateTerminal(@"\d+", f => int.Parse(f));

            var jsonObject = configurator.CreateNonTerminal();
            var optionalElementList = configurator.CreateNonTerminal();
            var elementList = configurator.CreateNonTerminal();
            var element = configurator.CreateNonTerminal();
            var value = configurator.CreateNonTerminal();
            var array = configurator.CreateNonTerminal();
            var optionalValueList = configurator.CreateNonTerminal();
            var valueList = configurator.CreateNonTerminal();

            jsonObject.AddProduction("{", optionalElementList, "}")
                                         .SetReduceFunction(f => new JsonObject { Elements = (List<JsonElement>)f[1] });

            optionalElementList.AddProduction(elementList)
                 .SetReduceFunction(f => f[0]);

            optionalElementList.AddProduction()
             .SetReduceFunction(f => new List<JsonElement>());

            elementList.AddProduction(elementList, ",", element)
             .SetReduceFunction(f =>
             {
                 var list = (List<JsonElement>)f[0];
                 list.Add((JsonElement)f[2]);
                 return list;
             });

            elementList.AddProduction(element)
             .SetReduceFunction(f => new List<JsonElement> { (JsonElement)f[0] });

            element.AddProduction(quotedString, ":", value)
                                      .SetReduceFunction(f => new JsonElement { Name = (string)f[0], Value = f[2] });


            value.AddProduction(quotedString).SetReduceFunction(f => f[0]);
            value.AddProduction(integerValue).SetReduceFunction(f => f[0]);
            value.AddProduction(doubleValue).SetReduceFunction(f => f[0]);
            value.AddProduction(jsonObject).SetReduceFunction(f => f[0]);
            value.AddProduction(array).SetReduceFunction(f => f[0]);
            value.AddProduction("true").SetReduceFunction(f => true);
            value.AddProduction("false").SetReduceFunction(f => false);
            value.AddProduction("null").SetReduceFunction(f => null);


            array.AddProduction("[", optionalValueList, "]")
                                    .SetReduceFunction(f => ((List<object>)f[1]).ToArray());

            optionalValueList.AddProduction(valueList)
                 .SetReduceFunction(f => f[0]);
            optionalValueList.AddProduction()
             .SetReduceFunction(f => new List<object>());

            valueList.AddProduction(valueList, ",", value)
                 .SetReduceFunction(f =>
                 {
                     var list = (List<object>)f[0];
                     list.Add(f[2]);
                     return list;
                 });
            valueList.AddProduction(value)
             .SetReduceFunction(f => new List<object> { f[0] });

            configurator.LexerSettings.EscapeLiterals = true;
            configurator.LexerSettings.Ignore = new[] { @"\s+" };
            var parser = configurator.CreateParser();

            var jObject = (JsonObject)parser.Parse("{ \"Property1\":\"va\\\"lue\", \"IntegerProperty\" : 1234 }");
        }
    }
}
