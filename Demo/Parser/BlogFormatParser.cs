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
            var parser = ParserFactory.Configure<bool>(configurator =>
            {
                configurator.LexerSettings.Ignore = new[] {@"\s+"};

                var name = configurator.Terminal("[a-z]+");
                var quotedString = configurator.Terminal("\"[^\"]+\"");

                var obj = configurator.NonTerminal();
                var optionalObjectList = configurator.NonTerminal();
                var objectList = configurator.NonTerminal();
                var optionalAttributeList = configurator.NonTerminal();
                var attributeList = configurator.NonTerminal();
                var attribute = configurator.NonTerminal();

                obj.Productions(p => p.Production(name, "{", optionalObjectList, optionalAttributeList, "}"));
                
                optionalObjectList.Productions(p =>
                {
                    p.Production(objectList);
                    p.Production();
                });

                objectList.Productions(p =>
                {
                    p.Production(objectList, obj);
                    p.Production(obj);
                });
                
                optionalAttributeList.Productions(p =>
                {
                    p.Production(attributeList);
                    p.Production();
                });

                attributeList.Productions(p => 
                {
                    p.Production(attributeList, attribute);
                    p.Production(attribute);
                });

                attribute.Productions(p => p.Production("[", name, "=", quotedString, "]"));
            });

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
