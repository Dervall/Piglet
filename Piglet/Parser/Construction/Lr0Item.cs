using System.Text;
using Piglet.Parser.Configuration;

namespace Piglet.Parser.Construction
{
    internal class Lr0Item<T>
    {
        public IProductionRule<T> ProductionRule { get; private set; } 
        public int DotLocation { get; private set; }

        public ISymbol<T> SymbolRightOfDot
        {
            get { 
                if (DotLocation < ProductionRule.Symbols.Length) 
                    return ProductionRule.Symbols[DotLocation];
                return null;
            }
        }

        public Lr0Item(IProductionRule<T> productionRule, int dotLocation)
        {
            DotLocation = dotLocation;
            ProductionRule = productionRule;
        }

        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ProductionRule.ResultSymbol.DebugName);
            sb.Append(" -> ");
            bool dotAdded = false;
            for (int i = 0; i < ProductionRule.Symbols.Length; ++i )
            {
                if (i == DotLocation)
                {
                    sb.Append("• ");
                    dotAdded = true;
                }
                sb.Append(ProductionRule.Symbols[i].DebugName);
                sb.Append(" ");
            }
            if (!dotAdded)
            {
                sb.Append("•");                
            }
            return sb.ToString();
        }
    }
}
