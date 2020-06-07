[![Build status](https://ci.appveyor.com/api/projects/status/7k5vohj4lhmdhac1?svg=true)](https://ci.appveyor.com/project/Unknown6656/piglet)
=================================================

Piglet is a library for lexing and parsing text, in the spirit of those big parser and lexer genererators such as bison, antlr and flex. While not as feature packed as those, it is also a whole lot leaner and much easier to understand.

Mission statement
-----------------

* To broaden the use of real parsing and lexer to extend to more than compiler construction
* To be a dependency free library that is embeddable in your code without requiring a compile-time step
* To be easy to use
* To be a source of understanding of the underlying algorithms by using understandable code

Why use piglet
==============

Piglets mission in life is to fill the void where regular expressions aren't enough and a full-blown parser generator is way too much work to integrate into your project.
A typical example is when you have hierarchical data, something which regular expressions cannot parse. Don't revert to hand writing parsers when Piglet can help you out!

How to use
==========

Piglet is composed of two parts, a lexer and a parser. 

Parser
------

Parsing is inheritly a complex subject, and Piglet tries it's best to make it as accessible as possible by using a fluent format that actually tells you what is going to happen. 
You declare rules, that themselves may contain other rules. The first rule that you define is what the entire thing must be reduced to using the other rules.

The classic example is the calculator. Here it is, implemented in Piglet:

```csharp
var config = ParserFactory.Fluent();
var expr = config.Rule();
var term = config.Rule();
var factor = config.Rule();
var plusOrMinus = config.Rule();
var mulOrDiv = config.Rule();

plusOrMinus.IsMadeUp.By("+").WhenFound(f => '+')
                    .Or.By("-").WhenFound(f => '-');

expr.IsMadeUp.By(expr).As("Left").Followed.By(plusOrMinus).As("Operator").Followed.By(term).As("Right")
    .WhenFound(f => f.Operator == '+' ? f.Left + f.Right : f.Left - f.Right)
    .Or.By(term);

mulOrDiv.IsMadeUp.By("*").WhenFound(f => '*')
                .Or.By("/").WhenFound(f => '/');

term.IsMadeUp.By(term).As("Left").Followed.By(mulOrDiv).As("Operator").Followed.By(factor).As("Right")
    .WhenFound(f => f.Operator == '*' ? f.Left * f.Right : f.Left / f.Right)
    .Or.By(factor);

factor.IsMadeUp.By<int>()
    .Or.By("(").Followed.By(expr).As("Expression").Followed.By(")")
    .WhenFound(f => f.Expression);

var parser = config.CreateParser();

int result = (int)parser.Parse("7+8*2-2+2");

Assert.AreEqual(23, result);
```

If the fluent style configuration is too verbose for you, there is also the option of doing the configuration using a more "technical" API. This configuration is exactly the same as the configuration above. This is more closely associated with the BNF form, that may be familiar to you if you have previous experience with parser generators.

```csharp
var configurator = ParserFactory.Configure<int>();

ITerminal<int> number = configurator.CreateTerminal("\\d+", int.Parse);

INonTerminal<int> expr = configurator.CreateNonTerminal();
INonTerminal<int> term = configurator.CreateNonTerminal();
INonTerminal<int> factor = configurator.CreateNonTerminal();

expr.AddProduction(expr, "+", term).SetReduceFunction(s => s[0] + s[2]);
expr.AddProduction(expr, "-", term).SetReduceFunction(s => s[0] - s[2]);
expr.AddProduction(term).SetReduceFunction(s => s[0]);

term.AddProduction(term, "*", factor).SetReduceFunction(s => s[0] * s[2]);
term.AddProduction(term, "/", factor).SetReduceFunction(s => s[0] / s[2]);
term.AddProduction(factor).SetReduceFunction(s => s[0]);

factor.AddProduction(number).SetReduceFunction(s => s[0]);
factor.AddProduction("(", expr, ")").SetReduceFunction(s => s[1]);

var parser = configurator.CreateParser();
int result = parser.Parse(new StringReader("7+8*2-2+2"));

Assert.AreEqual(23, result);
```

Lexer
-----

Sometimes you don't need a full parser, but only a tool to identify tokens. This the sort of work that you typically do using a series of regular expressions or perhaps a lot of tryParse. A lexer is a tool for identifying tokens in a much more flexible way than doing it yourself. It is also more efficient. An example:

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
foreach (var token in lexer.Tokenize(input))
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

foreach (var token in lexer.Tokenize("up down left right right north west left north up"))
{
    Console.WriteLine("{0} Current position is {1},{2}", token.Item2, positionX, positionY);
}
```

More samples and documentation
------------------------------

Piglet is quite extensively covered by integration type tests, that provides many sample uses of both the parser and the lexer. There is also the wiki here on github which I hope will get filled out as this library matures. There is also a Demo project that comes with the Solution, which has a few interesting sample uses of both the lexer and parser components.

Releases
--------

Releases are numbered in major, minor and revision number. 

* Major number are updated on major changes which are not backwards compatible. 
* Minor versions add features and might in some cases distrupt some backwards compatibility
* Revisions are fully-backwards compatible fixes on a previous versions.

All releases are available from both NuGet, and are always represented as tags on github.

Apart from compiling the source yourself, the easiest way to get your hands on the library is to use NuGet. Just search for Piglet, and you shall be rewarded.

# 1.4.0
* Added thread safety to lexing and parsing.
* Improved lexer usage. Tokenize is now new preferred method of listing tokens, which is also thread safe.
* Made \w and \d more consistent with MS use of the term.
* Added icon to NuGet package
* Added convenience reduction functions for common cases of reducing to a single member in tech configuration
* Fixed some left over console output in error recovery

# 1.3.0
* Piglet now supports Unicode! Piglet will now lex the full unicode character set.
* You can now specify the lexer runtime, giving you more options on the speed tradeoffs of lexer construction and lexer runtime

# 1.2.2
* Added support for ignoring expressions in fluent configuration parsing.

# 1.2.1
* Added support for escaped characters inside character classes.

# 1.2.0 
* Added error recovery and reporting to fluent parser configuration
* Added token precedence to fluent configuration
* Completed XML documentation to include every method
* Fixed bug with escaped ?
* Fixed bug with numbered repetition
* Fixed bug with possible wrong order of defined expressions for fluent configuration
* Automated the NuGet package management

# 1.1.0 
* Added DFA minimization to the lexer generation algorithm. 
* Added public dotnotation functionality for getting debug graphs for lexers.
* Unit test framework changed to NUnit.

# 1.0.1 
* Added missing Piglet.XML file to the NuGet package. Documentation should now be available in intellisense.

# 1.0.0 
*First NuGet release

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
+ http://twitter.com/perdervall
+ http://binarysculpting.com

Copyright and license
---------------------

Piglet is licenced under the MIT license. Refer to LICENSE.txt for more information.