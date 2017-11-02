using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyNLP.LanguageModels;

namespace MyNLP.Tagging
{
    /// <summary>
    /// This is a log-linear trigram tagger
    /// </summary>
    public class FAQSegmentationLogLinearModel : LogLinearModel<FAQSegmentationInput, string>
    {
        public FAQSegmentationLogLinearModel()
        {
            FAQSegmentationFeatureFunction features = new FAQSegmentationFeatureFunction(() =>
            {
                return mYDomain;
            }); 
            mModelFeatureFunction = features;
        }

        protected override string ToYValue(string yValue)
        {
            return yValue;
        }

        public void BuildRules(IList<PairedDataInstance<FAQSegmentationInput, string>> training_data)
        {
            HashSet<string> uniqueYValues = new HashSet<string>();
            foreach (PairedDataInstance<FAQSegmentationInput, string> rec in training_data)
            {
                uniqueYValues.Add(rec.Label);
            }
            mYDomain = uniqueYValues.ToList();

            //(mModelFeatureFunction as FAQSegmentationFeatureFunction).GenerateFeatures(tree_bank);

            Train(training_data);
        }

        public string[] FindFAQSegmentTags(IList<string> text_lines)
        {
            int S = YDomain.Count;

            HashSet<string> uniqueLines = new HashSet<string>();
            foreach (string text_line in text_lines)
            {
                uniqueLines.Add(text_line);
            }
            List<string> sentence_corpus = uniqueLines.ToList();

            int[] observations = new int[text_lines.Count];
            for (int i = 0; i < observations.Length; ++i)
            {
                observations[i] = sentence_corpus.IndexOf(text_lines[i]);
            }

            double[][] log_conditional_probability = new double[S][];
            for (int u = 0; u < S; ++u) //state at k-1
            {
                log_conditional_probability[u] = new double[S];
                for (int v = 0; v < S; ++v) //state at k
                {
                    double prob = 0;

                    for (int i = 1; i < text_lines.Count; ++i)
                    {
                        double prob_i = System.Math.Exp(GetLogConditionalProbability(mYDomain[v], new FAQSegmentationInput()
                        {
                            PrevTextLine = text_lines[i - 1],
                            TextLine = text_lines[i],
                            PrevTextLineTag = mYDomain[u]
                        }));
                        prob += prob_i;
                    }

                    log_conditional_probability[u][v] = System.Math.Log(prob);

                }
            }

            double logLikelihood;
            int[] path = LogViterbi.Forward(S, observations, log_conditional_probability, out logLikelihood);

            string[] tags = new string[text_lines.Count];

            for (int i = 0; i < path.Length; ++i)
            {
                int v = path[i];
                string tag = mYDomain[v];
                tags[i] = tag;
            }

            return tags;
        }
    }
}
