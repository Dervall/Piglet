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
            name.ThatMatches("[a-z]+").AndReturns(f => f);

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
            configurator.LexerSettings.Ignore = new[] { @"\s+" };

            var name = configurator.CreateTerminal("[a-z]+");
            var quotedString = configurator.CreateTerminal("\"[^\"]+\"");

            var obj = configurator.CreateNonTerminal();
            var optionalObjectList = configurator.CreateNonTerminal();
            var objectList = configurator.CreateNonTerminal();
            var optionalAttributeList = configurator.CreateNonTerminal();
            var attributeList = configurator.CreateNonTerminal();
            var attribute = configurator.CreateNonTerminal();

            obj.AddProduction(name, "{", optionalObjectList, optionalAttributeList, "}");

            optionalObjectList.AddProduction(objectList);
            optionalObjectList.AddProduction();

            objectList.AddProduction(objectList, obj);
            objectList.AddProduction(obj);

            optionalAttributeList.AddProduction(attributeList);
            optionalAttributeList.AddProduction();

            attributeList.AddProduction(attributeList, attribute);
            attributeList.AddProduction(attribute);

            attribute.AddProduction("[", name, "=", quotedString, "]");

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
