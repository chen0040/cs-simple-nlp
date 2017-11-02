using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace MyNLP.Grammars
{
    /// <summary>
    /// Context Free Grammar
    /// </summary>
    public class CFG : IGrammar
    {
        protected List<string> mNonTerminals = new List<string>();
        protected List<string> mTerminals = new List<string>();
        protected List<ProductionRule> mRules = new List<ProductionRule>();
        protected List<string> mStartingSymbols = new List<string>();

        public virtual void LearnGrammarFromTreeBank(IEnumerable<string> filePaths)
        {
            foreach (string filePath in filePaths)
            {
                PennTreeBankHelper.LearnGrammarFromPennTreeBankFile(this, filePath);
            }
        }

        public List<string> StartingSymbols
        {
            get { return mStartingSymbols; }
        }

        public List<string> NonTerminals
        {
            get { return mNonTerminals; }
        }

        public List<string> Terminals
        {
            get { return mTerminals; }
        }

        public List<ProductionRule> Rules
        {
            get { return mRules; }
        }

        public void Clear()
        {
            mRules.Clear();
            mNonTerminals.Clear();
            mTerminals.Clear();
            mStartingSymbols.Clear();
        }

        protected void TryAddNonTerminals(string left, params string[] right)
        {
            if(!IsFoundInNonTerminals(left))
            {
                mNonTerminals.Add(left);
            }

            for (int i = 0; i < right.Length; ++i)
            {
                if (!IsFoundInNonTerminals(right[i]))
                {
                    mNonTerminals.Add(right[i]);
                }
            }
        }

        protected bool IsFoundInNonTerminals(string nonTerminal)
        {
            for (int i = 0; i < mNonTerminals.Count; ++i)
            {
                if (mNonTerminals[i] == nonTerminal)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual ProductionRule AddUnitProductionRule(string left, string right)
        {
            ProductionRule rule = new ProductionRule(true, left, right);
            mRules.Add(rule);
            TryAddNonTerminals(left);
            TryAddTerminals(right);
            return rule;
        }

        public virtual ProductionRule AddProductionRule(string left, params string[] right)
        {
            ProductionRule rule = new ProductionRule(left, right);
            mRules.Add(rule);
            TryAddNonTerminals(left, right);

            return rule;
        }

        protected void TryAddTerminals(string newTerminal)
        {
            if (!IsFoundInTerminals(newTerminal))
            {
                mTerminals.Add(newTerminal);
            }
        }

        private bool IsFoundInTerminals(string newTerminal)
        {
            for (int i = 0; i < mTerminals.Count; ++i)
            {
                if (mTerminals[i] == newTerminal)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddStartSymbol(string s)
        {
            bool isFound = false;
            for (int i = 0; i < mStartingSymbols.Count; ++i)
            {
                if (mStartingSymbols[i] == s)
                {
                    isFound = true;
                    break;
                }
            }
            if (!isFound)
            {
                mStartingSymbols.Add(s);
            }
        }

        public void AddProductionRule(ProductionRule rule)
        {
            bool isFound = false;
            for (int i = 0; i < mRules.Count; ++i)
            {
                if (mRules[i].Equals(rule))
                {
                    isFound = true;
                    break;
                }
            }
            if (!isFound)
            {
                mRules.Add(rule);
                if (rule.IsUnitProductionRule)
                {
                    TryAddNonTerminals(rule.Left);
                    TryAddTerminals(rule.Right[0]);
                }
                else
                {
                    TryAddNonTerminals(rule.Left, rule.Right.ToArray());
                }
            }
        }


        public string Derive(out GrammarTreeNode S)
        {
            string sentence = "";
            S = new GrammarTreeNode("S", false);

            Stack<GrammarTreeNode> derivation = new Stack<GrammarTreeNode>();
            Queue<ProductionRule> rules = new Queue<ProductionRule>();
            derivation.Push(S);
            while (derivation.Count > 0)
            {
                GrammarTreeNode s = derivation.Pop();

                if (s.IsLeaf)
                {
                    if (string.IsNullOrEmpty(sentence))
                    {
                        sentence = s.Terminal;
                    }
                    else
                    {
                        sentence += " " + s.Terminal;
                    }
                }
                else
                {
                    ProductionRule rule = SelectRandomRule(s);
                    List<GrammarTreeNode> results = rule.Fire(s);

                    for (int i = 0; i < results.Count; ++i)
                    {
                        s.AddChild(results[i]);
                    }

                    for (int i = results.Count - 1; i >= 0; --i)
                    {
                        derivation.Push(results[i]);
                    }
                }
            }

            return sentence;
        }

        public static string GetText(GrammarTreeNode tree)
        {
            string prefix = "";
            GetText(tree, ref prefix);
            return prefix;
        }

        private static void GetText(GrammarTreeNode node, ref string prefix)
        {
            if (node.IsLeaf)
            {
                if (string.IsNullOrEmpty(prefix))
                {
                    prefix = node.Terminal;
                }
                else
                {
                    prefix = prefix + " " + node.Terminal;
                }
                return;
            }

            for (int i = 0; i < node.ChildCount; ++i)
            {
                GetText(node.GetChild(i), ref prefix);
            }
        }
       


        private static Random mRandom = new Random();

        protected ProductionRule SelectRandomRule(GrammarTreeNode x)
        {
            List<ProductionRule> matchedRules = new List<ProductionRule>();

            for (int i = 0; i < mRules.Count; ++i)
            {
                if (mRules[i].Match(x))
                {
                    matchedRules.Add(mRules[i]);
                }
            }

            return matchedRules[mRandom.Next(matchedRules.Count)];
        }

        protected int GetLevel(GrammarTreeNode node)
        {
            int level = 0;
            GrammarTreeNode x = node;
            while(x != null)
            {
                level++;
                x=x.Parent;
            }

            return level;
        }

        protected Stack<ProductionRule> MatchRules(GrammarTreeNode x, int maxLevel)
        {
            int level = GetLevel(x);
            Stack<ProductionRule> matchedRules = new Stack<ProductionRule>();

            for (int i = 0; i < mRules.Count; ++i)
            {
                if (level > maxLevel)
                {
                    if (mRules[i].Match(x) && mRules[i].IsUnitProductionRule)
                    {
                        matchedRules.Push(mRules[i]);
                    }
                }
                else
                {
                    if (mRules[i].Match(x))
                    {
                        matchedRules.Push(mRules[i]);
                    }
                }
            }

            return matchedRules;
        }
    }
}
