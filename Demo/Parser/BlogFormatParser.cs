using Piglet.Parser;

namespace Piglet.Demo.Parser
{
    /// <summary>
    /// This code written in support for blog article on parsing
    /// </summary>
    public class BlogFormatParser
    {
        public static void Run()
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
