
///////////////////////////////////////////////////////////////////////
//             AUTOGENERATED 2020-06-10 21:36:18.331809              //
//   All your changes to this file will be lost upon re-generation.  //
///////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System;

using Piglet.Parser.Construction;
using Piglet.Lexer.Runtime;


namespace Piglet.Parser.Configuration.Generic
{
    public abstract class ParserConstructor<TOut>
    {
        private ParserWrapper? _parser = null;
        private volatile int _ntcounter = 0;

        public IParserConfigurator<object> Configurator { get; }


        public ParserConstructor()
            : this(ParserFactory.Configure<object>())
        {
        }

        public ParserConstructor(IParserConfigurator<object> configurator) => Configurator = configurator;

        protected NonTerminalWrapper<T> CreateNonTerminal<T>(string name) => new NonTerminalWrapper<T>(Configurator.CreateNonTerminal(name));

        protected NonTerminalWrapper<T> CreateNonTerminal<T>() => CreateNonTerminal<T>($"NT{++_ntcounter}");

        protected TerminalWrapper<T> CreateTerminal<T>(string regex, Func<string, T> func) => new TerminalWrapper<T>(Configurator.CreateTerminal(regex, s => func(s)));

        protected TerminalWrapper<string> CreateTerminal(string regex) => new TerminalWrapper<string>(Configurator.CreateTerminal(regex));

        /// <summary>ascending precedence!</summary>
        protected void SetPrecedenceList(params (AssociativityDirection direction, ITerminalWrapper[] symbols)[] group)
        {
            foreach ((AssociativityDirection d, ITerminalWrapper[] s) in group)
                SetAssociativity(d, s);
        }

        protected IPrecedenceGroup SetAssociativity(AssociativityDirection dir, params ITerminalWrapper[] symbol)
        {
            ITerminal<object>[] arr = symbol.Select(s => s.Symbol).ToArray();

            if (dir == AssociativityDirection.Left)
                return Configurator.LeftAssociative(arr);
            else if (dir == AssociativityDirection.Right)
                return Configurator.RightAssociative(arr);
            else
                throw new ArgumentException();
        }

        protected void CreateProduction<T, S0>(NonTerminalWrapper<T> to, SymbolWrapper<S0> s) => to.AddProduction(s).SetReduceToFirst();

        protected ProductionWrapper<T> CreateProduction<T>(NonTerminalWrapper<T> to, Func<T> func) => to.AddProduction().SetReduceFunction(func);

        protected ProductionWrapper<S0, T> CreateProduction<T, S0>(NonTerminalWrapper<T> to, SymbolWrapper<S0> s0, Func<S0, T> func) => to.AddProduction(s0).SetReduceFunction(func);

        protected ProductionWrapper<S0, S1, T> CreateProduction<T, S0, S1>(NonTerminalWrapper<T> to, SymbolWrapper<S0> s0, SymbolWrapper<S1> s1, Func<S0, S1, T> func) => to.AddProduction(s0, s1).SetReduceFunction(func);

        protected ProductionWrapper<S0, S1, S2, T> CreateProduction<T, S0, S1, S2>(NonTerminalWrapper<T> to, SymbolWrapper<S0> s0, SymbolWrapper<S1> s1, SymbolWrapper<S2> s2, Func<S0, S1, S2, T> func) => to.AddProduction(s0, s1, s2).SetReduceFunction(func);

        protected ProductionWrapper<S0, S1, S2, S3, T> CreateProduction<T, S0, S1, S2, S3>(NonTerminalWrapper<T> to, SymbolWrapper<S0> s0, SymbolWrapper<S1> s1, SymbolWrapper<S2> s2, SymbolWrapper<S3> s3, Func<S0, S1, S2, S3, T> func) => to.AddProduction(s0, s1, s2, s3).SetReduceFunction(func);

        protected ProductionWrapper<S0, S1, S2, S3, S4, T> CreateProduction<T, S0, S1, S2, S3, S4>(NonTerminalWrapper<T> to, SymbolWrapper<S0> s0, SymbolWrapper<S1> s1, SymbolWrapper<S2> s2, SymbolWrapper<S3> s3, SymbolWrapper<S4> s4, Func<S0, S1, S2, S3, S4, T> func) => to.AddProduction(s0, s1, s2, s3, s4).SetReduceFunction(func);

