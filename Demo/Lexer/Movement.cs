using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Piglet.Lexer;

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

            lexer.SetSource("up down left right right north west left north up");


            for (var token = lexer.Next(); token.Item1 != -1; token = lexer.Next())
            {
                Console.WriteLine("{0} Current position is {1},{2}", token.Item2, positionX, positionY);
            }

            Console.WriteLine(System.DateTime.Now.Ticks - ticks);
        }
    }
}
