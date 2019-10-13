using System;

namespace Piglet.Lexer.Construction
{
    internal sealed class TriangularTable<TIndexType, TObjectType>
    {
        // Space-inefficient implementation, we never use half the table.
        // Figure speed is more important than space these days
        private readonly TIndexType[,] _table;
        private readonly Func<TObjectType, int> _objIndexFunc;
        private readonly int _tableSize;


        public TriangularTable(int tableSize, Func<TObjectType, int> objIndexFunc)
        {
            _tableSize = tableSize;
            _table = new TIndexType[tableSize,tableSize];
            _objIndexFunc = objIndexFunc;
        }

        public void Fill(TIndexType value)
        {
            for (int i = 0; i < _tableSize; ++i)
                for (int j = 0; j < _tableSize; ++j )
                    _table[i, j] = value;
        }

        public TIndexType this[TObjectType a, TObjectType b]
        {
            get
            {
                int ia = _objIndexFunc(a);
                int ib = _objIndexFunc(b);

                // ia must contain the larger of the two
                if (ia < ib)
                {
                    int t = ia;

                    ia = ib;
                    ib = t;
                }

                return _table[ia, ib];
            }
            set
            {
                int ia = _objIndexFunc(a);
                int ib = _objIndexFunc(b);

                // ia must contain the larger of the two
                if (ia < ib)
                {
                    int t = ia;
                    ia = ib;
                    ib = t;
                }

                _table[ia, ib] = value;
            }
        }
    }
}