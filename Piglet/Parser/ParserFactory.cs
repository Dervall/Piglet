using System;
using Piglet.Parser.Configuration;
using Piglet.Parser.Configuration.Fluent;

namespace Piglet.Parser
{
    /// <summary>
    /// The parserfactory is the main way of obtaining parsers from Piglet.
    /// </summary>
    public static class ParserFactory
    {
        /// <summary>
        /// Create a code based configurator
        /// </summary>
        /// <typeparam name="T">Semantic value type of tokens</typeparam>
        /// <returns>A parser, ready for use</returns>
        public static IParserConfigurator<T> Configure<T>()
        {
            var parserConfigurator = new ParserConfigurator<T>();
            return parserConfigurator;
        }

        public static IFluentParserConfigurator Fluent()
        {
            return new FluentParserConfigurator(new ParserConfigurator<object>());
        }
    }
}