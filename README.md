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
void StringIntoNumbersAndWords(string source)
{
    LexerFactory(...) {
	}
	
	
}
```

As you can see, the lexer reacts whenever a pattern is matched by executing the proper function and returning the result. The tokens defined are matched greedily (it tries to find the largest match possible) and if multiple tokens match it will match the token that is defined first.

Parser
------

Parsing is inheritly a more complex subject, and Piglet tries it's best to make it as accessible as possible.

```csharp

```

More samples and documentation
------------------------------

Piglet is quite extensively covered by integration type tests, that provides many sample uses of both the parser and the lexer. There is also the wiki here on github which I hope will get filled out as this library matures.

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