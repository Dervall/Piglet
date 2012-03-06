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

            var quotedString = configurator.Terminal("\"(\\\\.|[^\"])*\"", f => f.Substring(1, f.Length - 2));
            var doubleValue = configurator.Terminal(@"\d+\.\d+", f => double.Parse(f));
            var integerValue = configurator.Terminal(@"\d+", f => int.Parse(f));

            var jsonObject = configurator.NonTerminal();
            var optionalElementList = configurator.NonTerminal();
            var elementList = configurator.NonTerminal();
            var element = configurator.NonTerminal();
            var value = configurator.NonTerminal();
            var array = configurator.NonTerminal();
            var optionalValueList = configurator.NonTerminal();
            var valueList = configurator.NonTerminal();

            jsonObject.Productions(p => p.AddProduction("{", optionalElementList, "}")
                                         .SetReduceFunction(f => new JsonObject { Elements = (List<JsonElement>)f[1] }));

            optionalElementList.Productions(p =>
            {
                p.AddProduction(elementList)
                 .SetReduceFunction(f => f[0]);

                p.AddProduction()
                 .SetReduceFunction(f => new List<JsonElement>());
            });

            elementList.Productions(p =>
            {
                p.AddProduction(elementList, ",", element)
                 .SetReduceFunction(f =>
                 {
                     var list = (List<JsonElement>)f[0];
                     list.Add((JsonElement)f[2]);
                     return list;
                 });

                p.AddProduction(element)
                 .SetReduceFunction(f => new List<JsonElement> { (JsonElement)f[0] });
            });

            element.Productions(p => p.AddProduction(quotedString, ":", value)
                                      .SetReduceFunction(f => new JsonElement { Name = (string)f[0], Value = f[2] }));

            value.Productions(p =>
            {
                p.AddProduction(quotedString).SetReduceFunction(f => f[0]);
                p.AddProduction(integerValue).SetReduceFunction(f => f[0]);
                p.AddProduction(doubleValue).SetReduceFunction(f => f[0]);
                p.AddProduction(jsonObject).SetReduceFunction(f => f[0]);
                p.AddProduction(array).SetReduceFunction(f => f[0]);
                p.AddProduction("true").SetReduceFunction(f => true);
                p.AddProduction("false").SetReduceFunction(f => false);
                p.AddProduction("null").SetReduceFunction(f => null);
            });

            array.Productions(p => p.AddProduction("[", optionalValueList, "]")
                                    .SetReduceFunction(f => ((List<object>)f[1]).ToArray()));

            optionalValueList.Productions(p =>
            {
                p.AddProduction(valueList)
                 .SetReduceFunction(f => f[0]);
                p.AddProduction()
                 .SetReduceFunction(f => new List<object>());
            });

            valueList.Productions(p =>
            {
                p.AddProduction(valueList, ",", value)
                 .SetReduceFunction(f =>
                 {
                     var list = (List<object>)f[0];
                     list.Add(f[2]);
                     return list;
                 });
                p.AddProduction(value)
                 .SetReduceFunction(f => new List<object> { f[0] });
            });

            configurator.LexerSettings.EscapeLiterals = true;
            configurator.LexerSettings.Ignore = new[] { @"\s+" };
            var parser = configurator.CreateParser();

            var jObject = (JsonObject)parser.Parse("{ \"Property1\":\"va\\\"lue\", \"IntegerProperty\" : 1234 }");
        }
    }
}
