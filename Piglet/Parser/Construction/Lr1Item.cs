using System.Collections.Generic;
using System.Linq;
using Piglet.Parser.Configuration;

namespace Piglet.Parser.Construction
{
    internal class Lr1Item<T> : Lr0Item<T>
    {
        public ISet<Terminal<T>> Lookaheads { get; set; }

        public Lr1Item(IProductionRule<T> productionRule, int dotLocation, ISet<Terminal<T>> lookaheads) : base(productionRule, dotLocation)
        {
            Lookaheads = new HashSet<Terminal<T>>();
            Lookaheads.UnionWith(lookaheads);
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", base.ToString(), string.Join("/", Lookaheads.Select(f => f.DebugName)));
        }
    }
}
