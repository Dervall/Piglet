using System;

using Piglet.Parser.Configuration;
using Piglet.Lexer.Runtime;

namespace Piglet.Parser.Construction
{
    internal sealed class ReductionRule<T>
        : IReductionRule<T>
    {
        public int NumTokensToPop { get; }
        public int TokenToPush { get; }
        public INonTerminal<T> ReductionSymbol { get; }
        public Func<ParseException, LexedToken<T>[], T> OnReduce { get; }


        public ReductionRule(INonTerminal<T> sym, int pop, int push, Func<ParseException, LexedToken<T>[], T> func)
        {
            ReductionSymbol = sym;
            NumTokensToPop = pop;
            TokenToPush = push;
            OnReduce = func;
        }
    }
}