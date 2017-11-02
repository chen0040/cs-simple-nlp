using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.Grammars
{
    /// <summary>
    /// Chomsky Normal Form
    /// </summary>
    public class CNF : PCFG
    {
        public override ProductionRule AddProductionRule(double weight, string left, params string[] right)
        {
            throw new NotImplementedException();
        }

        public virtual ProductionRule AddBiOutputProductionRule(double weight, string left, string right1, string right2)
        {
            return base.AddProductionRule(weight, left, right1, right2);
        }
    }
}
