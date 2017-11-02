using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MyNLP.Grammars
{
    public class ProductionRule
    {
        protected GrammarTreeNode mLeft = null;
        protected List<GrammarTreeNode> mRight = new List<GrammarTreeNode>();
        protected double mWeight = 1; //used for PCFG
        protected int mLexHeadIndex = 0; //used for Lexicalized PCFG

        public override bool Equals(object obj)
        {
            ProductionRule rhs = obj as ProductionRule;
            if (Left != rhs.Left) return false;
            if (mRight.Count != rhs.mRight.Count) return false;
            for (int i = 0; i < mRight.Count; ++i)
            {
                if (Right[i] != rhs.Right[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = Left.GetHashCode();
            for (int i = 0; i < mRight.Count; ++i)
            {
                hash = 31 * hash + Right[i].GetHashCode();
            }
            return hash;
        }

        public int LexHeadIndex
        {
            get { return mLexHeadIndex; }
            set { mLexHeadIndex = value; }
        }
        
        public ProductionRule(bool isTerminal, string left, string right)
        {
            mLeft=new GrammarTreeNode(left, false);
            mRight.Add(new GrammarTreeNode(right, isTerminal));
        }

        public ProductionRule(string left, params string[] right)
        {
            mLeft = new GrammarTreeNode(left, false);
            for (int i = 0; i < right.Length; ++i)
            {
                mRight.Add(new GrammarTreeNode(right[i], false));
            }
        }

        public double Weight
        {
            get { return mWeight; }
            set { mWeight = value; }
        }

        public void Add2Right(GrammarTreeNode rightlet)
        {
            mRight.Add(rightlet);
        }

        public List<GrammarTreeNode> Fire(GrammarTreeNode root)
        {
            root.Rule = this;
            root.LexHeadIndex = mLexHeadIndex;

            List<GrammarTreeNode> results = new List<GrammarTreeNode>();
            for (int i = 0; i < mRight.Count; ++i)
            {
                results.Add(mRight[i].Clone());
            }
            
            return results;
        }


        public bool Match(GrammarTreeNode x)
        {
            return mLeft.NonTerminal == x.NonTerminal;
        }

        public bool Match(string nt)
        {
            return mLeft.NonTerminal == nt;
        }

        public bool IsUnitProductionRule
        {
            get
            {
                return mRight.Count == 1 && mRight[0].IsLeaf;
            }
        }

        protected string GetSymbol(GrammarTreeNode x)
        {
            if (x.IsLeaf) return x.Terminal;
            return x.NonTerminal;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} =>", GetSymbol(mLeft));
            for (int i = 0; i < mRight.Count; ++i)
            {
                sb.AppendFormat(" {0}", GetSymbol(mRight[i]));
            }
            return sb.ToString();
        }

        public bool IsBiOutputProductionRule
        {
            get
            {
                return mRight.Count == 2;
            }
        }

        public string Left
        {
            get { return mLeft.Symbol; }
        }

        public List<string> Right
        {
            get {
                List<string> outputs = new List<string>();
                foreach (GrammarTreeNode n in mRight)
                {
                    outputs.Add(n.Symbol);
                }

                return outputs;
            }
        }

        public void SetRight(int p, GrammarTreeNode node)
        {
            mRight[p] = node;
        }

        public void SetLeft(GrammarTreeNode node)
        {
            mLeft = node;
        }

    }
}
