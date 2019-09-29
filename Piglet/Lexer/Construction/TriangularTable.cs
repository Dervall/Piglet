using System;
using System.Text;

namespace Piglet.Lexer.Construction
{
    internal class TriangularTable<TIndexType, TObjectType>
    {
        // Space-inefficient implementation, we never use half the table.
        // Figure speed is more important than space these days
        private readonly TIndexType[,] table;
        private readonly Func<TObjectType, int> objIndexFunc;
        private readonly int tableSize;

        public TriangularTable(int tableSize, Func<TObjectType, int> objIndexFunc)
        {
            this.tableSize = tableSize;
            table = new TIndexType[tableSize,tableSize];
            this.objIndexFunc = objIndexFunc;
        }

        public void Fill(TIndexType value)
        {
            for (int i = 0; i < tableSize; ++i)
                for (int j = 0; j < tableSize; ++j )
                    table[i, j] = value;
        }

        public TIndexType this[TObjectType a, TObjectType b]
        {
            get
            {
                int ia = objIndexFunc(a);
                int ib = objIndexFunc(b);

                // ia must contain the larger of the two
                if (ia < ib)
                {
                    int t = ia;
                    ia = ib;
                    ib = t;
                }
                return table[ia, ib];
            }

            set
            {
                int ia = objIndexFunc(a);
                int ib = objIndexFunc(b);

                // ia must contain the larger of the two
                if (ia < ib)
                {
                    int t = ia;
                    ia = ib;
                    ib = t;
                }
                table[ia, ib] = value;
            }
        }
    }
}