        protected ProductionWrapper<S0, S1, S2, S3, S4, S5, T> CreateProduction<T, S0, S1, S2, S3, S4, S5>(NonTerminalWrapper<T> to, SymbolWrapper<S0> s0, SymbolWrapper<S1> s1, SymbolWrapper<S2> s2, SymbolWrapper<S3> s3, SymbolWrapper<S4> s4, SymbolWrapper<S5> s5, Func<S0, S1, S2, S3, S4, S5, T> func) => to.AddProduction(s0, s1, s2, s3, s4, s5).SetReduceFunction(func);

        protected ProductionWrapper<S0, S1, S2, S3, S4, S5, S6, T> CreateProduction<T, S0, S1, S2, S3, S4, S5, S6>(NonTerminalWrapper<T> to, SymbolWrapper<S0> s0, SymbolWrapper<S1> s1, SymbolWrapper<S2> s2, SymbolWrapper<S3> s3, SymbolWrapper<S4> s4, SymbolWrapper<S5> s5, SymbolWrapper<S6> s6, Func<S0, S1, S2, S3, S4, S5, S6, T> func) => to.AddProduction(s0, s1, s2, s3, s4, s5, s6).SetReduceFunction(func);

        protected ProductionWrapper<S0, S1, S2, S3, S4, S5, S6, S7, T> CreateProduction<T, S0, S1, S2, S3, S4, S5, S6, S7>(NonTerminalWrapper<T> to, SymbolWrapper<S0> s0, SymbolWrapper<S1> s1, SymbolWrapper<S2> s2, SymbolWrapper<S3> s3, SymbolWrapper<S4> s4, SymbolWrapper<S5> s5, SymbolWrapper<S6> s6, SymbolWrapper<S7> s7, Func<S0, S1, S2, S3, S4, S5, S6, S7, T> func) => to.AddProduction(s0, s1, s2, s3, s4, s5, s6, s7).SetReduceFunction(func);

        protected ProductionWrapper<S0, S1, S2, S3, S4, S5, S6, S7, S8, T> CreateProduction<T, S0, S1, S2, S3, S4, S5, S6, S7, S8>(NonTerminalWrapper<T> to, SymbolWrapper<S0> s0, SymbolWrapper<S1> s1, SymbolWrapper<S2> s2, SymbolWrapper<S3> s3, SymbolWrapper<S4> s4, SymbolWrapper<S5> s5, SymbolWrapper<S6> s6, SymbolWrapper<S7> s7, SymbolWrapper<S8> s8, Func<S0, S1, S2, S3, S4, S5, S6, S7, S8, T> func) => to.AddProduction(s0, s1, s2, s3, s4, s5, s6, s7, s8).SetReduceFunction(func);

        protected ProductionWrapper<S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, T> CreateProduction<T, S0, S1, S2, S3, S4, S5, S6, S7, S8, S9>(NonTerminalWrapper<T> to, SymbolWrapper<S0> s0, SymbolWrapper<S1> s1, SymbolWrapper<S2> s2, SymbolWrapper<S3> s3, SymbolWrapper<S4> s4, SymbolWrapper<S5> s5, SymbolWrapper<S6> s6, SymbolWrapper<S7> s7, SymbolWrapper<S8> s8, SymbolWrapper<S9> s9, Func<S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, T> func) => to.AddProduction(s0, s1, s2, s3, s4, s5, s6, s7, s8, s9).SetReduceFunction(func);

        protected ProductionWrapper<S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, T> CreateProduction<T, S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10>(NonTerminalWrapper<T> to, SymbolWrapper<S0> s0, SymbolWrapper<S1> s1, SymbolWrapper<S2> s2, SymbolWrapper<S3> s3, SymbolWrapper<S4> s4, SymbolWrapper<S5> s5, SymbolWrapper<S6> s6, SymbolWrapper<S7> s7, SymbolWrapper<S8> s8, SymbolWrapper<S9> s9, SymbolWrapper<S10> s10, Func<S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, T> func) => to.AddProduction(s0, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10).SetReduceFunction(func);

