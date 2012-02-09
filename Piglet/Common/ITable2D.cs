namespace Piglet.Common
{
    internal interface ITable2D
    {
        int this[int state, int input] { get; set; }
    }
}