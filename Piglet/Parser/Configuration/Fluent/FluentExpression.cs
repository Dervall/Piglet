using System;
using System.Globalization;

namespace Piglet.Parser.Configuration.Fluent
{
    internal class FluentExpression : IExpressionConfigurator, IExpressionReturnConfigurator
    {
        private readonly ParserConfigurator<object> configurator;
        private Terminal<object> terminal;
        private string regex;

        public FluentExpression(ParserConfigurator<object> configurator)
        {
            this.configurator = configurator;
        }

        public Terminal<object> Terminal
        {
            get
            {
                if (terminal == null)
                {
                    throw new ParserConfigurationException("An expression must be fully configured before use!");
                }
                return terminal;
            }
        }

        public void ThatMatches<TExpressionType>()
        {
            var type = typeof (TExpressionType);
            if (type == typeof(int))
            {
                ThatMatches(@"\d+").AndReturns(f => int.Parse(f));
            }
            else if (type == typeof(double))
            {
                ThatMatches(@"\d+(\.\d+)?").AndReturns(f => double.Parse(f, CultureInfo.InvariantCulture));
            }
            else if (type == typeof(float))
            {
                ThatMatches(@"\d+(\.\d+)?").AndReturns(f => float.Parse(f));
            }
            else if (type == typeof(bool))
            {
                ThatMatches(@"((true)|(false))").AndReturns(f => bool.Parse(f));
            }
            else
            {
                throw new ParserConfigurationException("Unknown type passed to ThatMatches.");
            }
        }

        public IExpressionReturnConfigurator ThatMatches(string regex)
        {
            this.regex = regex;
            return this;
        }

        public void AndReturns(Func<string, object> func)
        {
            // Create the terminal now to ensure that the tokens will be created in the right order
            terminal = (Terminal<object>) configurator.CreateTerminal(regex, func);
        }
    }
}