        protected ProductionWrapper<S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, T> CreateProduction<T, S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11>(NonTerminalWrapper<T> to, SymbolWrapper<S0> s0, SymbolWrapper<S1> s1, SymbolWrapper<S2> s2, SymbolWrapper<S3> s3, SymbolWrapper<S4> s4, SymbolWrapper<S5> s5, SymbolWrapper<S6> s6, SymbolWrapper<S7> s7, SymbolWrapper<S8> s8, SymbolWrapper<S9> s9, SymbolWrapper<S10> s10, SymbolWrapper<S11> s11, Func<S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, T> func) => to.AddProduction(s0, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11).SetReduceFunction(func);

        protected ProductionWrapper<S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, T> CreateProduction<T, S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12>(NonTerminalWrapper<T> to, SymbolWrapper<S0> s0, SymbolWrapper<S1> s1, SymbolWrapper<S2> s2, SymbolWrapper<S3> s3, SymbolWrapper<S4> s4, SymbolWrapper<S5> s5, SymbolWrapper<S6> s6, SymbolWrapper<S7> s7, SymbolWrapper<S8> s8, SymbolWrapper<S9> s9, SymbolWrapper<S10> s10, SymbolWrapper<S11> s11, SymbolWrapper<S12> s12, Func<S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, T> func) => to.AddProduction(s0, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12).SetReduceFunction(func);

        protected ProductionWrapper<S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, T> CreateProduction<T, S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13>(NonTerminalWrapper<T> to, SymbolWrapper<S0> s0, SymbolWrapper<S1> s1, SymbolWrapper<S2> s2, SymbolWrapper<S3> s3, SymbolWrapper<S4> s4, SymbolWrapper<S5> s5, SymbolWrapper<S6> s6, SymbolWrapper<S7> s7, SymbolWrapper<S8> s8, SymbolWrapper<S9> s9, SymbolWrapper<S10> s10, SymbolWrapper<S11> s11, SymbolWrapper<S12> s12, SymbolWrapper<S13> s13, Func<S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, T> func) => to.AddProduction(s0, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13).SetReduceFunction(func);

        protected ProductionWrapper<S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, S14, T> CreateProduction<T, S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, S14>(NonTerminalWrapper<T> to, SymbolWrapper<S0> s0, SymbolWrapper<S1> s1, SymbolWrapper<S2> s2, SymbolWrapper<S3> s3, SymbolWrapper<S4> s4, SymbolWrapper<S5> s5, SymbolWrapper<S6> s6, SymbolWrapper<S7> s7, SymbolWrapper<S8> s8, SymbolWrapper<S9> s9, SymbolWrapper<S10> s10, SymbolWrapper<S11> s11, SymbolWrapper<S12> s12, SymbolWrapper<S13> s13, SymbolWrapper<S14> s14, Func<S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, S14, T> func) => to.AddProduction(s0, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14).SetReduceFunction(func);

        protected ProductionWrapper<S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, S14, S15, T> CreateProduction<T, S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, S14, S15>(NonTerminalWrapper<T> to, SymbolWrapper<S0> s0, SymbolWrapper<S1> s1, SymbolWrapper<S2> s2, SymbolWrapper<S3> s3, SymbolWrapper<S4> s4, SymbolWrapper<S5> s5, SymbolWrapper<S6> s6, SymbolWrapper<S7> s7, SymbolWrapper<S8> s8, SymbolWrapper<S9> s9, SymbolWrapper<S10> s10, SymbolWrapper<S11> s11, SymbolWrapper<S12> s12, SymbolWrapper<S13> s13, SymbolWrapper<S14> s14, SymbolWrapper<S15> s15, Func<S0, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, S14, S15, T> func) => to.AddProduction(s0, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14, s15).SetReduceFunction(func);

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

        public ProductionWrapper<T0, T> AddProduction<T0>(SymbolWrapper<T0> sym0) =>
            new ProductionWrapper<T0, T>(((INonTerminal<object>)Symbol).AddProduction(sym0.Symbol));

