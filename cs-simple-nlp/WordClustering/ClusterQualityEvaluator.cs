using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.WordClustering
{
    public class ClusterQualityEvaluator
    {
        public HashSet<int> UniqueValues(Dictionary<string, int> C)
        {
            HashSet<int> set = new HashSet<int>();
            foreach (int v in C.Values)
            {
                set.Add(v);
            }
            return set;
        }

        public double CalcClusterQuality(Dictionary<string, int> C, string[] words, int[] word_counts, int[][] word_sequence_counts)
        {
            int[] clusterIds = UniqueValues(C).ToArray();

            int M = clusterIds.Length;

            Dictionary<int, int> mapping = new Dictionary<int, int>();
            for (int i = 0; i < M; ++i)
            {
                mapping[clusterIds[i]] = i;
            }
            
            int[][] nn = new int[M][];
            int[] ns = new int[M];
            double[][] pp = new double[M][];
            double[] ps = new double[M];
            for (int i = 0; i < M; ++i)
            {
                nn[i] = new int[M];
                pp[i] = new double[M];
            }

            int word_count = words.Length;

            bool[] inClusters = new bool[word_count];
            int[] wordClusterMapping = new int[word_count];
            for (int i = 0; i < word_count; ++i)
            {
                string word = words[i];
                inClusters[i] = C.ContainsKey(word);
                wordClusterMapping[i] = inClusters[i] ? mapping[C[word]] : -1;
            }
            
            for(int i=0; i < word_count; ++i)
            {
                string word = words[i];
                if (!inClusters[i])
                {
                    continue;
                }
                int ni1 = wordClusterMapping[i];
                ns[ni1] += word_counts[i];

                for (int j = 0; j < word_count; ++j)
                {
                    string next_word = words[j];
                    if (!inClusters[j])
                    {
                        continue;
                    }
                    int ni2 = wordClusterMapping[j];
                    nn[ni1][ni2]+=word_sequence_counts[i][j];
                }
            }

            int nnall = 0;
            int nall = 0;
            for (int i = 0; i < M; ++i)
            {
                nall += ns[i];
                for(int j=0; j < M; ++j)
                {
                    nnall += nn[i][j];
                }
            }

            for (int i = 0; i < M; ++i)
            {
                ps[i] = (double)ns[i] / nall;
                for (int j = 0; j < M; ++j)
                {
                    pp[i][j] = (double)nn[i][j] / nnall;
                }
            }

            double quality = 0;
            for (int i = 0; i < M; ++i)
            {
                for (int j = 0; j < M; ++j)
                {
                    if (i == j) continue;
                    double ppij = pp[i][j];
                    if (ppij > 0)
                    {
                        quality += ppij * System.Math.Log(ppij / (ps[i] * ps[j]));
                    }
                }
            }

            for (int i = 0; i < M; ++i)
            {
                double ppii = pp[i][i];
                if (ppii > 0)
                {
                    quality += ppii * System.Math.Log(ppii / (ps[i] * ps[i]));
                }
            }

            return quality;

        }
    }
}
