using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.Tagging
{
    public class FAQSegmentationInput
    {
        protected string mTextLine;
        protected string mPrevTextLineTag;
        protected string mPrevTextLine;

        public string TextLine
        {
            get { return mTextLine; }
            set { mTextLine = value; }
        }

        public string PrevTextLine
        {
            get { return mPrevTextLine; }
            set { mPrevTextLine = value; }
        }

        public string PrevTextLineTag
        {
            get { return mPrevTextLineTag; }
            set { mPrevTextLineTag = value; }
        }
    }
}
