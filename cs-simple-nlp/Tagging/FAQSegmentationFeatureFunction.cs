using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyNLP.LanguageModels;
using System.Text.RegularExpressions;

namespace MyNLP.Tagging
{
    public class FAQSegmentationFeatureFunction : IFeatureFunction<FAQSegmentationInput, string, int>
    {
        public delegate bool GetValueHandle(int d, FAQSegmentationInput x, string y);
        protected List<GetValueHandle> mHandles = new List<GetValueHandle>();

        public delegate IList<string> YDomainEntity();
        public YDomainEntity mTagDomainEntity;

        public FAQSegmentationFeatureFunction(YDomainEntity yDomainEntity)
        {
            mTagDomainEntity = yDomainEntity;    
        }

        protected IList<string> TagDomain
        {
            get
            {
                return mTagDomainEntity();
            }
        }


        public int Dimension
        {
            get { return mHandles.Count; }
        }

        public List<int> Get(FAQSegmentationInput x, string y)
        {
            List<int> result = new List<int>();
            for (int d = 0; d < Dimension; ++d)
            {
                if (mHandles[d](d, x, y))
                {
                    result.Add(d);
                }
            }
            return result;
        }

        public void AddFeature(GetValueHandle handle)
        {
            mHandles.Add(handle);
        }

        public void Reset()
        {
            mHandles.Clear();
        }

        public void InitializeDefault()
        {
            AddFeature((d, x, y) =>
                {
                    return y == "question" && x.PrevTextLineTag == "head" && BeginsWithNumber(x.TextLine);
                });
            AddFeature((d, x, y) =>
                {
                    return y == "question" && x.PrevTextLineTag == "head" && ContainsAlphaNum(x.TextLine);
                });
            AddFeature((d, x, y) =>
                {
                    return y == "question" && x.PrevTextLineTag == "head" && ContainsNonSpace(x.TextLine);
                });
            AddFeature((d, x, y)=>
                {
                    return y == "question" && x.PrevTextLineTag=="head" && ContainsNumber(x.TextLine);
                });
            AddFeature((d, x, y)=>
                {
                    return y == "question" && x.PrevTextLineTag=="head" && IsEmpty(x.PrevTextLine);
                });
        }

        public static bool BeginsWithNumber(string text)
        {
            return char.IsDigit(text[0]);
        }

        public void GenerateFeatures(IList<PairedDataInstance<FAQSegmentationInput, string>> training_data)
        {

        }
        public static bool ContainsAlphaNum(string text)
        {
            Regex regex = new Regex("[A-Za-z0-9]*");
            return regex.IsMatch(text);
        }

        public static bool ContainsNumber(string text)
        {
            Regex regex = new Regex("[0-9]*");
            return regex.IsMatch(text);
        }

        public static bool ContainsNonSpace(string text)
        {
            return !string.IsNullOrEmpty(text);
        }

        public static bool IsEmpty(string text)
        {
            return string.IsNullOrEmpty(text);
        }
    }
}
