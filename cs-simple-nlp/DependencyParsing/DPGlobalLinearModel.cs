using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyNLP.LanguageModels;
using MyNLP.Tagging;
using MyNLP.Tokenizers;

namespace MyNLP.DependencyParsing
{
    public class DPGlobalLinearModel : GlobalLinearModel<TaggedSentence, DPSet>
    {
        public DPGlobalLinearModel()
        {
            DPGlobalFeatureFunction features = new DPGlobalFeatureFunction();
            mModelFeatureFunction = features;
        }

        public override void Train(IList<PairedDataInstance<TaggedSentence, DPSet>> training_data)
        {
            (mModelFeatureFunction as DPGlobalFeatureFunction).GenerateFeatures(training_data);

            base.Train(training_data);
        }

        protected bool IsZero(double[] theta)
        {
            for (int i = 0; i < theta.Length; ++i)
            {
                if (theta[i] > 0) return false;
            }
            return true;
        }

        /// <summary>
        /// Implements Viterbi dynamic programming to find the 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="theta"></param>
        /// <returns></returns>
        protected override DPSet F(TaggedSentence tagged_sentence, double[] theta)
        {
            //DPSet allStates = new DPSet();

            //for(int head = 0; head < tagged_sentence.Count; ++head)
            //{
            //    for(int modifier = 0; modifier < tagged_sentence.Count; ++modifier)
            //    {
            //        if(head == modifier) continue;
            //        allStates.Add(new PairedDataInstance<int,int>(){
            //            Entry = head,
            //            Label = modifier
            //        });
            //    }
            //}

            //int S = allStates.Count;

            //HashSet<string> uniqueWordsInSentence = new HashSet<string>();
            //foreach (PairedToken token in tagged_sentence)
            //{
            //    uniqueWordsInSentence.Add(token.Word);
            //}
            //List<string> sentence_corpus = uniqueWordsInSentence.ToList();

            //int[] sentence_observations = new int[tagged_sentence.Count];
            //for (int i = 0; i < sentence_observations.Length; ++i)
            //{
            //    sentence_observations[i] = sentence_corpus.IndexOf(tagged_sentence[i].Word);
            //}

            //double[][][] log_conditional_probability = new double[S][][];
            //for (int w = 0; w < S; ++w) //state at k-2
            //{
            //    log_conditional_probability[w] = new double[S][];
            //    for (int u = 0; u < S; ++u) //state at k-1
            //    {
            //        log_conditional_probability[w][u] = new double[S];
            //        for (int v = 0; v < S; ++v) //state at k
            //        {
            //            log_conditional_probability[w][u][v] = GetLogConditionalProbability(tagged_sentence, allStates[v], S, theta);

            //        }
            //    }
            //}

            //double logLikelihood;
            //int[] path = LogViterbi.Forward(S, sentence_observations, log_conditional_probability, out logLikelihood);

            //DPSet dp_candidate = new DPSet();

            //for (int i = 0; i < path.Length; ++i)
            //{
            //    int v = path[i];
            //    PairedDataInstance<int, int> state = allStates[v];
            //    dp_candidate.Add(state);
            //}

            //return dp_candidate;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the conditional probability of y given x
        /// </summary>
        /// <param name="yVal"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public double GetLogConditionalProbability(TaggedSentence x, PairedDataInstance<int, int> y, int S, double[] theta)
        {
            //int head = y.Entry;
            //int modifier = y.Label;

            //DPGlobalFeatureFunction f = mModelFeatureFunction as DPGlobalFeatureFunction;

            //int dimension = theta.Length;
            //double log_conditional_prob = 0;
            //for (int i = 0; i < dimension; ++i)
            //{
            //    log_conditional_prob += theta[i] * f.Get(i, x, head, modifier);
            //}

            //double pF = 0;
            //for (int yi = 0; yi < S; ++yi)
            //{
            //    double pf = 0;
            //    for (int d = 0; d < dimension; ++d)
            //    {
            //        pf += theta[d] * f.Get(d, x, head, modifier);
            //    }
            //    pF += System.Math.Exp(pf);
            //}
            //log_conditional_prob -= System.Math.Log(pF);

            //return log_conditional_prob;
            throw new NotImplementedException();
        }

        public List<PairedDataInstance<int, int>> ParseDependency(TaggedSentence sentence)
        {
            List<PairedDataInstance<int,int>> dependencies = F(sentence, mTheta);
            return dependencies;
        }
    }
}
