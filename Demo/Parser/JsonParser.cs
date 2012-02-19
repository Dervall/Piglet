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
            var parser = ParserFactory.Configure<object>(configurator =>
            {
                var quotedString = configurator.Terminal("\"(\\\\.|[^\"])*\"",    f => f.Substring(1, f.Length - 2));
                var doubleValue = configurator.Terminal(@"\d+\.\d+",            f => double.Parse(f));
                var integerValue = configurator.Terminal(@"\d+",                f => int.Parse(f));

                var jsonObject = configurator.NonTerminal();
                var optionalElementList = configurator.NonTerminal();
                var elementList = configurator.NonTerminal();
                var element = configurator.NonTerminal();
                var value = configurator.NonTerminal();
                var array = configurator.NonTerminal();
                var optionalValueList = configurator.NonTerminal();
                var valueList = configurator.NonTerminal();

                jsonObject.Productions(p => p.Production("{", optionalElementList, "}")
                                             .OnReduce( f => new JsonObject { Elements = (List<JsonElement>) f[1]} ));

                optionalElementList.Productions(p =>
                {
                    p.Production(elementList)
                     .OnReduce(f => f[0]);

                    p.Production()
                     .OnReduce(f => new List<JsonElement>());
                });

                elementList.Productions(p =>
                {
                    p.Production(elementList, ",", element)
                     .OnReduce(f => {
                                       var list = (List<JsonElement>)f[0];
                                       list.Add((JsonElement)f[2]);
                                       return list;
                                   });

                    p.Production(element)
                     .OnReduce(f => new List<JsonElement> { (JsonElement)f[0] });
                });

                element.Productions(p => p.Production(quotedString, ":", value)
                                          .OnReduce(f => new JsonElement { Name = (string)f[0], Value = f[2]}));

                value.Productions(p =>
                {
                    p.Production(quotedString)  .OnReduce(f => f[0]);
                    p.Production(integerValue)  .OnReduce(f => f[0]);
                    p.Production(doubleValue)   .OnReduce(f => f[0]);
                    p.Production(jsonObject)    .OnReduce(f => f[0]);
                    p.Production(array)         .OnReduce(f => f[0]);
                    p.Production("true")        .OnReduce(f => true);
                    p.Production("false")       .OnReduce(f => false);
                    p.Production("null")        .OnReduce(f => null);
                });

                array.Productions(p => p.Production("[", optionalValueList, "]")
                                        .OnReduce( f => ((List<object>)f[1]).ToArray() ));

                optionalValueList.Productions(p =>
                {
                    p.Production(valueList)
                     .OnReduce(f => f[0]);
                    p.Production()
                     .OnReduce(f => new List<object>());
                });

                valueList.Productions(p =>
                {
                    p.Production(valueList, ",", value)
                     .OnReduce(f => {
                                       var list = (List<object>)f[0];
                                       list.Add(f[2]);
                                       return list;
                                   });
                    p.Production(value)
                     .OnReduce(f => new List<object>{f[0]});
                });

                configurator.LexerSettings.EscapeLiterals = true;
                configurator.LexerSettings.Ignore = new [] { @"\s+" };
            });


            var jObject = (JsonObject)parser.Parse("{ \"Property1\":\"va\\\"lue\", \"IntegerProperty\" : 1234 }");
        }
    }
}
