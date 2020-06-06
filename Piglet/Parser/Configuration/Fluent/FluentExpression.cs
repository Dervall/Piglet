using System.Globalization;
using System;

namespace Piglet.Parser.Configuration.Fluent
{
    internal sealed class FluentExpression
        : IExpressionConfigurator
        , IExpressionReturnConfigurator
    {
        private readonly ParserConfigurator<object> _configurator;
        private Terminal<object> _terminal;
        private string _regex;


        public FluentExpression(ParserConfigurator<object> configurator) => _configurator = configurator;

        public FluentExpression(ITerminal<object> terminal) => _terminal = (Terminal<object>)terminal;

        public Terminal<object> Terminal => _terminal ?? throw new ParserConfigurationException("An expression must be fully configured before use!");

        public void ThatMatches<TExpressionType>()
        {
            Type type = typeof(TExpressionType);

            if (type == typeof(int))
                ThatMatches(@"\d+").AndReturns(f => int.Parse(f));
            else if (type == typeof(double))
                ThatMatches(@"\d+(\.\d+)?").AndReturns(f => double.Parse(f, CultureInfo.InvariantCulture));
            else if (type == typeof(float))
                ThatMatches(@"\d+(\.\d+)?").AndReturns(f => float.Parse(f));
            else if (type == typeof(bool))
                ThatMatches(@"((true)|(false))").AndReturns(f => bool.Parse(f));
            else
                throw new ParserConfigurationException("Unknown type passed to ThatMatches.");
        }

        public IExpressionReturnConfigurator ThatMatches(string regex)
        {
            _regex = regex;

            return this;
        }

        // Create the terminal now to ensure that the tokens will be created in the right order
        public void AndReturns(Func<string, object> func) => _terminal = (Terminal<object>)_configurator.CreateTerminal(_regex, func);
    }
}