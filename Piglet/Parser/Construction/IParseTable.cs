using Piglet.Common;

namespace Piglet.Parser.Construction
{
    public interface IParseTable<T>
    {
        ITable2D Action { get; }
        ITable2D Goto { get; }
        IReductionRule<T>[] ReductionRules { get; set; }
        int StateCount { get; }
    }
}