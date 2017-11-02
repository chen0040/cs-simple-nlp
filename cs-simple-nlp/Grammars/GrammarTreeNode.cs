using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.Grammars
{
    public class GrammarTreeNode
    {
        protected string mNonTerminal = null;
        protected string mTerminal = null;
        protected List<GrammarTreeNode> mChildren = new List<GrammarTreeNode>();
        protected GrammarTreeNode mParent;
        protected double mWeight = 1;
        protected ProductionRule mRule = null;
        protected int mLexHeadIndex = 0;

        public int LexHeadIndex
        {
            get { return mLexHeadIndex; }
            set { mLexHeadIndex = value; }
        }


        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (this.IsLeaf)
            {
                sb.Append(this.Terminal);
            }
            else
            {
                sb.Append("(");
                sb.AppendFormat("{0}", this.NonTerminal);
                for (int i = 0; i < this.ChildCount; ++i)
                {
                    sb.AppendFormat(" {0}", this.GetChild(i));
                }
                sb.Append(")");
            }

            return sb.ToString();
        }


        public double Probability
        {
            get { return mWeight; }
            set { mWeight = value; }
        }

        public ProductionRule Rule
        {
            get { return mRule; }
            set { mRule = value; }
        }

        public GrammarTreeNode Parent
        {
            get { return mParent; }
            set { mParent = value; }
        }

        public GrammarTreeNode(string nt, bool isTerminal)
        {
            if (isTerminal)
            {
                mTerminal = nt;
            }
            else
            {
                mNonTerminal = nt;
            }
        }

        public GrammarTreeNode()
        {
        }

        public List<GrammarTreeNode> CollectLeafNodes()
        {
            List<GrammarTreeNode> leaf_nodes = new List<GrammarTreeNode>();
            CollectLeafNodes(this, leaf_nodes);

            return leaf_nodes;
        }

        public static void CollectLeafNodes(GrammarTreeNode node, List<GrammarTreeNode> leaf_nodes)
        {
            if (node == null) return;

            if (node.IsLeaf)
            {
                leaf_nodes.Add(node);
                return;
            }

            for(int i=0; i < node.ChildCount; ++i)
            {
                GrammarTreeNode child = node.GetChild(i);
                CollectLeafNodes(child, leaf_nodes);
            }
        }

        public GrammarTreeNode Clone()
        {
            GrammarTreeNode node = new GrammarTreeNode();
            node.Copy(this);
            return node;
        }

        public void Copy(GrammarTreeNode rhs)
        {
            mNonTerminal = rhs.mNonTerminal;
            mTerminal = rhs.mTerminal;
            mParent = rhs.mParent;
            mWeight = rhs.mWeight;

            mChildren.Clear();

            for (int i = 0; i < rhs.ChildCount; ++i)
            {
                AddChild(rhs.GetChild(i).Clone());
            }
        }


        public string NonTerminal
        {
            get { return mNonTerminal; }
            set { mNonTerminal = value; }
        }

        public bool IsLeaf
        {
            get { return mNonTerminal == null; }
        }

        public string Terminal
        {
            get { return mTerminal; }
            set { mTerminal = value; }
        }

        public int ChildCount
        {
            get { return mChildren.Count; }
        }

        public GrammarTreeNode GetChild(int j)
        {
            return mChildren[j];
        }

        public GrammarTreeNode Root
        {
            get
            {
                GrammarTreeNode x = this;
                while (x.Parent != null)
                {
                    x = x.Parent;
                }
                return x;
            }
        }

        public void AddChild(GrammarTreeNode child)
        {
            child.Parent = this;
            mChildren.Add(child);
        }

        public void SetChild(int j, GrammarTreeNode y)
        {
            y.Parent = this;
            mChildren[j] = y;
        }

        public void RemoveAllChildren()
        {
            mChildren.Clear();
        }

        


        public string Symbol
        {
            get
            {
                if (mNonTerminal == null) return mTerminal;
                return mNonTerminal;
            }
        }
    }
}
