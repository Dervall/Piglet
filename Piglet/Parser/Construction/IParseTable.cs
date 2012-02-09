using Piglet.Common;

namespace Piglet.Parser.Construction
{
    internal interface IParseTable<T>
    {
        ITable2D Action { get; }
        ITable2D Goto { get; }
        ReductionRule<T>[] ReductionRules { get; set; }
    }
}