using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Piglet.Configuration;

namespace Piglet.Construction
{
    public static class ParserFactory
    {
        public static IParser<T> CreateParser<T>(IParserConfiguration<T> parserConfiguration)
        {
            // First order of business is to create the canonical list of LR0 states.
            // This starts with augmenting the grammar with an accept symbol, then we derive the
            // grammar from that
            var start = parserConfiguration.Start;
            
            // So, we are going to calculate the LR0 closure for the start symbol, which should
            // be the augmented accept state of the grammar.
            // The closure is all states which are accessible by the dot at the left hand side of the
            // item.
            var closures = Closure(new Lr0Item<T>(start, 0), parserConfiguration);
            while (true)
            {
                var toAdd = new List<Lr0Item<T>>();
                foreach (var symbol in parserConfiguration.AllSymbols)
                {
                    foreach (var gotoItem in Goto(closures, symbol))
                    {
                        if (!closures.Any(f => f.ProductionRule == gotoItem.ProductionRule && f.DotLocation == gotoItem.DotLocation))
                        {
                            toAdd.Add(gotoItem);
                        }
                    }   
                }

                if (!toAdd.Any())
                    break;
                closures.AddRange(toAdd);
            }

            return null;
        }

        private static IEnumerable<Lr0Item<T>> Goto<T>(IEnumerable<Lr0Item<T>> closures, ISymbol<T> symbol)
        {
            // Every place there is a symbol to the right of the dot that matches the symbol we are looking for
            // add a new Lr0 item that has the dot moved one step to the right.
            return from lr0Item in closures 
                   where lr0Item.SymbolRightOfDot != null && lr0Item.SymbolRightOfDot == symbol 
                   select new Lr0Item<T>(lr0Item.ProductionRule, lr0Item.DotLocation + 1);
        }

        private static List<Lr0Item<T>> Closure<T>(Lr0Item<T> lr0Item, IParserConfiguration<T> parserConfiguration)
        {
            // The item itself is always in it's own closure set
            var closure = new List<Lr0Item<T>> {lr0Item};

            var added = new HashSet<ISymbol<T>>();
            while (true)
            {
                var toAdd = new List<Lr0Item<T>>();
                foreach (var item in closure)
                {
                    ISymbol<T> symbolRightOfDot = item.SymbolRightOfDot;
                    if (symbolRightOfDot != null && !added.Contains(symbolRightOfDot))
                    {
                        // Create new Lr0 items from all rules where the resulting symbol of the production rule
                        // matches the symbol that was to the right of the dot.
                        toAdd.AddRange(
                            parserConfiguration.ProductionRules.Where(f => f.ResultSymbol == symbolRightOfDot).Select(
                                f => new Lr0Item<T>(f, 0)));
                        added.Add(symbolRightOfDot);
                    }
                }

                if (!toAdd.Any())
                    break;
                closure.AddRange(toAdd);
            }

            return closure;
        }
    }
}