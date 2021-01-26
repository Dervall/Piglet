using System;
using Piglet.Lexer;
using Piglet.Lexer.Runtime;

namespace Piglet.Demo.Lexer
{
    public static class WordsAndNumbers
    {
        public static void Run()
        {
            // Create a lexer returning type object
            var lexer = LexerFactory<object>.Configure(configurator =>
                                               {
                                                   // Returns an integer for each number it finds
                                                   configurator.Token(@"\d+", f => int.Parse(f));

                                                   // Returns a string for each string found
                                                   configurator.Token(@"[a-zA-Z]+", f => f);

                                                   // Ignores all white space
                                                   configurator.Ignore(@"\s+");
                                               });

            // Run the lexer
            string input = "10 piglets 5 boars 1 big sow";

            foreach ((int number, LexedToken<object> token) in lexer.Tokenize(input))
                Console.WriteLine($"Lexer found {(token.SymbolValue is int ? "an integer" : "a string")} {token.SymbolValue}");
        }
    }
}
