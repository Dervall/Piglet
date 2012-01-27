using System.Collections.Generic;
using Piglet.Construction;

namespace Piglet.Configuration
{
    public interface IParserConfiguration<T>
    {
        IProductionRule<T> Start { get; }
        IEnumerable<IProductionRule<T>> ProductionRules { get; }
    }
}
