using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyNLP.LanguageModels;
using System.Xml;
using MyNLP.Grammars;
using MyNLP.Tokenizers;
using System.Threading.Tasks;
using MyNLP.Helpers;
using System.Diagnostics;

namespace MyNLP.Tagging
{
    /// <summary>
    /// A log-linear trigram tagger for POS tagging or NE recognition
    /// </summary>
    public class TaggingLogLinearModel : LogLinearModel<HistoryInstance, string>
    {
        private const int mDefaultBeamSize = 6;

        protected int mBeamSize = mDefaultBeamSize;
        public int BeamSize
        {
            get { return mBeamSize; }
            set { mBeamSize = value; }
        }

        public TaggingLogLinearModel()
        {
            TaggingLocalFeatureFunction features = new TaggingLocalFeatureFunction(() =>
            {
                return mYDomain;
            }); 
            mModelFeatureFunction = features;
        }

        public int MaxPrefixLength
        {
            get { return (mModelFeatureFunction as TaggingLocalFeatureFunction).MaxPrefixLength; }
            set { (mModelFeatureFunction as TaggingLocalFeatureFunction).MaxPrefixLength = value; }
        }

        public int MaxSuffixLength
        {
            get { return (mModelFeatureFunction as TaggingLocalFeatureFunction).MaxSuffixLength; }
            set { (mModelFeatureFunction as TaggingLocalFeatureFunction).MaxSuffixLength = value; }
        }

        protected override string ToYValue(string yValue)
        {
            return yValue;
        }

        public void Train(IList<string> training_data)
        {
            List<TaggedSentence> tagged_sentences = TaggedSentence.ConvertToTaggedSentences(training_data);
            List<PairedDataInstance<HistoryInstance, string>> formatted_training_data = TaggedSentence.ConvertToTaggedHistoryInstances(tagged_sentences);
            Train(formatted_training_data);
        }

        public void Train(IList<GrammarTreeNode> tree_bank)
        {
            List<TaggedSentence> tagged_sentences = TaggedSentence.ConvertToTaggedSentences(tree_bank);

            Train(tagged_sentences);
        }

        public void Train(IList<TaggedSentence> tagged_sentences)
        {
            List<PairedDataInstance<HistoryInstance, string>> formatted_training_data = TaggedSentence.ConvertToTaggedHistoryInstances(tagged_sentences);
            Train(formatted_training_data);
        }

        public override void Train(IList<PairedDataInstance<HistoryInstance, string>> training_data)
        {
            (mModelFeatureFunction as TaggingLocalFeatureFunction).GenerateFeatures(training_data);

            base.Train(training_data);
        }

        public virtual string[] FindTags(string[] sentence)
        {
            return FindBestSequenceByBeamSearch(sentence, mBeamSize);
        }

        /// <summary>
        /// The method proposed by in his PhD thesis "Maximum entropy models for natural language ambiguity resolution"
        /// The PhD thesis can be found at http://www.ai.mit.edu/courses/6.891-nlp/READINGS/adwait.pdf
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        protected string[] FindBestSequenceByBeamSearch(string[] sentence, int beamSize)
        {
            int n = sentence.Length;
            int S = mYDomain.Count;

            List<List<string>> candidate_sequences = new List<List<string>>();

            for (int i = 0; i < beamSize; ++i)
            {
                candidate_sequences.Add(new List<string>());
            }

            TaggingLocalFeatureFunction ff = mModelFeatureFunction as TaggingLocalFeatureFunction;

            MaxPQ<List<string>> heap = new MaxPQ<List<string>>();
            heap.Add(1, new List<string>());

            for (int i = 0; i < n; ++i)
            {
                string word = sentence[i];

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

                List<MaxPQEntry<List<string>>> currBests = new List<MaxPQEntry<List<string>>>();
                for (int b = 0; b < bs; ++b)
                {
                    MaxPQEntry<List<string>> currBest = heap.DeleteMax();
                    currBests.Add(currBest);
                }

                heap.Clear();

                for(int b=0; b < bs; ++b)
                {
                    MaxPQEntry<List<string>> currBest = currBests[b];

                    List<string> s = currBest.Value;
                    double cprob = currBest.Key;

                    int u = s.Count - 1;
                    int w = u - 1;

                    foreach (string tag in tags)
                    {
                        List<string> newS = new List<string>();
                        newS.AddRange(s);
                        newS.Add(tag);
                        double newCProb = cprob * GetLogConditionalProbability(tag, new HistoryInstance()
                        {
                            w_i = sentence[i],
                            w_im1 = i > 0 ? sentence[i - 1] : "",
                            w_im2 = i > 1 ? sentence[i - 2] : "",
                            w_ip1 = i < sentence.Length - 1 ? sentence[i + 1] : "",
                            w_ip2 = i < sentence.Length - 2 ? sentence[i + 2] : "",
                            tag_im1 = u < 0 ? "" : s[u],
                            tag_im2 = w < 0 ? "" : s[w]
                        });
                        heap.Add(newCProb, newS);
                    }
                }
                
            }

            MaxPQEntry<List<string>> best = heap.DeleteMax();
            return best.Value.ToArray();
        }

        /// <summary>
        /// As proposed in the NLP course for finding the best tag sequence for the sentence.
        /// This is too expensive if the number of candidate tags for each word is large.
        /// Therefore normally it is not used.
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        protected string[] FindBestSequenceByViterbi(string[] sentence)
        {
            int S = YDomain.Count;

            HashSet<string> uniqueWordsInSentence = new HashSet<string>();
            foreach(string word in sentence)
            {
                uniqueWordsInSentence.Add(word);
            }
            List<string> sentence_corpus = uniqueWordsInSentence.ToList();

            int[] sentence_observations = new int[sentence.Length];
            for(int i=0; i < sentence_observations.Length; ++i)
            {
                sentence_observations[i]=sentence_corpus.IndexOf(sentence[i]);
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
                                    });
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
    }
}
