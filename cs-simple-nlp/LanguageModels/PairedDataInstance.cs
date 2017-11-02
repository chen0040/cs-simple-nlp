using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.LanguageModels
{
    public class PairedDataInstance<XDataType, YDataType>
    {
        protected XDataType mXData; //usually a history of data
        protected YDataType mYValue;
        


        public XDataType Entry
        {
            get { return mXData; }
            set { mXData = value; }
        }

        public YDataType Label
        {
            get { return mYValue; }
            set { mYValue = value; }
        }

        public override bool Equals(object obj)
        {
            PairedDataInstance<XDataType, YDataType> rhs = obj as PairedDataInstance<XDataType, YDataType>;
            if (!rhs.Label.Equals(mYValue)) return false;
            
            if (!rhs.Entry.Equals(mXData))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = mYValue.GetHashCode();
            hash = 31 * hash + mXData.GetHashCode();
            return hash;
        }
    }
}
