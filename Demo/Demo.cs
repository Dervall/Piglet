using System;
using Piglet.Demo.Lexer;
using Piglet.Demo.Parser;

namespace Piglet.Demo
{
    public class Demo
    {
        public static void Main(string[] args)
        {
            // Simple demo runner
            WordsAndNumbers.Run();
            Movement.Run();
            JsonParser.Run();
            BlogFormatParser.RunFluent();
         //   Console.ReadKey();
        }
    }
}
