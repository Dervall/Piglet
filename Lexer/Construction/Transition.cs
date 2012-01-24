using System.Collections.Generic;
using System.Linq;

namespace Piglet.Lexer.Construction
{
    public class Transition<TState>
    {
        public TState From { get; set; }
        public TState To { get; set; }

        // An empty set of valid input means that this is an Epsilon transition. Epsilon transitions
        // are only valid in NFAs
        public ISet<char> ValidInput { get; private set; }
        
        public Transition(TState from, TState to, IEnumerable<char> validInput = null)
        {
            ValidInput = new HashSet<char>();
            From = from;
            To = to;
            if (validInput != null)
            {
                foreach (var c in validInput)
                {
                    ValidInput.Add(c);
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0} ={1}=> {2}", From, !ValidInput.Any() ? "ε" : string.Join( ", ", ValidInput), To);
        }
    }
}
