using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyNLP.LanguageModels;
using System.Threading.Tasks;
using MyNLP.Helpers;

namespace MyNLP.Tagging
{
    

    public class TaggingGlobalLinearModel : GlobalLinearModel<SentenceStructure, TagSequence>
    {
        protected IList<string> mYDomain = null;
        public IList<string> YDomain
        {
            get { return mYDomain; }
            set { mYDomain = value; }
        }

        private const int mDefaultBeamSize = 6;

        protected int mBeamSize = mDefaultBeamSize;
        public int BeamSize
        {
            get { return mBeamSize; }
            set { mBeamSize = value; }
        }

        public TaggingGlobalLinearModel()
        {
            TaggingGlobalFeatureFunction features = new TaggingGlobalFeatureFunction(() =>
            {
                return mYDomain;
            });
            mModelFeatureFunction = features;
        }

        public int MaxPrefixLength
        {
            get { return (mModelFeatureFunction as TaggingGlobalFeatureFunction).MaxPrefixLength; }
            set { (mModelFeatureFunction as TaggingGlobalFeatureFunction).MaxPrefixLength = value; }
        }

        public int MaxSuffixLength
        {
            get { return (mModelFeatureFunction as TaggingGlobalFeatureFunction).MaxSuffixLength; }
            set { (mModelFeatureFunction as TaggingGlobalFeatureFunction).MaxSuffixLength = value; }
        }

