namespace Piglet.Parser.Configuration.Fluent
{
    internal class FluentExpression : IExpressionConfigurator
    {
        private readonly ParserConfigurator<object> configurator;
        private Terminal<object> terminal; 

        public FluentExpression(ParserConfigurator<object> configurator)
        {
            this.configurator = configurator;
        }

        public Terminal<object> Terminal
        {
            get { return terminal; }
        }

        public IExpressionConfigurator ThatMatches<TExpressionType>()
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

        public IExpressionConfigurator ThatMatches(string regex)
        {
            terminal = (Terminal<object>) configurator.Terminal(regex);
            return this;
        }
    }
}