namespace Piglet.Parser.Construction
{
    public interface ISparseParseTable
    {
        int this[int stateNumber, int tokenNumber] { get; set; }
    }
}