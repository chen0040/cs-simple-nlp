using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyNLP.LanguageModels;

namespace MyNLP.Segmentation
{
    /// <summary>
    /// base on probabilistic model
    /// </summary>
    public class TextSegmentation : NGramModel_Word
    {
        protected int mWordCount=0;
        protected double mEpsilon = 0.0000000001;

        public TextSegmentation(double epsilon = 0.0000000001)
           : base(1)
        {
            mEpsilon = epsilon;
        }

        public void ReadWordFromCorpus(string word)
        {
            mWordCount++;
            Parse4Gram(new string[1] { word });
        }

        public double FindProbability(string candidate_word)
        {
            double prob = 0;   
            NGram<string> ngram = FindNGram(null, candidate_word);
            if (ngram == null) prob = mEpsilon;
            else prob = ((double)ngram.JoinSupportLevel) / mWordCount; 

            return prob;
        }

        public double CalcJointProbability(List<string> candidate)
        {
            double product = 1;
            foreach (string candidate_word in candidate)
            {
                product *= FindProbability(candidate_word);
            }

            return product;
        }

        public List<string> Segment(string joined_string)
        {
            Dictionary<string, List<string>> m = new Dictionary<string, List<string>>();
            return Segment(joined_string, m);
        }

       
        public List<string> Segment(string joined_string, Dictionary<string, List<string>> m)
        {
            if (joined_string.Length == 1)
            {
                List<string> c = new List<string>() { joined_string };
                m[joined_string]=c;
                return c;
            }

            double max_joint_probablity = double.MinValue;
            double joint_probability;
            
            List<string> best_candidate = null;
            
            for (int i = 1; i < joined_string.Length; ++i)
            {
                string S_first = joined_string.Substring(0, i);
                string S_rest = joined_string.Substring(i, joined_string.Length - i);
                List<string> candidate = new List<string>();
                candidate.Add(S_first);
                List<string> S_rest_segment = null;
                if (m.ContainsKey(S_rest))
                {
                    S_rest_segment = m[S_rest];
                }
                else
                {
                    S_rest_segment = Segment(S_rest, m);
                }
                candidate.AddRange(S_rest_segment);
                joint_probability = CalcJointProbability(candidate); //S_rest_joint_prob;
                if (joint_probability > max_joint_probablity)
                {
                    max_joint_probablity = joint_probability;
                    best_candidate = candidate;
                }
            }

            List<string> overral_candidate = new List<string> { joined_string };
            double overral_prob = CalcJointProbability(overral_candidate);
            if (overral_prob > max_joint_probablity)
            {
                max_joint_probablity = overral_prob;
                best_candidate = overral_candidate;
            }

            m[joined_string] = best_candidate;

            return best_candidate;
        }
    }
}
