namespace Piglet.Parser.Construction
{
    public interface ITable2D
    {
        int this[int stateNumber, int tokenNumber] { get; set; }
    }
}