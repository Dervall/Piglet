using System;
using Piglet.Lexer;
using Piglet.Lexer.Runtime;

namespace Piglet.Demo.Lexer
{
    public class Movement
    {
        public static void Run()
        {
            int positionX = 0;
            int positionY = 0;

            var ticks = System.DateTime.Now.Ticks;

            var lexer = LexerFactory<string>.Configure(configurator =>
            {
                configurator.Token(@"(up|north)", s =>
                {
                    positionY--;
                    return "Moved north";
                });
                configurator.Token(@"(down|south)", s =>
                {
                    positionY++;
                    return "Moved south";
                });
                configurator.Token(@"(right|east)", s =>
                {
                    positionX++;
                    return "Moved east";
                });
                configurator.Token(@"(left|west)", s =>
                {
                    positionX--;
                    return "Moved west";
                });
                configurator.Ignore(@"\s+");
            });

            foreach ((int number, LexedToken<string> token) in lexer.Tokenize("up down left right right north west left north up"))
            {
                Console.WriteLine("{0} Current position is {1},{2}", token.SymbolValue, positionX, positionY);
            }

            Console.WriteLine(System.DateTime.Now.Ticks - ticks);
        }
    }
}
