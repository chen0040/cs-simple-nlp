using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.WordClustering
{
    /// <summary>
    /// Can be used for text_line clustering but is slow 
    /// </summary>
    public class OneLinkageClustering
    {
        protected int mK;
        protected Dictionary<string, int> mC;

        public delegate double CalcClusterQualityHandle(Dictionary<string, int> C, IEnumerable<string> word_sequence);
        public event CalcClusterQualityHandle CalcClusterQuality;

        protected ClusterQualityEvaluator mClusterEvaluator = new ClusterQualityEvaluator();

        protected double _CalcClusterQuality(Dictionary<string, int> C, IEnumerable<string> word_sequence)
        {
            if (CalcClusterQuality != null)
            {
                return CalcClusterQuality(C, word_sequence);
            }
            else
            {
                throw new NotImplementedException();
                //return mClusterEvaluator.CalcClusterQuality(Z, word_sequence);
            }
        }

        
        public OneLinkageClustering(int K)
        {
            mK = K;
        }

        public void Cluster(Corpus corpus, IEnumerable<string> word_sequence)
        {
            int N = corpus.Count;
            mC = new Dictionary<string, int>();
            int classLabel = 0;
            HashSet<int> clusterLabels = new HashSet<int>();
            foreach(string word in corpus)
            {
                mC[word] = classLabel;
                clusterLabels.Add(classLabel);
                classLabel++;
            }

            for (int k = 0; k < N - mK; ++k)
            {
                int[] clusterList = clusterLabels.ToArray();

                int selectedClassLabel1 = -1;
                int selectedClassLabel2 = -2;
                double maxCQuality = double.MinValue;

                for (int i = 0; i < clusterList.Length-1; ++i)
                {
                    int classLabel1 = clusterList[i];
                    for (int j = i+1; j < clusterList.Length; ++j)
                    {
                        int classLabel2 = clusterList[j];
                        Dictionary<string, int> tempC = new Dictionary<string,int>(); // mC.Clone();
                        foreach (string key in mC.Keys)
                        {
                            tempC[key] = mC[key];
                        }

                        //merge cluster1 and cluster2
                        foreach (string word in corpus)
                        {
                            if (tempC[word] == classLabel2)
                            {
                                tempC[word] = classLabel1;
                            }
                        }

                        double cQuality = _CalcClusterQuality(tempC, word_sequence);
                        if (cQuality > maxCQuality)
                        {
                            selectedClassLabel1 = classLabel1;
                            selectedClassLabel2 = classLabel2;
                            maxCQuality = cQuality;
                        }
                    }
                }

                if (selectedClassLabel1 != -1 || selectedClassLabel2 != -1)
                {
                    break;
                }

                //merge cluster1 and cluster2
                foreach (string word in corpus)
                {
                    if (mC[word] == selectedClassLabel2)
                    {
                        mC[word] = selectedClassLabel1;
                    }
                }

                clusterLabels.Remove(selectedClassLabel2);
            }

            Dictionary<int, int> correctedMapping = new Dictionary<int, int>();
            classLabel = 0;
            foreach (int cLabel in clusterLabels)
            {
                correctedMapping[cLabel] = classLabel++;
            }

            foreach (string word in corpus)
            {
                classLabel = mC[word];
                int corrected = correctedMapping[classLabel];
                mC[word] = corrected;
            }
        }
    }
}
