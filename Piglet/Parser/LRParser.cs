using System.Collections.Generic;
using System.Linq;
using System;

using Piglet.Parser.Construction;
using Piglet.Lexer.Runtime;
using Piglet.Lexer;

namespace Piglet.Parser
{
    internal sealed class LRParser<T>
        : IParser<T>
    {
        private readonly int _errorTokenNumber;
        private readonly int _endOfInputTokenNumber;
        private readonly string?[] _terminalDebugNames;


        public IParseTable<T> ParseTable { get; }
        public ILexer<T>? Lexer { get; set; }


        internal LRParser(IParseTable<T> parseTable, int errorTokenNumber, int endOfInputTokenNumber, string?[] terminalDebugNames)
        {
            ParseTable = parseTable;
            _errorTokenNumber = errorTokenNumber;
            _endOfInputTokenNumber = endOfInputTokenNumber;
            _terminalDebugNames = terminalDebugNames;
        }

        private LexedToken<T> Parse(ILexerInstance<T>? lexerInstance)
        {
            if (lexerInstance is null)
                throw new ArgumentNullException(nameof(lexerInstance));

            Stack<LexedToken<T>> valueStack = new Stack<LexedToken<T>>();
            Stack<int> parseStack = new Stack<int>();

            // Push default state onto the parse stack. Default state is always 0
            parseStack.Push(0);

            (int number, LexedToken<T> token) input = lexerInstance.Next();

            // This holds the last exception we found when parsing, since we
            // will need to pass this to an error handler once the proper handler has been found
            ParseException? exception = null;

            while (true)
            {
                int state = parseStack.Peek();
                int action = ParseTable.Action?[state, input.number] ?? short.MinValue;

                if (action >= 0)
                {
                    if (action == short.MaxValue)
                        return valueStack.Pop(); // Accept!

                    // Shift
                    parseStack.Push(input.number);   // Push token unto stack
                    parseStack.Push(action);        // Push state unto stack

                    // Shift token value unto value stack
                    valueStack.Push(input.token);

                    // Lex next token
                    input = lexerInstance.Next();
                }
                else if (action == short.MinValue)
                {
                    // Get the expected tokens
                    string[] expectedTokens = GetExpectedTokenNames(state).ToArray();

                    // Create an exception that either might be thrown or may be handed to the error handling routine.
                    exception = new ParseException($"Illegal token '{_terminalDebugNames[input.number]}', expected {{'{string.Join("', '", expectedTokens)}'}} at ({lexerInstance.CurrentLineNumber}:{lexerInstance.CurrentCharacterIndex}).")
                    {
                        LexerState = lexerInstance,
                        FoundToken = _terminalDebugNames[input.number],
                        ExpectedTokens = expectedTokens,
                        FoundTokenId = input.number,
                        ParserState = state
                    };

                    // Go for error recovery!
                    while ((ParseTable.Action?[parseStack.Peek(), _errorTokenNumber] ?? short.MinValue) == short.MinValue)
                    {
                        // If we run out of stack while searching for the error handler, throw the exception
                        // This is what happens when there is no error handler defined at all.
                        if (parseStack.Count <= 2)
                            throw exception;

                        parseStack.Pop(); // Pop state
                        parseStack.Pop(); // Pop token
                        valueStack.Pop(); // Pop whatever value
                    }

                    // Shift the error token unto the stack
                    state = parseStack.Peek();

                    parseStack.Push(_errorTokenNumber);
                    parseStack.Push(ParseTable.Action?[state, _errorTokenNumber] ?? short.MinValue);
                    valueStack.Push(new LexedToken<T>(default!, lexerInstance.CurrentAbsoluteIndex, lexerInstance.CurrentLineNumber, lexerInstance.CurrentCharacterIndex, 0));

                    state = parseStack.Peek();

                    // We have now found a state where error recovery is enabled. This means that we 
                    // continue to scan the input stream looking for something which is accepted.
                    // End of input will cause the exception to be thrown
                    for (; ParseTable.Action[state, input.number] == short.MinValue && input.number != _endOfInputTokenNumber; input = lexerInstance.Next())
                        ; // nom nom nom

                    // Ran out of file looking for the end of the error rule
                    if (input.number == _endOfInputTokenNumber)
                        throw exception;
                        
                    // If we get here we are pretty cool, continue running the parser. The actual error recovery routine will be
                    // called as soon as the error rule itself is reduced.
                }
                else if (ParseTable.ReductionRules?[-(action + 1)] is IReductionRule<T> rule) // Get the right reduction rule to apply
                {
                    for (int i = 0; i < rule.NumTokensToPop * 2; ++i)
                        parseStack.Pop();

                    // Transfer to state found in goto table
                    int stateOnTopOfStack = parseStack.Peek();

                    parseStack.Push(rule.TokenToPush);
                    parseStack.Push(ParseTable.Goto?[stateOnTopOfStack, rule.TokenToPush] ?? short.MinValue);

                    // Get tokens off the value stack for the OnReduce function to run on
                    LexedToken<T>[] tokens = new LexedToken<T>[rule.NumTokensToPop];

                    // Need to do it in reverse since thats how the stack is organized
                    for (int i = rule.NumTokensToPop - 1; i >= 0; --i)
                        tokens[i] = valueStack.Pop();

                    // This calls the reduction function with the possible exception set. The exception could be cleared here, but
                    // there is no real reason to do so, since all the normal rules will ignore it, and all the error rules are guaranteed
                    // to have the exception set prior to entering the reduction function.
                    Func<ParseException?, LexedToken<T>[], T> reduceFunc = rule.OnReduce!;
                    T result = reduceFunc == null ? default : reduceFunc(exception, tokens);

                    valueStack.Push(new LexedNonTerminal<T>(result!, rule.ReductionSymbol, tokens));
                }
            }
        }

        private IEnumerable<string> GetExpectedTokenNames(int state) => _terminalDebugNames.Where((t, i) => t is { } && ParseTable.Action?[state, i] != short.MinValue).Cast<string>();

        public LexedToken<T> ParseTokens(string input) => Parse(Lexer?.Begin(input));

        public T Parse(string input) => ParseTokens(input).SymbolValue;
    }
}