        public override void Train(IList<PairedDataInstance<SentenceStructure, TagSequence>> training_data)
        {
            HashSet<string> uniqueYValues = new HashSet<string>();
            foreach (PairedDataInstance<SentenceStructure, TagSequence> rec in training_data)
            {
                foreach (string tag in rec.Label)
                {
                    uniqueYValues.Add(tag);
                }
            }
            mYDomain = uniqueYValues.ToList();

            (mModelFeatureFunction as TaggingGlobalFeatureFunction).GenerateFeatures(training_data);

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
        protected override TagSequence F(SentenceStructure sentence, double[] theta)
        {
            return FindBestSequenceByBeamSearch(sentence, theta, mBeamSize);
        }

        protected override List<double> GetFScores(SentenceStructure x, TagSequence y)
        {
            return y.FScores;
        }

        /// <summary>
        /// The method proposed by in his PhD thesis "Maximum entropy models for natural language ambiguity resolution"
        /// The PhD thesis can be found at http://www.ai.mit.edu/courses/6.891-nlp/READINGS/adwait.pdf
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        protected TagSequence FindBestSequenceByBeamSearch(SentenceStructure sentence, double[] theta, int beamSize)
        {
            int n = sentence.Count;
            int S = mYDomain.Count;

            List<List<string>> candidate_sequences = new List<List<string>>();

            for (int i = 0; i < beamSize; ++i)
            {
                candidate_sequences.Add(new List<string>());
            }

            TaggingGlobalFeatureFunction ff = mModelFeatureFunction as TaggingGlobalFeatureFunction;

            MaxPQ<TagSequence> heap = new MaxPQ<TagSequence>();
            heap.Add(1, new TagSequence());

            for (int i = 0; i < n; ++i)
            {
                string word = sentence[i].w_i;

                HashSet<string> tags = ff.GetTags(word);

                if (tags == null)
                {
                    tags = new HashSet<string>();
                    for (int v = 0; v < S; ++v)
                    {
                        tags.Add(mYDomain[v]);
                    }
                }

                int bs = System.Math.Min(beamSize, heap.Count);

                List<MaxPQEntry<TagSequence>> currBests = new List<MaxPQEntry<TagSequence>>();
                for (int b = 0; b < bs; ++b)
                {
                    MaxPQEntry<TagSequence> currBest = heap.DeleteMax();
                    currBests.Add(currBest);
                }

                heap.Clear();

                for (int b = 0; b < bs; ++b)
                {
                    MaxPQEntry<TagSequence> currBest = currBests[b];

                    TagSequence s = currBest.Value;
                    double cprob = currBest.Key;

                    int u = s.Count - 1;
                    int w = u - 1;

                    foreach (string tag in tags)
                    {
                        TagSequence newS = new TagSequence();
                        newS.AddRange(s);
                        newS.Add(tag);

                        newS.FScores.AddRange(s.FScores);

                        double prob = GetLogConditionalProbability(tag, sentence[i], theta);
                        newS.FScores.Add(prob);

                        double newCProb = cprob * prob;
                        heap.Add(newCProb, newS);
                    }
                }

            }

            MaxPQEntry<TagSequence> best = heap.DeleteMax();
            return best.Value;
        }

        /// <summary>
        /// As proposed in the NLP course for finding the best tag sequence for the sentence.
        /// This is too expensive if the number of candidate tags for each word is large.
        /// Therefore normally it is not used.
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        protected string[] FindBestSequenceByViterbi(string[] sentence, double[] theta)
        {
            int S = YDomain.Count;

            HashSet<string> uniqueWordsInSentence = new HashSet<string>();
            foreach (string word in sentence)
            {
                uniqueWordsInSentence.Add(word);
            }
            List<string> sentence_corpus = uniqueWordsInSentence.ToList();

            int[] sentence_observations = new int[sentence.Length];
            for (int i = 0; i < sentence_observations.Length; ++i)
            {
                sentence_observations[i] = sentence_corpus.IndexOf(sentence[i]);
            }

            double[][][][] log_conditional_probability = new double[S][][][];
            Task<double>[] tasks = new Task<double>[S * S * S * sentence.Length];
            //Console.WriteLine("Task count: {0}", tasks.Length);
            TaskFactory<double> tFactory = new TaskFactory<double>();
            int taskIndex = 0;
            for (int w2 = 0; w2 < S; ++w2) //state at k-2
            {
                log_conditional_probability[w2] = new double[S][][];
                for (int u2 = 0; u2 < S; ++u2) //state at k-1
                {
                    log_conditional_probability[w2][u2] = new double[S][];
                    for (int v2 = 0; v2 < S; ++v2) //state at k
                    {
                        int n = sentence.Length;
                        log_conditional_probability[w2][u2][v2] = new double[n];
                        for (int i2 = 0; i2 < n; ++i2)
                        {
                            tasks[taskIndex++] = tFactory.StartNew((objs) =>
                            {
                                int[] args = (int[])objs;
                                int w = args[0];
                                int u = args[1];
                                int v = args[2];
                                int i = args[3];
                                return GetLogConditionalProbability(mYDomain[v], new HistoryInstance()
                                {
                                    w_i = sentence[i],
                                    w_im1 = i > 0 ? sentence[i - 1] : "",
                                    w_im2 = i > 1 ? sentence[i - 2] : "",
                                    w_ip1 = i < sentence.Length - 1 ? sentence[i + 1] : "",
                                    w_ip2 = i < sentence.Length - 2 ? sentence[i + 2] : "",
                                    tag_im1 = mYDomain[u],
                                    tag_im2 = mYDomain[w]
                                }, theta);
                                //Console.WriteLine("w: {0}, v: {1}, i:{2}, prob: {3}", w, v, i, prob_i);

                            }, new int[4] { w2, u2, v2, i2 });
                        }

                    }
                }
            }

            Task.WaitAll(tasks);

            taskIndex = 0;
            for (int w2 = 0; w2 < S; ++w2)
            {
                for (int u2 = 0; u2 < S; ++u2)
                {
                    for (int v2 = 0; v2 < S; ++v2)
                    {
                        for (int i2 = 0; i2 < sentence.Length; ++i2)
                        {
                            log_conditional_probability[w2][u2][v2][i2] = tasks[taskIndex++].Result;
                        }
                    }
                }
            }

            double logLikelihood;
            int[] path = LogViterbi.Forward(S, sentence_observations, log_conditional_probability, out logLikelihood);
            string[] tags = new string[sentence.Length];

            for (int i = 0; i < path.Length; ++i)
            {
                int v = path[i];
                string tag = mYDomain[v];
                tags[i] = tag;
            }

            return tags;
        }

        /// <summary>
        /// Get the conditional probability of y given x
        /// </summary>
        /// <param name="yVal"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public double GetLogConditionalProbability(string y, HistoryInstance x_input, double[] theta)
        {
            List<PairedDataInstance<HistoryInstance, string>> records = new List<PairedDataInstance<HistoryInstance, string>>()
                {
                    new PairedDataInstance<HistoryInstance, string>()
                    {
                        Entry = x_input,
                        Label = y
                    }
                };

            TaggingGlobalFeatureFunction f = mModelFeatureFunction as TaggingGlobalFeatureFunction;

            int dimension = theta.Length;
            double log_conditional_prob = 0;
            List<int> featureIndexList = f.Get(x_input, y);
            for (int fIndex = 0; fIndex < featureIndexList.Count; ++fIndex) 
            {
                log_conditional_prob += theta[featureIndexList[fIndex]];
            }

            double pF = 0;
            for (int yi = 0; yi < mYDomain.Count; ++yi)
            {
                double maxTheta = double.MinValue;
                List<int> featureIndexList2 = f.Get(x_input, mYDomain[yi]);
                for (int fIndex = 0; fIndex < featureIndexList2.Count; ++fIndex)
                {
                    maxTheta = System.Math.Max(maxTheta, theta[fIndex]); 
                }
                double pf = maxTheta;
                for (int fIndex = 0; fIndex < featureIndexList2.Count; ++fIndex)
                {
                    pF += System.Math.Exp(theta[fIndex]-maxTheta);
                }
            }
            log_conditional_prob -= System.Math.Log(pF);

            return log_conditional_prob;
        }

        public string[] FindTags(string[] sentence)
        {
            List<string> tags = F(Convert(sentence), mTheta);
            return tags.ToArray();
        }

        protected SentenceStructure Convert(string[] sentence)
        {
            SentenceStructure sentence_pi = new SentenceStructure();
            for (int j = 0; j < sentence.Length; ++j)
            {
                HistoryInstance input = new HistoryInstance();
                input.w_im2 = j > 1 ? sentence[j - 2] : "";
                input.w_im1 = j > 0 ? sentence[j - 1] : "";
                input.w_i = sentence[j];
                input.w_ip1 = j < sentence.Length - 1 ? sentence[j + 1] : "";
                input.w_ip2 = j < sentence.Length - 2 ? sentence[j + 2] : "";
                input.tag_im1 = "";
                input.tag_im2 = "";

                sentence_pi.Add(input);
            }

            return sentence_pi;
        }
    }
}
