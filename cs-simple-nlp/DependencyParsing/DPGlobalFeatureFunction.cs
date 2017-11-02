using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyNLP.LanguageModels;
using MyNLP.Tokenizers;
using MyNLP.Tagging;

namespace MyNLP.DependencyParsing
{
    public class DPGlobalFeatureFunction : IFeatureFunction<TaggedSentence, DPSet, int>
    {
        private class DPLocalFunc
        {
            public delegate bool GetValueHandle(int d, TaggedSentence x, int h, int m, string[] keywords);
            private GetValueHandle mValueFunc;
            private string[] mKeywords;

            public DPLocalFunc(GetValueHandle handle, string[] keywords)
            {
                mValueFunc = handle;
                mKeywords = keywords;
            }

            public int GetValue(int d, TaggedSentence x, int h, int m)
            {
                return mValueFunc(d, x, h, m, mKeywords) ? 1 : 0;
            }
        }

        private List<DPLocalFunc> mHandles = new List<DPLocalFunc>();

        public DPGlobalFeatureFunction()
        {
   
        }

        public int Dimension
        {
            get { return mHandles.Count; }
        }

        public int Get(int d, TaggedSentence x, int head, int modifier)
        {
            return mHandles[d].GetValue(d, x, head, modifier);
        }

        public List<int> Get(TaggedSentence x, DPSet y)
        {
            //int L = x.Count;

            //List<double> result = new List<double>();

            //for (int d = 0; d < Dimension; ++d)
            //{
            //    int count = 0;
            //    for (int k = 0; k < y.Count; ++k)
            //    {
            //        int head = y[k].Entry;
            //        int modifier = y[k].Label;
            //        count += Get(d, x, head, modifier);
            //    }
            //    result.Add(count);
            //}

            //return result;
            throw new NotImplementedException();
        }

        private void AddFeature(DPLocalFunc.GetValueHandle handle, string[] keywords)
        {
            mHandles.Add(new DPLocalFunc(handle, keywords));
        }

        public void Reset()
        {
            mHandles.Clear();
        }

        protected void BuildContext(IList<PairedDataInstance<TaggedSentence, DPSet>> training_data)
        {
           
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

        public void GenerateFeatures(IList<PairedDataInstance<TaggedSentence, DPSet>> training_data)
        {
            foreach (PairedDataInstance<TaggedSentence, DPSet> instance in training_data)
            {
                string[] tags = null;
                string[] words = Extract(instance.Entry, out tags);

                //unigram
                GenerateFeatures_UniGram(words, tags);

                //bigram
                GenerateFeatures_BiGram(words, tags);
                GenerateFeatures_BiGram2(words, tags);
                GenerateFeatures_BiGram3(words, tags);

                //contextual features
                GenerateFeatures_Contextual(instance.Entry);
            }
        }

        protected void GenerateFeatures_Contextual(TaggedSentence tagged_sentence)
        {

        }

        protected void GenerateFeatures_BiGram(string[] words, string[] tags)
        {
            for (int h = 0; h < tags.Length; ++h)
            {
                for (int m = 0; m < tags.Length; ++m)
                {
                    if (h == m) continue;
                    AddFeature((d, x, h1, m1, keywords) =>
                        {
                            return x[h1].Tag == keywords[0] && x[m1].Tag == keywords[1];
                        }, new string[] { tags[h], tags[m] });
                }
            }

            for (int h = 0; h < words.Length; ++h)
            {
                for (int m = 0; m < words.Length; ++m)
                {
                    if (h == m) continue;
                    AddFeature((d, x, h1, m1, keywords) =>
                        {
                            return x[h1].Word == keywords[0] && x[m1].Word == keywords[1];
                        }, new string[] { words[h], words[m] });
                }
            }

            for (int h = 0; h < words.Length; ++h)
            {
                for (int m = 0; m < tags.Length; ++m)
                {
                    AddFeature((d, x, h1, m1, keywords) =>
                        {
                            return x[h1].Word == keywords[0] && x[m1].Tag == keywords[1];
                        }, new string[] { words[h], tags[m] });
                }
            }

            for (int h = 0; h < tags.Length; ++h)
            {
                for (int m = 0; m < words.Length; ++m)
                {
                    AddFeature((d, x, h1, m1, keywords) =>
                        {
                            return x[h1].Tag == keywords[0] && x[m1].Word == keywords[1];
                        }, new string[] { tags[h], words[m] });
                }
            }
        }

        protected void GenerateFeatures_BiGram3(string[] words, string[] tags)
        {
            for (int i = 0; i < tags.Length; ++i)
            {
                for (int j = 0; j < tags.Length; ++j)
                {
                    for (int k = 0; k < words.Length; ++k)
                    {
                        for (int l = 0; l < words.Length; ++l)
                        {
                            if (i == j && k == l) continue;

                            AddFeature((d, x, h, m, keywords) =>
                            {
                                return x[h].Tag == keywords[0] && x[m].Tag == keywords[1] && x[h].Word == keywords[2] && x[m].Word == keywords[3];
                            }, new string[] { tags[i], tags[j], words[k], words[l] });
                        }
                    }
                }
            }
        }

        protected void GenerateFeatures_BiGram2(string[] words, string[] tags)
        {
            for (int i = 0; i < tags.Length; ++i)
            {
                for (int j = 0; j < tags.Length; ++j)
                {
                    if (i == j) continue;

                    for (int k = 0; k < words.Length; ++k)
                    {
                        AddFeature((d, x, h, m, keywords) =>
                        {
                            return x[h].Tag == keywords[0] && x[m].Tag == keywords[1] && x[h].Word == keywords[2];
                        }, new string[] { tags[i], tags[j], words[k] });
                    }
                }
            }

            for (int i = 0; i < words.Length; ++i)
            {
                for (int j = 0; j < words.Length; ++j)
                {
                    if (i == j) continue;

                    for (int k = 0; k < tags.Length; ++k)
                    {
                        AddFeature((d, x, h, m, keywords) =>
                        {
                            return x[h].Word == keywords[0] && x[m].Word == keywords[1] && x[h].Tag == keywords[2];
                        }, new string[] { words[i], words[j], tags[k] });
                    }
                }
            }

        }

        protected void GenerateFeatures_UniGram(string[] words, string[] tags)
        {
            for (int i = 0; i < words.Length; ++i)
            {
                AddFeature((d, x, h, m, keywords) =>
                {
                    return x[h].Word == keywords[0];
                }, new string[] { words[i] });
            }

            for (int i = 0; i < words.Length; ++i)
            {
                AddFeature((d, x, h, m, keywords) =>
                {
                    return x[m].Word == keywords[0];
                }, new string[] { words[i] });
            }

            for (int i = 0; i < tags.Length; ++i)
            {
                AddFeature((d, x, h, m, keywords) =>
                {
                    return x[h].Tag == keywords[0];
                }, new string[] { tags[i] });
            }

            for (int i = 0; i < tags.Length; ++i)
            {
                AddFeature((d, x, h, m, keywords) =>
                {
                    return x[m].Tag == keywords[0];
                }, new string[] { tags[i] });
            }
        }

        public static string[] Extract(TaggedSentence tagged_sentence, out string[] tags)
        {
            HashSet<string> word_set = new HashSet<string>();
            HashSet<string> tag_set = new HashSet<string>();
            foreach (PairedToken token in tagged_sentence)
            {
                word_set.Add(token.Word);
                tag_set.Add(token.Tag);
            }

            tags = tag_set.ToArray();

            return word_set.ToArray();
        }


    }
}
