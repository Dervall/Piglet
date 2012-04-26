using System;
using System.IO;
using System.Text;

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

        protected override int GetNextState(char c)
        {
            return transitionTable[State, c];
        }

        protected override Tuple<int, Func<string, T>> GetAction()
        {
            return transitionTable.GetAction(State);
        }

        protected override void ResetState()
        {
            State = 0;
        }
    }
}