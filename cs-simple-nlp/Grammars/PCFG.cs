using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.Grammars
{
    /// <summary>
    /// Probablistic Context Free Grammar
    /// </summary>
    public class PCFG : CFG
    {
        

        public override void LearnGrammarFromTreeBank(IEnumerable<string> filePaths)
        {
            base.LearnGrammarFromTreeBank(filePaths);

            List<ProductionRule> rules = new List<ProductionRule>();
            int count = mRules.Count;
            for (int i = 0; i < count; ++i)
            {
                rules.Add(mRules[i]);
            }

            mRules.Clear();

            Dictionary<string, int> ruleCounts = new Dictionary<string, int>();
            Dictionary<string, ProductionRule> ruleMapping = new Dictionary<string, ProductionRule>();
            Dictionary<string, int> nonTerminalCounts = new Dictionary<string,int>();

            for (int i = 0; i < count; ++i)
            {
                ProductionRule rule = rules[i];

                string ruleText = rule.ToString();

                string left = rule.Left;

                if(nonTerminalCounts.ContainsKey(left))
                {
                    nonTerminalCounts[left] += 1;
                }
                else
                {
                    nonTerminalCounts[left] = 1;
                }

                if (ruleCounts.ContainsKey(ruleText))
                {
                    ruleCounts[ruleText] += 1;
                }
                else
                {
                    ruleCounts[ruleText] = 1;
                    ruleMapping[ruleText] = rule;
                }
            }

            foreach (string ruleText in ruleCounts.Keys)
            {
                ProductionRule rule = ruleMapping[ruleText];
                rule.Weight = (double)ruleCounts[ruleText] / nonTerminalCounts[rule.Left];

                mRules.Add(rule);
            }
        }

        public static double GetTreeProbability(GrammarTreeNode node)
        {
            double probability = node.Rule.Weight;
            for (int i = 0; i < node.ChildCount; ++i)
            {
                GrammarTreeNode child = node.GetChild(i);
                probability *= GetTreeProbability(child);
            }

            node.Probability = probability;

            return probability;
        }

        public virtual ProductionRule AddUnitProductionRule(double weight, string left, string right)
        {
            ProductionRule rule = new ProductionRule(true, left, right);
            rule.Weight = weight;
            mRules.Add(rule);
            TryAddNonTerminals(left);
            TryAddTerminals(right);

            return rule;
        }

        public virtual ProductionRule AddProductionRule(double weight, string left, params string[] right)
        {
            ProductionRule rule = new ProductionRule(left, right);
            rule.Weight = weight;
            mRules.Add(rule);
            TryAddNonTerminals(left, right);

            return rule;
        }
    }
}