        public ProductionWrapper<T0, T1, T> AddProduction<T0, T1>(SymbolWrapper<T0> sym0, SymbolWrapper<T1> sym1) =>
            new ProductionWrapper<T0, T1, T>(((INonTerminal<object>)Symbol).AddProduction(sym0.Symbol, sym1.Symbol));

        public ProductionWrapper<T0, T1, T2, T> AddProduction<T0, T1, T2>(SymbolWrapper<T0> sym0, SymbolWrapper<T1> sym1, SymbolWrapper<T2> sym2) =>
            new ProductionWrapper<T0, T1, T2, T>(((INonTerminal<object>)Symbol).AddProduction(sym0.Symbol, sym1.Symbol, sym2.Symbol));

        public ProductionWrapper<T0, T1, T2, T3, T> AddProduction<T0, T1, T2, T3>(SymbolWrapper<T0> sym0, SymbolWrapper<T1> sym1, SymbolWrapper<T2> sym2, SymbolWrapper<T3> sym3) =>
            new ProductionWrapper<T0, T1, T2, T3, T>(((INonTerminal<object>)Symbol).AddProduction(sym0.Symbol, sym1.Symbol, sym2.Symbol, sym3.Symbol));

        public ProductionWrapper<T0, T1, T2, T3, T4, T> AddProduction<T0, T1, T2, T3, T4>(SymbolWrapper<T0> sym0, SymbolWrapper<T1> sym1, SymbolWrapper<T2> sym2, SymbolWrapper<T3> sym3, SymbolWrapper<T4> sym4) =>
            new ProductionWrapper<T0, T1, T2, T3, T4, T>(((INonTerminal<object>)Symbol).AddProduction(sym0.Symbol, sym1.Symbol, sym2.Symbol, sym3.Symbol, sym4.Symbol));

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T> AddProduction<T0, T1, T2, T3, T4, T5>(SymbolWrapper<T0> sym0, SymbolWrapper<T1> sym1, SymbolWrapper<T2> sym2, SymbolWrapper<T3> sym3, SymbolWrapper<T4> sym4, SymbolWrapper<T5> sym5) =>
            new ProductionWrapper<T0, T1, T2, T3, T4, T5, T>(((INonTerminal<object>)Symbol).AddProduction(sym0.Symbol, sym1.Symbol, sym2.Symbol, sym3.Symbol, sym4.Symbol, sym5.Symbol));

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T> AddProduction<T0, T1, T2, T3, T4, T5, T6>(SymbolWrapper<T0> sym0, SymbolWrapper<T1> sym1, SymbolWrapper<T2> sym2, SymbolWrapper<T3> sym3, SymbolWrapper<T4> sym4, SymbolWrapper<T5> sym5, SymbolWrapper<T6> sym6) =>
            new ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T>(((INonTerminal<object>)Symbol).AddProduction(sym0.Symbol, sym1.Symbol, sym2.Symbol, sym3.Symbol, sym4.Symbol, sym5.Symbol, sym6.Symbol));

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T> AddProduction<T0, T1, T2, T3, T4, T5, T6, T7>(SymbolWrapper<T0> sym0, SymbolWrapper<T1> sym1, SymbolWrapper<T2> sym2, SymbolWrapper<T3> sym3, SymbolWrapper<T4> sym4, SymbolWrapper<T5> sym5, SymbolWrapper<T6> sym6, SymbolWrapper<T7> sym7) =>
            new ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T>(((INonTerminal<object>)Symbol).AddProduction(sym0.Symbol, sym1.Symbol, sym2.Symbol, sym3.Symbol, sym4.Symbol, sym5.Symbol, sym6.Symbol, sym7.Symbol));

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T> AddProduction<T0, T1, T2, T3, T4, T5, T6, T7, T8>(SymbolWrapper<T0> sym0, SymbolWrapper<T1> sym1, SymbolWrapper<T2> sym2, SymbolWrapper<T3> sym3, SymbolWrapper<T4> sym4, SymbolWrapper<T5> sym5, SymbolWrapper<T6> sym6, SymbolWrapper<T7> sym7, SymbolWrapper<T8> sym8) =>
            new ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T>(((INonTerminal<object>)Symbol).AddProduction(sym0.Symbol, sym1.Symbol, sym2.Symbol, sym3.Symbol, sym4.Symbol, sym5.Symbol, sym6.Symbol, sym7.Symbol, sym8.Symbol));

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T> AddProduction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(SymbolWrapper<T0> sym0, SymbolWrapper<T1> sym1, SymbolWrapper<T2> sym2, SymbolWrapper<T3> sym3, SymbolWrapper<T4> sym4, SymbolWrapper<T5> sym5, SymbolWrapper<T6> sym6, SymbolWrapper<T7> sym7, SymbolWrapper<T8> sym8, SymbolWrapper<T9> sym9) =>
            new ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T>(((INonTerminal<object>)Symbol).AddProduction(sym0.Symbol, sym1.Symbol, sym2.Symbol, sym3.Symbol, sym4.Symbol, sym5.Symbol, sym6.Symbol, sym7.Symbol, sym8.Symbol, sym9.Symbol));

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> AddProduction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(SymbolWrapper<T0> sym0, SymbolWrapper<T1> sym1, SymbolWrapper<T2> sym2, SymbolWrapper<T3> sym3, SymbolWrapper<T4> sym4, SymbolWrapper<T5> sym5, SymbolWrapper<T6> sym6, SymbolWrapper<T7> sym7, SymbolWrapper<T8> sym8, SymbolWrapper<T9> sym9, SymbolWrapper<T10> sym10) =>
            new ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T>(((INonTerminal<object>)Symbol).AddProduction(sym0.Symbol, sym1.Symbol, sym2.Symbol, sym3.Symbol, sym4.Symbol, sym5.Symbol, sym6.Symbol, sym7.Symbol, sym8.Symbol, sym9.Symbol, sym10.Symbol));

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T> AddProduction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(SymbolWrapper<T0> sym0, SymbolWrapper<T1> sym1, SymbolWrapper<T2> sym2, SymbolWrapper<T3> sym3, SymbolWrapper<T4> sym4, SymbolWrapper<T5> sym5, SymbolWrapper<T6> sym6, SymbolWrapper<T7> sym7, SymbolWrapper<T8> sym8, SymbolWrapper<T9> sym9, SymbolWrapper<T10> sym10, SymbolWrapper<T11> sym11) =>
            new ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T>(((INonTerminal<object>)Symbol).AddProduction(sym0.Symbol, sym1.Symbol, sym2.Symbol, sym3.Symbol, sym4.Symbol, sym5.Symbol, sym6.Symbol, sym7.Symbol, sym8.Symbol, sym9.Symbol, sym10.Symbol, sym11.Symbol));

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T> AddProduction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(SymbolWrapper<T0> sym0, SymbolWrapper<T1> sym1, SymbolWrapper<T2> sym2, SymbolWrapper<T3> sym3, SymbolWrapper<T4> sym4, SymbolWrapper<T5> sym5, SymbolWrapper<T6> sym6, SymbolWrapper<T7> sym7, SymbolWrapper<T8> sym8, SymbolWrapper<T9> sym9, SymbolWrapper<T10> sym10, SymbolWrapper<T11> sym11, SymbolWrapper<T12> sym12) =>
            new ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T>(((INonTerminal<object>)Symbol).AddProduction(sym0.Symbol, sym1.Symbol, sym2.Symbol, sym3.Symbol, sym4.Symbol, sym5.Symbol, sym6.Symbol, sym7.Symbol, sym8.Symbol, sym9.Symbol, sym10.Symbol, sym11.Symbol, sym12.Symbol));

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T> AddProduction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(SymbolWrapper<T0> sym0, SymbolWrapper<T1> sym1, SymbolWrapper<T2> sym2, SymbolWrapper<T3> sym3, SymbolWrapper<T4> sym4, SymbolWrapper<T5> sym5, SymbolWrapper<T6> sym6, SymbolWrapper<T7> sym7, SymbolWrapper<T8> sym8, SymbolWrapper<T9> sym9, SymbolWrapper<T10> sym10, SymbolWrapper<T11> sym11, SymbolWrapper<T12> sym12, SymbolWrapper<T13> sym13) =>
            new ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T>(((INonTerminal<object>)Symbol).AddProduction(sym0.Symbol, sym1.Symbol, sym2.Symbol, sym3.Symbol, sym4.Symbol, sym5.Symbol, sym6.Symbol, sym7.Symbol, sym8.Symbol, sym9.Symbol, sym10.Symbol, sym11.Symbol, sym12.Symbol, sym13.Symbol));

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T> AddProduction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(SymbolWrapper<T0> sym0, SymbolWrapper<T1> sym1, SymbolWrapper<T2> sym2, SymbolWrapper<T3> sym3, SymbolWrapper<T4> sym4, SymbolWrapper<T5> sym5, SymbolWrapper<T6> sym6, SymbolWrapper<T7> sym7, SymbolWrapper<T8> sym8, SymbolWrapper<T9> sym9, SymbolWrapper<T10> sym10, SymbolWrapper<T11> sym11, SymbolWrapper<T12> sym12, SymbolWrapper<T13> sym13, SymbolWrapper<T14> sym14) =>
            new ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T>(((INonTerminal<object>)Symbol).AddProduction(sym0.Symbol, sym1.Symbol, sym2.Symbol, sym3.Symbol, sym4.Symbol, sym5.Symbol, sym6.Symbol, sym7.Symbol, sym8.Symbol, sym9.Symbol, sym10.Symbol, sym11.Symbol, sym12.Symbol, sym13.Symbol, sym14.Symbol));

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T> AddProduction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(SymbolWrapper<T0> sym0, SymbolWrapper<T1> sym1, SymbolWrapper<T2> sym2, SymbolWrapper<T3> sym3, SymbolWrapper<T4> sym4, SymbolWrapper<T5> sym5, SymbolWrapper<T6> sym6, SymbolWrapper<T7> sym7, SymbolWrapper<T8> sym8, SymbolWrapper<T9> sym9, SymbolWrapper<T10> sym10, SymbolWrapper<T11> sym11, SymbolWrapper<T12> sym12, SymbolWrapper<T13> sym13, SymbolWrapper<T14> sym14, SymbolWrapper<T15> sym15) =>
            new ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T>(((INonTerminal<object>)Symbol).AddProduction(sym0.Symbol, sym1.Symbol, sym2.Symbol, sym3.Symbol, sym4.Symbol, sym5.Symbol, sym6.Symbol, sym7.Symbol, sym8.Symbol, sym9.Symbol, sym10.Symbol, sym11.Symbol, sym12.Symbol, sym13.Symbol, sym14.Symbol, sym15.Symbol));

    }

