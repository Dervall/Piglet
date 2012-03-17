namespace Piglet.Common
{
    /// <summary>
    /// This abstracts a table with two dimenstions
    /// </summary>
    public interface ITable2D
    {
        /// <summary>
        /// Gets an entry from the table.
        /// </summary>
        /// <param name="state">State to get for</param>
        /// <param name="input">Input to get for</param>
        /// <returns>Table value</returns>
        int this[int state, int input] { get; }
    }
}