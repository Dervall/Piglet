Piglet, the little friendly parser and lexer tool
=================================================

Piglet is a library for lexing and parsing text, in the spirit of those big parser and lexer genererators such as bison, antlr and flex. While not as feature packed as those, it is also a whole lot leaner and much easier to understand.

Mission statement
-----------------

* To broaden the use of real parsing and lexer to extend to more than compiler construction
* To be a dependency free library that is embeddable in your code without requiring a compile-time step
* To be easy to use
* To be a source of understanding of the underlying algorithms by using understandable code

How to use
==========

Piglet is composed of two parts, a lexer and a parser. 

Lexer
-----

A lexer is a tool for identifying tokens in a much more flexible way than parsing it yourself. An example:

```csharp
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
lexer.SetSource(input);
for (var token = lexer.Next(); token.Item1 != -1; token = lexer.Next())
{
    if (token.Item2 is int)
    {
        Console.WriteLine("Lexer found an integer {0}", token.Item2);
    }
    else
    {
        Console.WriteLine("Lexer found a string {0}", token.Item2);
    }
}
```

As you can see, the lexer reacts whenever a pattern is matched by executing the proper function and returning the result. The tokens defined are matched greedily (it tries to find the largest match possible) and if multiple tokens match it will match the token that is defined first. Another use of the lexer is to run actions to react on words. Like this:

```csharp
int positionX = 0;
int positionY = 0;

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
```

Parser
------

Parsing is inheritly a more complex subject, and Piglet tries it's best to make it as accessible as possible. The classic example is the calculator. Here it is, implemented in Piglet

```csharp
var parser = ParserFactory.Configure<int>( configurator =>
{
    ITerminal<int> number = configurator.Terminal("\\d+", int.Parse);

    INonTerminal<int> expr = configurator.NonTerminal();
    INonTerminal<int> term = configurator.NonTerminal();
    INonTerminal<int> factor = configurator.NonTerminal();

    expr.Productions(p =>
    {
        p.Production(expr, "+", term).OnReduce(s => s[0] + s[2]);
        p.Production(expr, "-", term).OnReduce(s => s[0] - s[2]);
        p.Production(term).OnReduce(s => s[0]);
    });

    term.Productions(p =>
    {
        p.Production(term, "*", factor).OnReduce(s => s[0] * s[2]);
        p.Production(term, "/", factor).OnReduce(s => s[0] / s[2]);
        p.Production(factor).OnReduce(s => s[0]);
    });

    factor.Productions(p =>
    {
        p.Production(number).OnReduce(s => s[0]);
        p.Production("(", expr, ")").OnReduce(s => s[1]);
    });
});

int result = parser.Parse(new StringReader("7+8*2-2+2"));

Assert.AreEqual(23, result);
```

More samples and documentation
------------------------------

Piglet is quite extensively covered by integration type tests, that provides many sample uses of both the parser and the lexer. There is also the wiki here on github which I hope will get filled out as this library matures. There is also a Demo project that comes with the Solution, which has a few interesting sample uses of both the lexer and parser components.

Releases
--------

Piglet is quite early on in it's development, the main functionality is all there but who know how many bugs there are to fix. Consider the current version an alpha version. Once Piglet gets to a more stable state, expect this space to fill out :)

Contributing
------------

Contributors are welcome at any skill level! Forking the repo is probably the easiest way to get started. There is a nice list of issues, both bugs and features that is up for grabs. Or devise a feature of your own.

Bug tracker
-----------

Please create an issue here on GitHub!

https://github.com/Dervall/Piglet/issues

Authors
-------

**Per Dervall**
+ http://twitter.com/dervall
+ http://binarysculpting.com

Copyright and license
---------------------

Piglet is licenced under the MIT license. Refer to LICENSE.txt for more information.