using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.Grammars
{
    public interface IGrammar
    {
        List<string> NonTerminals
        {
            get;
        }

        List<string> Terminals
        {
            get;
        }

        List<ProductionRule> Rules
        {
            get;
        }

        List<string> StartingSymbols
        {
            get;
        }

        void Clear();

        void AddStartSymbol(string s);

        ProductionRule AddUnitProductionRule(string left, string right);
        ProductionRule AddProductionRule(string left, params string[] right);
        void AddProductionRule(ProductionRule rule);

        void LearnGrammarFromTreeBank(IEnumerable<string> filePaths);

        string Derive(out GrammarTreeNode tree);
    }
}
