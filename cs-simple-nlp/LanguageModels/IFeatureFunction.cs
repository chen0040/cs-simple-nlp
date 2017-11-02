using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.LanguageModels
{
    public interface IFeatureFunction<XDataType, YDataType, FuncResult>
    {
        int Dimension
        {
            get;
        }

        List<FuncResult> Get(XDataType x, YDataType y);
    }
}
