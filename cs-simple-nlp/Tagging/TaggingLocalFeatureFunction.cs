using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyNLP.LanguageModels;
using MyNLP.Tokenizers;

namespace MyNLP.Tagging
{
    /// <summary>
    /// Part-of-speech or Named Entity Recognition
    /// </summary>
    public class TaggingLocalFeatureFunction : IFeatureFunction<HistoryInstance, string, int>
    {
        private Dictionary<string, int> mFeatureLookup = new Dictionary<string, int>();
        private Dictionary<string, HashSet<string>> mTagDictionary = new Dictionary<string, HashSet<string>>();

        public delegate IList<string> YDomainEntity();
        public YDomainEntity mTagDomainEntity;

        private HashSet<string> mRareWords = new HashSet<string>();

        public int mRareWordFrequency = 3;
        public int RareWordFrequency
        {
            get { return mRareWordFrequency; }
            set { mRareWordFrequency = value; }
        }

        protected int mMaxPrefixLength = 4;
        protected int mMaxSuffixLength = 4;

        public int MaxPrefixLength
        {
            get { return mMaxPrefixLength; }
            set { mMaxPrefixLength = value; }
        }

        public int MaxSuffixLength
        {
            get { return mMaxSuffixLength; }
            set { mMaxSuffixLength = value; }
        }

        public TaggingLocalFeatureFunction(YDomainEntity yDomainEntity)
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
            get { return mFeatureLookup.Count; }
        }

        public List<int> Get(HistoryInstance x, string y)
        {
            HashSet<string> feature_set = new HashSet<string>();
            ExtractFeatures(x, y, feature_set);

            List<int> result = new List<int>();
            foreach (string feature in feature_set)
            {
                int d = -1;
                if (mFeatureLookup.TryGetValue(feature, out d))
                {
                    result.Add(d);
                }
            }

            return result;
        }

        public void Reset()
        {
            mFeatureLookup.Clear();
            mTagDictionary.Clear();
            mRareWords.Clear();
        }

        protected bool IsRareWord(string w_i)
        {
            return mRareWords.Contains(w_i);
        }

        protected bool ContainsNumber(string w_i)
        {
            for (int i = 0; i < w_i.Length; ++i)
            {
                if (char.IsDigit(w_i[i]))
                {
                    return true;
                }
            }
            return false;
        }

        protected bool ContainsUppercaseChar(string w_i)
        {
            for(int i=0; i < w_i.Length; ++i)
            {
                if (char.IsUpper(w_i[i]))
                {
                    return true;
                }
            }
            return false;
        }

        protected bool ContainsHyphen(string w_i)
        {
            for (int i = 0; i < w_i.Length; ++i)
            {
                if (w_i[i] == '-')
                {
                    return true;
                }
            }
            return false;
        }

        public void GenerateFeatures(IList<PairedDataInstance<HistoryInstance, string>> training_data)
        {
            Reset();

            Dictionary<string, int> histogram = new Dictionary<string, int>();
            foreach (PairedDataInstance<HistoryInstance, string> rec in training_data)
            {
                string w_i = rec.Entry.w_i;

                HashSet<string> tags_for_word = null;
                if (mTagDictionary.ContainsKey(w_i))
                {
                    tags_for_word = mTagDictionary[w_i];
                }
                else
                {
                    tags_for_word = new HashSet<string>();
                    mTagDictionary[w_i] = tags_for_word;
                }

                tags_for_word.Add(rec.Label);

                if (histogram.ContainsKey(w_i))
                {
                    histogram[w_i] += 1;
                }
                else
                {
                    histogram[w_i] = 1;
                }
            }

            HashSet<string> feature_set = new HashSet<string>();
            foreach (PairedDataInstance<HistoryInstance, string> rec in training_data)
            {
                ExtractFeatures(rec, feature_set);
            }
            foreach (string feature in feature_set)
            {
                int d = mFeatureLookup.Count;
                mFeatureLookup[feature] = d;
            }
        }

        private void ExtractFeatures(PairedDataInstance<HistoryInstance, string> rec, HashSet<string> feature_set)
        {
            ExtractFeatures(rec.Entry, rec.Label, feature_set);
        }

        private void ExtractFeatures(HistoryInstance x, string tag_i, HashSet<string> feature_set)
        {
            string w_i = x.w_i;
            string tag_im1 = x.tag_im1;
            string tag_im2 = x.tag_im2;
            string w_im1 = x.w_im1;
            string w_im2 = x.w_im2;
            string w_ip1 = x.w_ip1;
            string w_ip2 = x.w_ip2;

            feature_set.Add(string.Format("bigram={0};{1}", tag_im1, tag_i));
            feature_set.Add(string.Format("trigram={0};{1};{2}", tag_im2, tag_im1, tag_i));
            feature_set.Add(string.Format("preword2tag={0};{1}", w_im1, tag_i));
            feature_set.Add(string.Format("prepreword2tag={0};{1}", w_im2, tag_i));
            feature_set.Add(string.Format("nextword2tag={0};{1}", w_ip1, tag_i));
            feature_set.Add(string.Format("nextnextword2tag={0};{1}", w_ip2, tag_i));

            if (mRareWords.Contains(w_i))
            {
                int maxPrefixLength = System.Math.Min(w_i.Length, mMaxPrefixLength);
                int maxSuffixLength = System.Math.Min(w_i.Length, mMaxSuffixLength);

                for (int prefixLen = 1; prefixLen <= maxPrefixLength; ++prefixLen)
                {
                    feature_set.Add(string.Format("prefix={0};{1}", w_i.Substring(0, prefixLen), tag_i));
                }

                for (int suffixLen = 1; suffixLen <= maxSuffixLength; ++suffixLen)
                {
                    feature_set.Add(string.Format("suffix={0};{1}", w_i.Substring(w_i.Length - suffixLen, suffixLen), tag_i));
                }

                //feature_set.Add(string.Format("rare={0};{1}", w_i, tag_i));

                if (ContainsNumber(w_i))
                {
                    feature_set.Add(string.Format("number={0}", tag_i));
                }
                if (ContainsUppercaseChar(w_i))
                {
                    feature_set.Add(string.Format("ucase={0}", tag_i));
                }
                if (ContainsHyphen(w_i))
                {
                    feature_set.Add(string.Format("hyphen={0}", tag_i));
                }
            }
            else
            {
                feature_set.Add(string.Format("word2tag={0};{1}", w_i, tag_i));
            }    
        }

        public HashSet<string> GetTags(string word)
        {
            if (mTagDictionary.ContainsKey(word))
            {
                return mTagDictionary[word];
            }
            return null;
        }
    }
}
