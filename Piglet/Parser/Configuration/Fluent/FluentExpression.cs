using System;

namespace Piglet.Parser.Configuration.Fluent
{
    internal class FluentExpression<T> : IExpressionConfigurator<T>
    {
        private readonly ParserConfigurator<T> configurator;
        private string regex;
        private Terminal<T> terminal; 

        public FluentExpression(ParserConfigurator<T> configurator)
        {
            this.configurator = configurator;
        }

        public Terminal<T> Terminal
        {
            get { return terminal; }
        }

        public IExpressionConfigurator<T> ThatMatches<TExpressionType>()
        {
            var type = typeof (TExpressionType);
            if (type == typeof(int))
            {
                return ThatMatches(@"\d+");
            }
            if (type == typeof(double))
            {
                return ThatMatches(@"\d+(\.\d+)?");
            }
            throw new ParserConfigurationException("Unknown type passed to ThatMatches.");
        }

        public IExpressionConfigurator<T> ThatMatches(string regex)
        {
            terminal = (Terminal<T>) configurator.Terminal(regex);
            return this;
        }
    }
}