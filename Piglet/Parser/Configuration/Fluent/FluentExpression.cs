using System;
using System.Globalization;

namespace Piglet.Parser.Configuration.Fluent
{
    internal class FluentExpression : IExpressionConfigurator, IExpressionReturnConfigurator
    {
        private readonly ParserConfigurator<object> configurator;
        private Terminal<object> terminal;
        private string regex;
        private Func<string, object> func;

        public FluentExpression(ParserConfigurator<object> configurator)
        {
            this.configurator = configurator;
        }

        public Terminal<object> Terminal
        {
            get { return terminal ?? (terminal = (Terminal<object>) configurator.Terminal(regex, func)); }
        }

        public IExpressionReturnConfigurator ThatMatches<TExpressionType>()
        {
            var type = typeof (TExpressionType);
            if (type == typeof(int))
            {
                func = f => int.Parse(f);
                return ThatMatches(@"\d+");
            }
            if (type == typeof(double))
            {
                func = f => double.Parse(f, CultureInfo.InvariantCulture);
                return ThatMatches(@"\d+(\.\d+)?");
            }
            if (type == typeof(bool))
            {
                func = f => bool.Parse(f);
                return ThatMatches(@"((true)|(false))");
            }
            throw new ParserConfigurationException("Unknown type passed to ThatMatches.");
        }

        public IExpressionReturnConfigurator ThatMatches(string regex)
        {
            this.regex = regex;
            return this;
        }

        public void AndReturns(Func<string, object> func)
        {
            this.func = func;
        }
    }
}