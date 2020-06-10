using System.Diagnostics.CodeAnalysis;

namespace Piglet.Lexer.Construction
{
    internal class Transition<TState>
    {
        [MaybeNull]
        public TState From { get; set; }
        [MaybeNull]
        public TState To { get; set; }

        // An empty set of valid input means that this is an Epsilon transition. Epsilon transitions
        // are only valid in NFAs
        public CharSet ValidInput { get; internal set; }
        
        public Transition(TState from, TState to, CharSet? validInput = null)
        {
            ValidInput = validInput ?? new CharSet();
            From = from;
            To = to;
        }

        public override string ToString() => string.Format("{0} ={1}=> {2}", From, To, ValidInput);
    }
}