    public abstract class ProductionWrapperBase<T>
        where T : ProductionWrapperBase<T>
    {
        public IProduction<object> Production { get; }


        public ProductionWrapperBase(IProduction<object> production) => Production = production;

        public void SetReduceToFirst() => Production.SetReduceToFirst();

        public void SetPrecedence(IPrecedenceGroup precedence) => Production.SetPrecedence(precedence);
    }


    public sealed class ProductionWrapper<R>
        : ProductionWrapperBase<ProductionWrapper<R>>
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public ProductionWrapper<R> SetReduceFunction(Func<R> f)
        {
            Production.SetReduceFunction(args => f());

            return this;
        }
    }


    public sealed class ProductionWrapper<T0, R>
        : ProductionWrapperBase<ProductionWrapper<T0, R>>
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public ProductionWrapper<T0, R> SetReduceFunction(Func<T0, R> f)
        {
            Production.SetReduceFunction(args => f((T0)args[0]));

            return this;
        }
    }


    public sealed class ProductionWrapper<T0, T1, R>
        : ProductionWrapperBase<ProductionWrapper<T0, T1, R>>
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public ProductionWrapper<T0, T1, R> SetReduceFunction(Func<T0, T1, R> f)
        {
            Production.SetReduceFunction(args => f((T0)args[0], (T1)args[1]));

            return this;
        }
    }


    public sealed class ProductionWrapper<T0, T1, T2, R>
        : ProductionWrapperBase<ProductionWrapper<T0, T1, T2, R>>
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public ProductionWrapper<T0, T1, T2, R> SetReduceFunction(Func<T0, T1, T2, R> f)
        {
            Production.SetReduceFunction(args => f((T0)args[0], (T1)args[1], (T2)args[2]));

            return this;
        }
    }


    public sealed class ProductionWrapper<T0, T1, T2, T3, R>
        : ProductionWrapperBase<ProductionWrapper<T0, T1, T2, T3, R>>
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public ProductionWrapper<T0, T1, T2, T3, R> SetReduceFunction(Func<T0, T1, T2, T3, R> f)
        {
            Production.SetReduceFunction(args => f((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3]));

            return this;
        }
    }


    public sealed class ProductionWrapper<T0, T1, T2, T3, T4, R>
        : ProductionWrapperBase<ProductionWrapper<T0, T1, T2, T3, T4, R>>
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public ProductionWrapper<T0, T1, T2, T3, T4, R> SetReduceFunction(Func<T0, T1, T2, T3, T4, R> f)
        {
            Production.SetReduceFunction(args => f((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4]));

            return this;
        }
    }


    public sealed class ProductionWrapper<T0, T1, T2, T3, T4, T5, R>
        : ProductionWrapperBase<ProductionWrapper<T0, T1, T2, T3, T4, T5, R>>
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, R> SetReduceFunction(Func<T0, T1, T2, T3, T4, T5, R> f)
        {
            Production.SetReduceFunction(args => f((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5]));

            return this;
        }
    }


    public sealed class ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, R>
        : ProductionWrapperBase<ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, R>>
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, R> SetReduceFunction(Func<T0, T1, T2, T3, T4, T5, T6, R> f)
        {
            Production.SetReduceFunction(args => f((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6]));

            return this;
        }
    }


    public sealed class ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, R>
        : ProductionWrapperBase<ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, R>>
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, R> SetReduceFunction(Func<T0, T1, T2, T3, T4, T5, T6, T7, R> f)
        {
            Production.SetReduceFunction(args => f((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7]));

            return this;
        }
    }


    public sealed class ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, R>
        : ProductionWrapperBase<ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, R>>
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, R> SetReduceFunction(Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, R> f)
        {
            Production.SetReduceFunction(args => f((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8]));

            return this;
        }
    }


    public sealed class ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, R>
        : ProductionWrapperBase<ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, R>>
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, R> SetReduceFunction(Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, R> f)
        {
            Production.SetReduceFunction(args => f((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8], (T9)args[9]));

            return this;
        }
    }


    public sealed class ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>
        : ProductionWrapperBase<ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>>
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R> SetReduceFunction(Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R> f)
        {
            Production.SetReduceFunction(args => f((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8], (T9)args[9], (T10)args[10]));

            return this;
        }
    }


    public sealed class ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, R>
        : ProductionWrapperBase<ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, R>>
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, R> SetReduceFunction(Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, R> f)
        {
            Production.SetReduceFunction(args => f((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8], (T9)args[9], (T10)args[10], (T11)args[11]));

            return this;
        }
    }


    public sealed class ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, R>
        : ProductionWrapperBase<ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, R>>
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, R> SetReduceFunction(Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, R> f)
        {
            Production.SetReduceFunction(args => f((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8], (T9)args[9], (T10)args[10], (T11)args[11], (T12)args[12]));

            return this;
        }
    }


    public sealed class ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, R>
        : ProductionWrapperBase<ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, R>>
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, R> SetReduceFunction(Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, R> f)
        {
            Production.SetReduceFunction(args => f((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8], (T9)args[9], (T10)args[10], (T11)args[11], (T12)args[12], (T13)args[13]));

            return this;
        }
    }


    public sealed class ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, R>
        : ProductionWrapperBase<ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, R>>
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, R> SetReduceFunction(Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, R> f)
        {
            Production.SetReduceFunction(args => f((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8], (T9)args[9], (T10)args[10], (T11)args[11], (T12)args[12], (T13)args[13], (T14)args[14]));

            return this;
        }
    }


    public sealed class ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, R>
        : ProductionWrapperBase<ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, R>>
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public ProductionWrapper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, R> SetReduceFunction(Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, R> f)
        {
            Production.SetReduceFunction(args => f((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8], (T9)args[9], (T10)args[10], (T11)args[11], (T12)args[12], (T13)args[13], (T14)args[14], (T15)args[15]));

            return this;
        }
    }
}
