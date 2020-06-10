﻿using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace Piglet.GenericGenerator
{
    public static class Program
    {
        public static int MAX_SIZE = 16;


        public static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine($@"Usage:
    {new FileInfo(typeof(Program).Assembly.Location).Name} <target-file> <type> [size]

Arguments:
    target-file     The path to the target .cs or .fs file which will contain the generated classes.
    type            'cs' or 'fs'. This determines it generates the C# or F# helper classes.
    size            Number of classes/production wrappers to generate.
");

                return -1;
            }

            FileInfo target_file = new FileInfo(args[0]);
            DirectoryInfo parent = target_file.Directory;

            if (!parent.Exists)
                parent.Create();

            if (args.Length > 2 && int.TryParse(args[2], out int size))
                MAX_SIZE = size;

            if (target_file.Exists)
                target_file.Delete();

            using FileStream stream = new FileStream(target_file.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
            using StreamWriter writer = new StreamWriter(stream);

            switch (args[1].ToLowerInvariant())
            {
                case "cs":
                    CreateGenericExtensions(writer);

                    break;
                case "fs":
                    CreateFSharpExtensions(writer);

                    break;
                default:
                    Console.Error.WriteLine($"Invalid output type '{args[1]}'. Expected 'cs' or 'fs'.");

                    return -1;
            }

            writer.Flush();
            stream.Flush();

            return 0;
        }

        private static void CreateGenericExtensions(StreamWriter writer)
        {
            writer.Write($@"
///////////////////////////////////////////////////////////////////////
//             AUTOGENERATED {DateTime.Now:yyyy-MM-dd HH:mm:ss.ffffff}              //
//   All your changes to this file will be lost upon re-generation.  //
///////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System;

using Piglet.Parser.Construction;
using Piglet.Lexer.Runtime;


namespace Piglet.Parser.Configuration.Generic
{{
    /// <summary>
    /// Represents an abstract generic parser constructor.
    /// <para/>
    /// The parser based on this constructor will return a parsed value of the type <typeparamref name=""TOut""/>.
    /// </summary>
    /// <typeparam name=""TOut"">The generic value return type of the parser.</typeparam>
    public abstract class ParserConstructor<TOut>
    {{
        private ParserWrapper? _parser = null;
        private volatile int _ntcounter = 0;

        /// <summary>
        /// The parser configuration.
        /// </summary>
        public IParserConfigurator<object> Configurator {{ get; }}


        /// <summary>
        /// Creates a new generic parser constructor with the default parser configuration.
        /// </summary>
        public ParserConstructor()
            : this(ParserFactory.Configure<object>())
        {{
        }}

        /// <summary>
        /// Creates a new generic parser constructor with the given parser configuration.
        /// </summary>
        /// <param name=""configurator"">Parser configuration.</param>
        public ParserConstructor(IParserConfigurator<object> configurator) => Configurator = configurator;

        /// <summary>
        /// Creates a new non-terminal symbol with the given generic semantic value and name.
        /// </summary>
        /// <typeparam name=""T"">Generic semantic value stored inside the new non-terminal symbol.</typeparam>
        /// <param name=""name"">The name of the new non-terminal symbol.</param>
        /// <returns>The newly created non-terminal symbol.</returns>
        protected NonTerminalWrapper<T> CreateNonTerminal<T>(string name) => new NonTerminalWrapper<T>(Configurator.CreateNonTerminal(name));

        /// <summary>
        /// Creates a new non-terminal symbol with the given generic semantic value and the default name for non-terminals (""NT..."").
        /// </summary>
        /// <typeparam name=""T"">Generic semantic value stored inside the new non-terminal symbol.</typeparam>
        /// <returns>The newly created non-terminal symbol.</returns>
        protected NonTerminalWrapper<T> CreateNonTerminal<T>() => CreateNonTerminal<T>($""NT{{++_ntcounter}}"");

        /// <summary>
        /// Creates a new terminal symbol associated with the given regex string and generic value.
        /// </summary>
        /// <typeparam name=""T"">Generic semantic value stored inside the new terminal symbol.</typeparam>
        /// <param name=""regex"">The regex string associated with the terminal symbol.</param>
        /// <param name=""value"">The value stored inside the new terminal value.</param>
        /// <returns>The newly created terminal symbol.</returns>
        protected TerminalWrapper<T> CreateTerminal<T>(string regex, T value) => CreateTerminal(regex, _ => value);

        /// <summary>
        /// Creates a new terminal symbol associated with the given regex string and the function providing the generic value.
        /// </summary>
        /// <typeparam name=""T"">Generic semantic value stored inside the new terminal symbol.</typeparam>
        /// <param name=""regex"">The regex string associated with the terminal symbol.</param>
        /// <param name=""func"">The function providing the generic value represented by the terminal symbol.</param>
        /// <returns>The newly created terminal symbol.</returns>
        protected TerminalWrapper<T> CreateTerminal<T>(string regex, Func<string, T> func) => new TerminalWrapper<T>(Configurator.CreateTerminal(regex, s => func(s)));

        /// <summary>
        /// Creates a new terminal symbol associated with the given regex string and the identity function (of the type <see langword=""string""/>).
        /// </summary>
        /// <param name=""regex"">The regex string associated with the terminal symbol.</param>
        /// <returns>The newly created terminal symbol.</returns>
        protected TerminalWrapper<string> CreateTerminal(string regex) => new TerminalWrapper<string>(Configurator.CreateTerminal(regex));

        /// <summary>
        /// Sets the precedence for all given symbols in ascending order. The first symbol group is therefore considered to have the lowest precedence and the last symbol group the highest precedence.
        /// </summary>
        /// <param name=""groups"">Ordered collection of groups containing a set of symbols with their corresponding associativity.</param>
        protected void SetPrecedenceList(params (AssociativityDirection direction, ITerminalWrapper[] symbols)[] groups)
        {{
            foreach ((AssociativityDirection d, ITerminalWrapper[] s) in groups)
                SetAssociativity(d, s);
        }}

        /// <summary>
        /// Sets the given associativity to all symbols in the given symbol collection. All sybols will be considered to have the same precedence group.
        /// </summary>
        /// <param name=""dir"">Associativity direction.</param>
        /// <param name=""symbols"">Target symbols.</param>
        /// <returns>The precedence group associated with the given symbols.</returns>
        protected IPrecedenceGroup SetAssociativity(AssociativityDirection dir, params ITerminalWrapper[] symbols)
        {{
            ITerminal<object>[] arr = symbols.Select(s => s.Symbol).ToArray();

            switch (dir)
            {{
                case AssociativityDirection.Left:
                    return Configurator.LeftAssociative(arr);
                case AssociativityDirection.Right:
                    return Configurator.RightAssociative(arr);
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir));
            }}
        }}

        protected void CreateProduction<T, S0>(NonTerminalWrapper<T> to, SymbolWrapper<S0> s) => to.AddProduction(s).SetReduceToFirst();
");

            for (int i = 0; i <= MAX_SIZE; ++i)
            {
                string types = string.Concat(range(0, i).Select(x => $", S{x}"));
                string ftypes = string.Concat(range(0, i).Select(x => $"S{x}, "));
                string names = string.Join(", ", range(0, i).Select(x => $"s{x}"));
                string args = string.Concat(range(0, i).Select(x => $", SymbolWrapper<S{x}> s{x}"));

                writer.Write($@"
        protected ProductionWrapper<{ftypes}T> CreateProduction<T{types}>(NonTerminalWrapper<T> to{args}, Func<{ftypes}T> func) => to.AddProduction({names}).SetReduceFunction(func);
");
            }

            writer.Write(@"
        /// <summary>
        /// Creates a new parser based on the current configurator and returns it.
        /// <para/>
        /// <b>Note:</b> Only the first call of <see cref=""CreateParser""/> creates a new parser. If you wish to reset the generated parser and re-create it, call the method <see cref=""ReesetParser""/> beforehand.
        /// </summary>
        /// <returns>The constructed parser.</returns>
        public ParserWrapper CreateParser()
        {
            if (_parser is null)
            {
                NonTerminalWrapper<TOut> nt = CreateNonTerminal<TOut>();

                Construct(nt);

                Configurator.SetStartSymbol((INonTerminal<object>)nt.Symbol);
                _parser = new ParserWrapper(Configurator.CreateParser());
            }

            return _parser;
        }

        /// <summary>
        /// Resets the constructed parser to <see langword=""null""/>, thereby forcing the parser to be re-constructed based on the current conficuration the next time <see cref=""CreateParser""/> is called.
        /// </summary>
        public void ResetParser() => _parser = null;

        /// <summary>
        /// Constructs the parser. This method must be implemented by every constructor based on <see cref=""ParserConstructor{TOut}""/>
        /// </summary>
        /// <param name=""start_symbol"">The non-terminal production start symbol. The value of this symbol will be returned when the constructed parser is executed.</param>
        protected abstract void Construct(NonTerminalWrapper<TOut> start_symbol);


        public sealed class ParserWrapper
        {
            public IParser<object> Parser { get; }


            internal ParserWrapper(IParser<object> parser) => Parser = parser;

            public ParserResult<TOut> Parse(string s)
            {
                List<LexedToken<object>> tokens = new List<LexedToken<object>>();
                void aggregate(LexedToken<object> token)
                {
                    tokens.Add(token);

                    if (token is LexedNonTerminal<object> nt)
                        foreach (LexedToken<object> child in nt.ChildNodes)
                            aggregate(child);
                }

                aggregate(Parser.ParseTokens(s));

                return new ParserResult<TOut>((TOut)tokens[0].SymbolValue, s.Split('\n'), tokens.ToArray());
            }
        }
    }

    public sealed class ParserResult<TOut>
    {
        public TOut ParsedValue { get; }
        public string[] SourceLines { get; }
        public LexedToken<object>[] LexedTokens { get; }


        internal ParserResult(TOut parsedValue, string[] sourceLines, LexedToken<object>[] lexedTokens)
        {
            ParsedValue = parsedValue;
            SourceLines = sourceLines;
            LexedTokens = lexedTokens;
        }

        public static implicit operator TOut(ParserResult<TOut> res) => res.ParsedValue;
    }

    public interface ITerminalWrapper
    {
        ITerminal<object> Symbol { get; }
    }

    public class SymbolWrapper<T>
    {
        public ISymbol<object> Symbol { get; }
        public Type SymbolType => typeof(T);


        public SymbolWrapper(ISymbol<object> symbol) => Symbol = symbol;

        public override string ToString() => Symbol.ToString();
    }

    public sealed class TerminalWrapper<T>
        : SymbolWrapper<T>
        , ITerminalWrapper
    {
        ITerminal<object> ITerminalWrapper.Symbol => (ITerminal<object>)Symbol;

        public TerminalWrapper(ISymbol<object> symbol)
            : base(symbol)
        {
        }
    }

    public sealed class NonTerminalWrapper<T>
        : SymbolWrapper<T>
    {
        public NonTerminalWrapper(ISymbol<object> symbol)
            : base(symbol)
        {
        }

        public ProductionWrapper<T> AddProduction() => new ProductionWrapper<T>(((INonTerminal<object>)Symbol).AddProduction());
");

            for (int i = 1; i <= MAX_SIZE; ++i)
            {
                string types = string.Join(", ", range(0, i).Select(x => $"T{x}"));
                string names = string.Join(", ", range(0, i).Select(x => $"sym{x}.Symbol"));
                string args = string.Join(", ", range(0, i).Select(x => $"SymbolWrapper<T{x}> sym{x}"));

                writer.Write($@"
        public ProductionWrapper<{types}, T> AddProduction<{types}>({args}) =>
            new ProductionWrapper<{types}, T>(((INonTerminal<object>)Symbol).AddProduction({names}));
");
            }

            writer.Write($@"
    }}

    public abstract class ProductionWrapperBase<T>
        where T : ProductionWrapperBase<T>
    {{
        public IProduction<object> Production {{ get; }}


        public ProductionWrapperBase(IProduction<object> production) => Production = production;

        public void SetReduceToFirst() => Production.SetReduceToFirst();

        public void SetPrecedence(IPrecedenceGroup precedence) => Production.SetPrecedence(precedence);
    }}
");
            for (int i = 1; i <= MAX_SIZE + 1; ++i)
            {
                IEnumerable<string> types = range(0, i).Select(x => x == i - 1 ? "R" : $"T{x}");
                string typestr = string.Join(", ", types);

                writer.Write($@"

    public sealed class ProductionWrapper<{typestr}>
        : ProductionWrapperBase<ProductionWrapper<{typestr}>>
    {{
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {{
        }}

        public ProductionWrapper<{typestr}> SetReduceFunction(Func<{typestr}> f)
        {{
            Production.SetReduceFunction(args => f({string.Join(", ", types.Take(i - 1).Select((x, i) => $"({x})args[{i}]"))}));

            return this;
        }}
    }}
");
            }

            writer.WriteLine("}");
        }

        private static void CreateFSharpExtensions(StreamWriter writer)
        {
            writer.Write($@"
///////////////////////////////////////////////////////////////////////
//             AUTOGENERATED {DateTime.Now:yyyy-MM-dd HH:mm:ss.ffffff}              //
//   All your changes to this file will be lost upon re-generation.  //
///////////////////////////////////////////////////////////////////////

namespace Piglet.Parser.Configuration.FSharp

open Piglet.Parser.Configuration.Generic
open System


/// A module containing F# code extensions for the namespace 'Piglet.Parser.Configuration.Generic'.
[<AutoOpen>]
module FSharpExtensions =
    
    /// <summary>
    /// Creates a new production rule to reduce the given symbol '<paramref name=""start""/>' into a constant provided by the function '<paramref name=""func""/>'.
    /// <para/>
    /// This function returns the newly created production rule. To discard the return value, either use ""|> ignore"" or use the function <see ref=""reduce_ci""/>.
    /// </summary>
    /// <param name=""start"">The start symbol for this production.</param>
    /// <param name=""func"">The reduce function.</param>
    /// <return>The newly created production rule.</return>
    let reduce_c (start : NonTerminalWrapper<'a>) func = start.AddProduction().SetReduceFunction(Func<'a>(func))

    /// <summary>
    /// Creates a new production rule to reduce the given symbol '<paramref name=""start""/>' into a constant provided by the function '<paramref name=""func""/>'.
    /// <para/>
    /// This function does not returns the newly created production rule. Use the function <see ref=""reduce_c""/> if you intend to reference the production rule.
    /// </summary>
    /// <param name=""start"">The start symbol for this production.</param>
    /// <param name=""func"">The reduce function.</param>
    let reduce_ci (start : NonTerminalWrapper<'a>) func = ignore <| reduce_c start func

    /// <summary>
    /// Creates a new production rule to reduce the given symbol '<paramref name=""start""/>' to the given symbol '<paramref name=""symbol""/>' using an identity mapping.
    /// <para/>
    /// This function returns the newly created production rule. To discard the return value, either use ""|> ignore"" or use the function <see ref=""reduce_0i""/>.
    /// </summary>
    /// <param name=""start"">The start symbol for this production.</param>
    /// <param name=""symbol"">The reduced symbol.</param>
    /// <return>The newly created production rule.</return>
    let reduce_0 (start : NonTerminalWrapper<'a>) symbol = start.AddProduction(symbol).SetReduceToFirst()

    /// <summary>
    /// Creates a new production rule to reduce the given symbol '<paramref name=""start""/>' to the given symbol '<paramref name=""symbol""/>' using an identity mapping.
    /// <para/>
    /// This function does not returns the newly created production rule. Use the function <see ref=""reduce_0""/> if you intend to reference the production rule.
    /// </summary>
    /// <param name=""start"">The start symbol for this production.</param>
    /// <param name=""symbol"">The reduced symbol.</param>
    /// <return>The newly created production rule.</return>
    let reduce_0i (start : NonTerminalWrapper<'a>) symbol = ignore <| reduce_0 start symbol
");

            for (int i = 1; i <= MAX_SIZE; ++i)
            {
                string[] variables = range(1, i).Select(x => "symbol" + x).ToArray();

                writer.Write($@"
    /// <summary>
    /// Creates a new production rule to reduce the given symbol '<paramref name=""start""/>' to the given {i} symbol(s) using the given production function.
    /// <para/>
    /// This function returns the newly created production rule. To discard the return value, either use ""|> ignore"" or use the function <see ref=""reduce_{i}i""/>.
    /// </summary>
    /// <param name=""start"">The start symbol for this production.</param
    /// <param name=""func"">The reduce function.</param>
    /// <return>The newly created production rule.</return>
    let reduce_{i} (start : NonTerminalWrapper<'a>) {string.Join(" ", variables)} func =
        start.AddProduction({string.Join(", ", variables)}).SetReduceFunction(Func<{string.Join(", ", Enumerable.Repeat('_', i))}, 'a>(func))


    /// <summary>
    /// Creates a new production rule to reduce the given symbol '<paramref name=""start""/>' to the given {i} symbol(s) using the given production function.
    /// <para/>
    /// This function does not returns the newly created production rule. Use the function <see ref=""reduce_{i}""/> if you intend to reference the production rule.
    /// </summary>
    /// <param name=""start"">The start symbol for this production.</param>
    /// <param name=""func"">The reduce function.</param>
    /// <return>The newly created production rule.</return>
    let reduce_{i}i (start : NonTerminalWrapper<'a>) {string.Join(" ", variables)} func =
        reduce_{i} start {string.Join(" ", variables)} func |> ignore
");
            }
        }

        private static IEnumerable<int> range(int start, int count) => Enumerable.Range(start, count);
    }
}