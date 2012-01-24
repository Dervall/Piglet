namespace Piglet.Lexer.Construction
{
    public class Transition<TState>
    {
        public Transition(TState from, char onCharacter, TState to)
        {
            From = from;
            ValidInput = onCharacter;
            To = to;
        }

        public TState From { get; set; }
        public TState To { get; set; }
        public char ValidInput { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ={1}=> {2}", From, ValidInput == '\0' ? 'ε' : ValidInput, To);
        }
    }
}
