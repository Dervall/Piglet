using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Piglet.Lexer;
using Piglet.Parser.Construction;

namespace Piglet.Parser
{
    internal class LRParser<T> : IParser<T>
    {
        private readonly IParseTable<T> parseTable;
        private readonly Stack<T> valueStack;
        private readonly Stack<int> parseStack;

        private readonly int errorTokenNumber;
        private readonly int endOfInputTokenNumber;
        private readonly string[] terminalDebugNames;

        internal LRParser(IParseTable<T> parseTable, int errorTokenNumber, int endOfInputTokenNumber, string[] terminalDebugNames)
        {
            this.parseTable = parseTable;
            
            valueStack = new Stack<T>();
            parseStack = new Stack<int>();

            this.errorTokenNumber = errorTokenNumber;
            this.endOfInputTokenNumber = endOfInputTokenNumber;
            this.terminalDebugNames = terminalDebugNames;
        }

        /// <summary>
        /// This is accessible for test and debug reasons
        /// </summary>
        internal IParseTable<T> Table { get { return parseTable; } }

        public ILexer<T> Lexer { get; set; }

        private T Parse()
        {
            // If this parser has been used before, clear the stacks
            valueStack.Clear();
            parseStack.Clear();

            // Push default state onto the parse stack. Default state is always 0
            parseStack.Push(0);

            var input = Lexer.Next();

            // This holds the last exception we found when parsing, since we
            // will need to pass this to an error handler once the proper handler has been found
            ParseException exception = null;

            while (true)
            {
                int state = parseStack.Peek();
                int action = parseTable.Action[state, input.Item1];
                if (action >= 0)
                {
                    if (action == short.MaxValue)
                    {
                        // Accept!
                        return valueStack.Pop();
                    }

                    // Shift
                    parseStack.Push(input.Item1);   // Push token unto stack
                    parseStack.Push(action);        // Push state unto stack

                    // Shift token value unto value stack
                    valueStack.Push(input.Item2);

                    // Lex next token
                    input = Lexer.Next();
                }
                else
                {
                    if (action == short.MinValue)
                    {
                        // Get the expected tokens
                        string[] expectedTokens = GetExpectedTokenNames(state).ToArray();
                        
                        // Create an exception that either might be thrown or may be handed to the error handling routine.
                        exception = new ParseException(string.Format("Illegal token {0}. Expected {1}", 
                            terminalDebugNames[input.Item1], string.Join(",", expectedTokens)))
                                        {
                                            LexerState = Lexer.LexerState,
                                            FoundToken = terminalDebugNames[input.Item1],
                                            ExpectedTokens = expectedTokens,
                                            FoundTokenId = input.Item1,
                                            ParserState = state
                                        };

                        // Go for error recovery!
                        while (parseTable.Action[parseStack.Peek(), errorTokenNumber] == short.MinValue)
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
                        parseStack.Push(errorTokenNumber);
                        parseStack.Push(parseTable.Action[state, errorTokenNumber]);
                        valueStack.Push(default(T));
                        state = parseStack.Peek();

                        // We have now found a state where error recovery is enabled. This means that we 
                        // continue to scan the input stream looking for something which is accepted.
                        // End of input will cause the exception to be thrown
                        for (; parseTable.Action[state, input.Item1] == short.MinValue && 
                               input.Item1 != endOfInputTokenNumber; input = Lexer.Next())
                            Console.WriteLine("Ate '{0}'", Lexer.LexerState.LastLexeme);// nom nom nom

                        // Ran out of file looking for the end of the error rule
                        if (input.Item1 == endOfInputTokenNumber)
                            throw exception;
                        
                        // If we get here we are pretty cool, continue running the parser. The actual error recovery routine will be
                        // called as soon as the error rule itself is reduced.
                    }
                    else
                    {
                        // Get the right reduction rule to apply
                        ReductionRule<T> reductionRule = parseTable.ReductionRules[-(action + 1)];
                        for (int i = 0; i < reductionRule.NumTokensToPop*2; ++i)
                        {
                            parseStack.Pop();
                        }

                        // Transfer to state found in goto table
                        int stateOnTopOfStack = parseStack.Peek();
                        parseStack.Push(reductionRule.TokenToPush);
                        parseStack.Push(parseTable.Goto[stateOnTopOfStack, reductionRule.TokenToPush]);

                        // Get tokens off the value stack for the OnReduce function to run on
                        var onReduceParams = new T[reductionRule.NumTokensToPop];

                        // Need to do it in reverse since thats how the stack is organized
                        for (int i = reductionRule.NumTokensToPop - 1; i >= 0; --i)
                        {
                            onReduceParams[i] = valueStack.Pop();
                        }

                        // This calls the reduction function with the possible exception set. The exception could be cleared here, but
                        // there is no real reason to do so, since all the normal rules will ignore it, and all the error rules are guaranteed
                        // to have the exception set prior to entering the reduction function.
                        var reduceFunc = reductionRule.OnReduce;
                        valueStack.Push(reduceFunc == null ? default(T) : reduceFunc(exception, onReduceParams));
                    }
                }
            }
        }

        private IEnumerable<string> GetExpectedTokenNames(int state)
        {
            return terminalDebugNames.Where((t, i) => parseTable.Action[state, i] != short.MinValue);
        }

        public T Parse(string input)
        {
            Lexer.SetSource(input);
            return Parse();
        }

        public T Parse(TextReader input)
        {
            Lexer.SetSource(input);
            return Parse();
        }
    }
}
