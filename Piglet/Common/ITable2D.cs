namespace Piglet.Common
{
    public interface ITable2D
    {
        int this[int state, int input] { get; }
    }
}