using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.Grammars
{
    public class Span
    {
        protected object mTag = null;

        public object Tag
        {
            get { return mTag; }
            set { mTag = value; }
        }

        protected int mStart;
        public int Start
        {
            get
            {
                return mStart;
            }
            set
            {
                mStart = value;
            }
        }

        protected int mEnd;
        public int End
        {
            get
            {
                return mEnd;
            }
            set
            {
                mEnd = value;
            }
        }

        public bool IsSingular
        {
            get
            {
                return mStart == mEnd;
            }
        }

        public override bool Equals(object obj)
        {
            Span rhs = obj as Span;
            return mStart == rhs.mStart && mEnd == rhs.mEnd;
        }

        public override int GetHashCode()
        {
            int hash = mStart.GetHashCode();
            hash = hash * 3001 + mEnd.GetHashCode();
            return hash;
        }
    }
}
