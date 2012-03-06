using Piglet.Parser;

namespace Piglet.Demo.Parser
{
    /// <summary>
    /// This code written in support for blog article on parsing
    /// </summary>
    public class BlogFormatParser
    {
        public static void RunFluent()
        {
            var config = ParserFactory.Fluent();
            var name = config.Expression();
            name.ThatMatches("[a-z]+");

            var obj = config.Rule();
            var attribute = config.Rule();

            obj.IsMadeUp.By(name)
                .Followed.By("{")
                .Followed.ByListOf(obj).ThatIs.Optional
                .Followed.ByListOf(attribute).ThatIs.Optional
                .Followed.By("}");
            attribute.IsMadeUp.By("[")
                .Followed.By(name)
                .Followed.By("=")
                .Followed.By(config.QuotedString).Followed.By("]");

            var parser = config.CreateParser();

            parser.Parse("fruits {" +
                         "    banana {" +
                         "        [tasty=\"true\"]" +
                         "        [colour=\"yellow\"]" +
                         "    }" +
                         "" +
                         "    orange {" +
                         "    }" +
                         "    " +
                         "    [eatable=\"if not rotten\"]" +
                         "}");
        }

        public static void RunTechnical()
        {
            var configurator = ParserFactory.Configure<bool>();
                configurator.LexerSettings.Ignore = new[] {@"\s+"};

                var name = configurator.Terminal("[a-z]+");
                var quotedString = configurator.Terminal("\"[^\"]+\"");

                var obj = configurator.NonTerminal();
                var optionalObjectList = configurator.NonTerminal();
                var objectList = configurator.NonTerminal();
                var optionalAttributeList = configurator.NonTerminal();
                var attributeList = configurator.NonTerminal();
                var attribute = configurator.NonTerminal();

                obj.Productions(p => p.AddProduction(name, "{", optionalObjectList, optionalAttributeList, "}"));
                
                optionalObjectList.Productions(p =>
                {
                    p.AddProduction(objectList);
                    p.AddProduction();
                });

                objectList.Productions(p =>
                {
                    p.AddProduction(objectList, obj);
                    p.AddProduction(obj);
                });
                
                optionalAttributeList.Productions(p =>
                {
                    p.AddProduction(attributeList);
                    p.AddProduction();
                });

                attributeList.Productions(p => 
                {
                    p.AddProduction(attributeList, attribute);
                    p.AddProduction(attribute);
                });

                attribute.Productions(p => p.AddProduction("[", name, "=", quotedString, "]"));

            var parser = configurator.CreateParser();
            parser.Parse("fruits {" +
                         "    banana {" +
                         "        [tasty=\"true\"]" +
                         "        [colour=\"yellow\"]" +
                         "    }" +
                         "" +
                         "    orange {" +
                         "    }" +
                         "    " +
                         "    [eatable=\"if not rotten\"]" +
                         "}");
        }
    }
}
