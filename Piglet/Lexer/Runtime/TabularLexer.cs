using System;

namespace Piglet.Lexer.Runtime
{
    internal class TabularLexer<T> : LexerBase<T, int>
    {
        private readonly TransitionTable<T> transitionTable;
        
        public TabularLexer(TransitionTable<T> transitionTable, int endOfInputTokenNumber)
            :   base(endOfInputTokenNumber)
        {
            
            this.transitionTable = transitionTable;
        }

        protected override bool ReachedTermination(int nextState)
        {
            return nextState == -1;
        }

        protected override int GetNextState(int state, char c)
        {
            return transitionTable[state, c];
        }

        protected override Tuple<int, Func<string, T>> GetAction(int state)
        {
            return transitionTable.GetAction(state);
        }

        protected override int GetInitialState()
        {
            return 0;
        }
    }
}