namespace Piglet.Parser.Construction
{
    internal interface ITable2D
    {
        int this[int stateNumber, int tokenNumber] { get; set; }
    }
}