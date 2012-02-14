using Microsoft.VisualStudio.TestTools.UnitTesting;
using Piglet.Common;

namespace Piglet.Tests.Common
{
    [TestClass]
    public class TestParseTables
    {
        [TestMethod]
        public void TestCompressedActionTable()
        {
            var uncompressedTable = new short[,]
                                             {
                                                 {7, 8, 9, 6, 5, 4},
                                                 {1, 1, 1, 1, 1, 1},
                                                 {2, 2, 2, 2, 2, 2},
                                                 {2, 2, 3, 3, 3, 3},
                                                 {1, 1, 1, 2, 2, 2}
                                             };
            var compressed = new CompressedTable(uncompressedTable);

            for (int x = 0; x < 5; ++x)
            {
                for (int y = 0; y < 6; ++y)
                {
                    Assert.AreEqual(uncompressedTable[x, y], compressed[x, y], "Mismatch at " + x + " " + y);
                }
            }

        }
    }
}
