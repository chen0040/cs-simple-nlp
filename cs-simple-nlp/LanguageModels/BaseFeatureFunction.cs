using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.LanguageModels
{
    public class BaseFeatureFunction<XDataType, YDataType, FuncResult> : IFeatureFunction<XDataType, YDataType, FuncResult>
    {
        private int mDimension = -1;

        public delegate List<FuncResult> GetValueHandle(XDataType xData, YDataType y);
        protected GetValueHandle mGetValueFunction;

        public BaseFeatureFunction(int dimension, GetValueHandle handler)
        {
            mDimension = dimension;
            mGetValueFunction = handler;
        }

        public int Dimension
        {
            get { return mDimension; }
        }

        public List<FuncResult> Get(XDataType x, YDataType y)
        {
            return mGetValueFunction(x, y);
        }
    }
}
