using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MyNLP.Correction
{
    /// <summary>
    /// Assumption: based on probabilistic model
    /// Usage:
    /// List&lt;string&gt; corpus=new List&lt;string&gt;();
    /// Dictionary&lt;string, List&lt;string&gt;&gt; spelling_database=new Dictionary&lt;string, List&lt;string&gt;&gt;
    /// SetupCorpus(corpus);
    /// SetupSpellingDatabase(spelling_database);
    /// 
    /// SpellingCorrection sc=new SpellingCorrection();
    /// foreach(string c in corpus)
    /// {
    ///   sc.ScanCorpus(c);
    /// }
    /// foreach(string correct_word in spelling_database.Keys)
    /// {
    ///   List&lt;string&gt; errored_words=spelling_database[correct_word];
    ///   foreach(string errored_word in errored_words)
    ///   {
    ///     sc.ScanSpellingCorrectionDatabase(correct_word, errored_word);
    ///   }
    /// }
    /// 
    /// string corrected_word=sc.FindCorrection("Goed"); // possibly return Good
    /// </summary>
    public class SpellingCorrection
    {
        protected Dictionary<string, int> mSupportLevel_W_given_C = new Dictionary<string, int>();
        protected Dictionary<string, int> mSupportLevel_C=new Dictionary<string,int>();
        protected int mTotalCount_C = 0;
        protected Dictionary<string, int> mTotalCount_W_given_C = new Dictionary<string,int>();

        protected double mEpsilon = 0.000000001;
        public SpellingCorrection(double epsilon = 0.000000001)
        {
            mEpsilon = epsilon;
        }

        public void ScanSpellingCorrectionDatabase(string correction, string errored_word)
        {
            string W_given_C = Find_W_given_C(errored_word, correction);

            string[] parts = W_given_C.Split('|');

            if (mSupportLevel_W_given_C.ContainsKey(W_given_C))
            {
                mSupportLevel_W_given_C[W_given_C]++;
            }
            else
            {
                mSupportLevel_W_given_C[W_given_C] = 1;
            }

            string C = parts[1];
            if (mTotalCount_W_given_C.ContainsKey(C))
            {
                mTotalCount_W_given_C[C]++;
            }
            else
            {
                mTotalCount_W_given_C[C] = 1;
            }
        }

        public string Find_W_given_C(string W, string C)
        {
            // find and return difference between W and Z. e.g., if W = "thew" and Z = "thaw" then W_given_C = "e|a"
            Debug.Assert(W.Length == C.Length); //currently can only handle same length

            string w = "";
            string c = "";
            for (int i = 0; i < W.Length; ++i)
            {
                if (W[i] != C[i])
                {
                    w += W[i];
                    c += C[i];
                }
            }

            return w + "|" + c;
        }

        //currently can only handle same length
        public void AddCorrectionWithEditDistance(string word, Dictionary<string, string> corrections)
        {
            int char_to_int = Convert.ToInt32('a');
            for (int j = 0; j < word.Length; ++j)
            {
                for (int i = 0; i < 26; ++i)
                {
                    char int_to_char = Convert.ToChar(char_to_int + i);
                    string candidate = null;
                    if (j == 0)
                    {
                        candidate = int_to_char + word.Substring(1, word.Length - 1);
                    }
                    else if (j == word.Length - 1)
                    {
                        candidate = word.Substring(0, word.Length - 1) + int_to_char;
                    }
                    else
                    {
                        candidate = word.Substring(0, j) + int_to_char + word.Substring(j + 1, word.Length - j - 1);
                    }
                    if (mSupportLevel_C.ContainsKey(candidate))
                    {
                        corrections[candidate] = word[j] + "|" + int_to_char;
                    }
                }
            }
        }

        public void ScanCorpus(string word)
        {
            mTotalCount_C++;
            if (mSupportLevel_C.ContainsKey(word))
            {
                mSupportLevel_C[word]++;
            }
            else
            {
                mSupportLevel_C[word] = 1;
            }
        }

        public double FindProbability(string C)
        {
            return (double)mSupportLevel_C[C] / mTotalCount_C;
        }

        public double FindProbabilityForSpellingCorrection(string W_given_C)
        {
            if(mSupportLevel_W_given_C.ContainsKey(W_given_C))
            {
                string[] parts = W_given_C.Split('|');
                string C = parts[1];
                return (double)mSupportLevel_W_given_C[W_given_C] / mTotalCount_W_given_C[C];
            }
            return mEpsilon;
        }

        public string FindCorrection(string word)
        {
            double maxProb_C_given_W = double.MinValue;
            Dictionary<string, string> corrections=new Dictionary<string,string>();
            AddCorrectionWithEditDistance(word, corrections);

            string best_correction = null;
            foreach (string correction in corrections.Keys)
            {
                string W_given_C = corrections[correction];
                double prob_W_given_C = FindProbabilityForSpellingCorrection(W_given_C);
                double prob_C = FindProbability(correction);
                double prob_C_given_W = prob_W_given_C * prob_C;
                if (maxProb_C_given_W < prob_C_given_W)
                {
                    maxProb_C_given_W = prob_C_given_W;
                    best_correction = correction;
                }
            }

            return best_correction;
        }
    }
}
