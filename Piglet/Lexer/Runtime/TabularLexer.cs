using System;

namespace Piglet.Lexer.Runtime
{
    internal class TabularLexer<TContext, T> : LexerBase<TContext, T, int>
    {
        private readonly TransitionTable<TContext, T> transitionTable;
        
        public TabularLexer(TransitionTable<TContext, T> transitionTable, int endOfInputTokenNumber)
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

        protected override Tuple<int, Func<TContext, string, T>> GetAction(int state)
        {
            return transitionTable.GetAction(state);
        }

        protected override int GetInitialState()
        {
            return 0;
        }
    }
}