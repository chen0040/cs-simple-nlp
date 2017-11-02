using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.Tagging
{
    public class TagSequence : List<string>
    {
        public List<double> mFScores = new List<double>();
        public override bool Equals(object obj)
        {
            TagSequence rhs = obj as TagSequence;
            if (this.Count != rhs.Count) return false;
            for (int i = 0; i < this.Count; ++i)
            {
                if (this[i] != rhs[i])
                {
                    return false;
                }
            }
            return true;
        }

        public TagSequence(List<string> obj)
        {
            for (int i = 0; i < obj.Count; ++i)
            {
                this.Add(obj[i]);
            }
            
        }

        public TagSequence()
        {
        }

        public List<double> FScores
        {
            get { return mFScores; }
        }

        public override int GetHashCode()
        {
            int hash = 0;
            for (int i = 0; i < this.Count; ++i)
            {
                hash = hash * 31 + this[i].GetHashCode();
            }
            return hash;
        }
    }
}
