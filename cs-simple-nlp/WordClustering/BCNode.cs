using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.WordClustering
{
    public class BCNode
    {
        protected string mWord;

        public bool IsLeaf
        {
            get { return mLeft == null && mRight == null; }
        }

        public string Word
        {
            get { return mWord; }
            set { mWord = value; }
        }

        public BCNode(int ID, string word)
        {
            mWord = word;
            mID = ID;
        }

        public Dictionary<string, int> ToST()
        {
            Dictionary<string, int> st = new Dictionary<string, int>();
            ToST(this, st, mID);
            return st;
        }

        public static void ToST(BCNode node, Dictionary<string, int> st, int clusterId)
        {
            if (node == null) return;
            if (node.Word != null)
            {
                st[node.Word] = clusterId;
            }
            ToST(node.Left, st, clusterId);
            ToST(node.Right, st, clusterId);
        }

        public override int GetHashCode()
        {
            return mID;
        }

        public override bool Equals(object obj)
        {
            return mID == (obj as BCNode).mID;
        }

        public override string ToString()
        {
            return mID.ToString();
        }

        protected int mID = 0;
        public int ID
        {
            get { return mID; }
        }

        protected BCNode mLeft;
        protected BCNode mRight;

        public BCNode Left
        {
            get { return mLeft; }
            set { mLeft = value; }
        }

        public BCNode Right
        {
            get { return mRight; }
            set { mRight = value; }
        }
    }
}
