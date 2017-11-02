using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml;
using FeedbackControl.Helpers;

namespace MyNLP.WordClustering
{
    public class BrownClustering
    {
        protected int mM;
        private BCNode mClusterRoot = null;

        public BrownClustering(int m = 1000)
        {
            mM = m;
        }

        
        public string GetCodeWord(string word)
        {
            return GetCodeWord(mClusterRoot, word, "");
        }

        private string GetCodeWord(BCNode x, string word, string codeword)
        {
            if (x == null) return null;

            if (word == x.Word)
            {
                return codeword;
            }

            string codeword3 = GetCodeWord(x.Left, word, codeword+"0");
            string codeword4 = GetCodeWord(x.Right, word, codeword+"1");

            Debug.Assert((codeword3 == null && codeword4 != null) || (codeword3 != null && codeword4 == null));

            if (codeword3 == null) return codeword4;
            return codeword3;
        }

        public delegate double CalcClusterQualityHandle(Dictionary<string, int> C, string[] words, int[] word_counts, int[][] word_sequence_counts);
        public event CalcClusterQualityHandle CalcClusterQuality;

        protected ClusterQualityEvaluator mClusterEvaluator = new ClusterQualityEvaluator();

        protected double _CalcClusterQuality(Dictionary<string, int> C, string[] words, int[] word_counts, int[][] word_sequence_counts)
        {
            if (CalcClusterQuality != null)
            {
                return CalcClusterQuality(C, words, word_counts, word_sequence_counts);
            }
            else
            {
                return mClusterEvaluator.CalcClusterQuality(C, words, word_counts, word_sequence_counts);
            }
        }

        public Dictionary<string, List<string>> GetClustersWithCodewordsOfLength(int hLevel)
        {
            List<string> codewords = new List<string>();
            GetCodewordsWithLength(mClusterRoot, hLevel, codewords, "");

            Dictionary<string, List<string>> results = new Dictionary<string, List<string>>();
            foreach (string codeword in codewords)
            {
                List<string> words = new List<string>();
                GetWordsWithCodewordPrefix(mClusterRoot, codeword, words, "");
                results[codeword] = words;
            }
            return results;
        }

        private void GetWordsWithCodewordPrefix(BCNode x, string codeword, List<string> words, string prefix)
        {
            if (x == null) return;
            if (prefix.Length == codeword.Length)
            {
                if (prefix == codeword)
                {
                    words.AddRange(x.ToST().Keys);
                }
                return;
            }

            GetWordsWithCodewordPrefix(x.Left, codeword, words, prefix + "0");
            GetWordsWithCodewordPrefix(x.Right, codeword, words, prefix + "1");
        }

        private void GetCodewordsWithLength(BCNode x, int length, List<string> codewords, string codeword)
        {
            if (x == null) return;
            if (codeword.Length == length)
            {
                codewords.Add(codeword);
                return;
            }
            GetCodewordsWithLength(x.Left, length, codewords, codeword + "0");
            GetCodewordsWithLength(x.Right, length, codewords, codeword + "1");
        }

        public XmlElement ToXml(XmlDocument doc)
        {
            return ToXml(mClusterRoot, doc, 0, "");
        }

        private XmlElement ToXml(BCNode x, XmlDocument doc, int level, string codeword)
        {
            if (x == null) return null;
            XmlElement xmlElement = doc.CreateElement("Node");

            if (x.Word != null)
            {
                XmlAttribute attrWord = doc.CreateAttribute("Word");
                attrWord.Value = x.Word;
                xmlElement.Attributes.Append(attrWord);
            }

            XmlAttribute attrCodeword = doc.CreateAttribute("Codeword");
            attrCodeword.Value = codeword;
            xmlElement.Attributes.Append(attrCodeword);

            XmlElement xmlElement1 = ToXml(x.Left, doc, level+1, codeword+"0");
            XmlElement xmlElement2 = ToXml(x.Right, doc, level+1, codeword+"1");

            if(xmlElement1!=null)
            {
                XmlAttribute attrWing = doc.CreateAttribute("Wing");
                attrWing.Value = "0";
                xmlElement1.Attributes.Append(attrWing);
                xmlElement.AppendChild(xmlElement1);
            }

            if (xmlElement2 != null)
            {
                XmlAttribute attrWing = doc.CreateAttribute("Wing");
                attrWing.Value = "1";
                xmlElement2.Attributes.Append(attrWing);
                xmlElement.AppendChild(xmlElement2);
            }

            return xmlElement;
        }

        public void Cluster(Corpus corpus, IEnumerable<string> word_sequence)
        {
            Dictionary<string, int> word_count_mapping = new Dictionary<string, int>();

            Dictionary<string, Dictionary<string, int>> word_sequence_count_mapping = new Dictionary<string, Dictionary<string, int>>();

            string prev_word = null;
            
            foreach (string word in word_sequence)
            {
                if (!corpus.Contains(word))
                {
                    prev_word = null;
                    continue;
                }

                if (word_count_mapping.ContainsKey(word))
                {
                    word_count_mapping[word] += 1;
                }
                else
                {
                    word_count_mapping[word] = 1;
                }

                if (prev_word == null)
                {
                    prev_word = word;
                    continue;
                }
                Dictionary<string, int> next_word_counts = null;
                if (word_sequence_count_mapping.ContainsKey(prev_word))
                {
                    next_word_counts = word_sequence_count_mapping[prev_word];
                }
                else
                {
                    next_word_counts = new Dictionary<string, int>();
                    word_sequence_count_mapping[prev_word] = next_word_counts;
                }

                if (next_word_counts.ContainsKey(word))
                {
                    next_word_counts[word] += 1;
                }
                else
                {
                    next_word_counts[word] = 1;
                }
            }

            string[] words = word_count_mapping.Keys.ToArray();
            int word_count = words.Length;
            int[] word_counts = new int[word_count];
            int[][] word_sequence_counts = new int[word_count][];
            
            for (int i = 0; i < word_count; ++i)
            {
                prev_word = words[i];
                word_counts[i] = word_count_mapping[prev_word];

                Dictionary<string, int> next_word_count_mapping = null;
                if (word_sequence_count_mapping.ContainsKey(prev_word))
                {
                    next_word_count_mapping = word_sequence_count_mapping[prev_word];
                }
                int[] next_word_counts = new int[word_count];
                word_sequence_counts[i] = next_word_counts;
                for (int j = 0; j < word_count; ++j)
                {
                    string word = words[j];
                    if (next_word_count_mapping == null || !next_word_count_mapping.ContainsKey(word))
                    {
                        next_word_counts[j] = 0;
                    }
                    else
                    {

                        next_word_counts[j] = next_word_count_mapping[word];
                    }
                }
            }

           
            BinaryHeapMaxPQ<string> pq = new BinaryHeapMaxPQ<string>();
            pq.CompareKeys += (k1, k2) =>
                {
                    return word_count_mapping[k1].CompareTo(word_count_mapping[k2]);
                };
            foreach (string word in corpus)
            {
                pq.Insert(word);
            }

            int N = corpus.Count;
            int m = System.Math.Min(N, mM);
            HashSet<BCNode> clusters = new HashSet<BCNode>();
            for (int k = 0; k < m; ++k)
            {
                string frequent_word = pq.DeleteMax();
                BCNode node = new BCNode(k, frequent_word);
                clusters.Add(node);
            }

            int ccount = clusters.Count+1;
            int tws_count = (ccount * (ccount - 1)) / 2;

            int parentClusterClassId = N;
            for (int k = m; k < N; ++k)
            {
                string next_frequent_word = pq.DeleteMax();
                BCNode node = new BCNode(k, next_frequent_word);
                clusters.Add(node);

                BCNode[] nodes = clusters.ToArray();

                Debug.Assert(nodes.Length == ccount);

                TaskFactory<ClusterTaskResult> tFactory = new TaskFactory<ClusterTaskResult>();
                
                Task<ClusterTaskResult>[] tasks = new Task<ClusterTaskResult>[tws_count];

                int tws_index = 0;
                for (int i = 0; i < nodes.Length-1; ++i)
                {
                    for (int j = i + 1; j < nodes.Length; ++j)
                    {
                        Func<object, ClusterTaskResult> action = (Object obj) =>
                            {
                                int[] args = (int[])obj;
                                int ii = args[0];
                                int jj = args[1];
                                int taskId = args[2];

                                BCNode node1 = nodes[ii];
                                Dictionary<string, int> C1 = node1.ToST();
                                IEnumerable<string> keys1 = C1.Keys;

                                BCNode node2 = nodes[jj];
                                int clusterId2 = node2.ID;

                                Dictionary<string, int> tempC = new Dictionary<string, int>();
                                for (int l = 0; l < nodes.Length; ++l)
                                {
                                    if (l != ii)
                                    {
                                        BCNode.ToST(nodes[l], tempC, nodes[l].ID);
                                    }
                                }

                                foreach (string key1 in keys1)
                                {
                                    tempC[key1] = node2.ID;
                                }

                                double cQuality = _CalcClusterQuality(tempC, words, word_counts, word_sequence_counts);

                                //Console.WriteLine("task: {0} ii: {1} jj: {2} quality: {3:0.00}", taskId, ii, jj, cQuality);
                                ClusterTaskResult result = new ClusterTaskResult();
                                result.cQuality = cQuality;
                                result.Node1 = node1;
                                result.Node2 = node2;

                                return result;
                            };

                        tasks[tws_index] = tFactory.StartNew(action, new int[] { i, j, tws_index });
                        tws_index++;
                    }
                }
                Debug.Assert(tws_index == tws_count);
                Task<ClusterTaskResult>.WaitAll(tasks);

                double maxCQuality = double.MinValue;

                BCNode selectedNode1 = null;
                BCNode selectedNode2 = null;

                for (int i = 0; i < tasks.Length; ++i)
                {
                    ClusterTaskResult result = tasks[i].Result;
                    double cQuality = result.cQuality;

                    if (cQuality > maxCQuality)
                    {
                        maxCQuality = cQuality;
                        selectedNode1 = result.Node1;
                        selectedNode2 = result.Node2;
                    }
                }

                if (selectedNode1 == null || selectedNode2 == null)
                {
                    break;
                }

                Console.WriteLine("k : {0} Node1: {1} Node2: {2} Quality: {3}", k, selectedNode1.ID, selectedNode2.ID, maxCQuality);
                

                BCNode parentNode = new BCNode(parentClusterClassId++, null);
                parentNode.Left = selectedNode1;
                parentNode.Right = selectedNode2;
                clusters.Remove(selectedNode1);
                clusters.Remove(selectedNode2);
                clusters.Add(parentNode);
            }

            for (int k = 0; k < m - 1; ++k)
            {
                BCNode[] nodes = clusters.ToArray();

                tws_count = (nodes.Length - 1) * nodes.Length / 2;

                TaskFactory<ClusterTaskResult> tFactory = new TaskFactory<ClusterTaskResult>();
                
                Task<ClusterTaskResult>[] tasks = new Task<ClusterTaskResult>[tws_count];

                int tws_index = 0;
                for (int i = 0; i < nodes.Length - 1; ++i)
                {
                    for (int j = i + 1; j < nodes.Length; ++j)
                    {
                        Func<object, ClusterTaskResult> action = (object obj) =>
                            {
                                int[] args = (int[])obj;
                                int ii = args[0];
                                int jj = args[1];

                                BCNode node1 = nodes[ii];
                                Dictionary<string, int> C1 = node1.ToST();
                                IEnumerable<string> keys1 = C1.Keys;

                                BCNode node2 = nodes[jj];
                                int classLabel2 = node2.ID;

                                Dictionary<string, int> tempC = new Dictionary<string, int>();
                                for (int l = 0; l < nodes.Length; ++l)
                                {
                                    if (l != ii)
                                    {
                                        BCNode.ToST(nodes[l], tempC, nodes[l].ID);
                                    }
                                }

                                foreach (string key1 in keys1)
                                {
                                    tempC[key1] = classLabel2;
                                }

                                double cQuality = _CalcClusterQuality(tempC, words, word_counts, word_sequence_counts);

                                ClusterTaskResult result = new ClusterTaskResult();
                                result.cQuality = cQuality;
                                result.Node1 = node1;
                                result.Node2 = node2;

                                return result;
                            };
                        tasks[tws_index++] = tFactory.StartNew(action, new int[] { i, j });
                    }
                }

                Debug.Assert(tws_index == tws_count);
                Task<ClusterTaskResult>.WaitAll(tasks);

                double maxCQuality = double.MinValue;

                BCNode selectedNode1 = null;
                BCNode selectedNode2 = null;

                for (int i = 0; i < tasks.Length; ++i)
                {
                    ClusterTaskResult result = tasks[i].Result;
                    double cQuality = result.cQuality;

                    if (cQuality > maxCQuality)
                    {
                        maxCQuality = cQuality;
                        selectedNode1 = result.Node1;
                        selectedNode2 = result.Node2;
                    }
                }

                if (selectedNode1 == null || selectedNode2 == null)
                {
                    break;
                }

                Console.WriteLine("K : {0} Node1: {1} Node2: {2} Quality: {3}", k, selectedNode1.ID, selectedNode2.ID, maxCQuality);
                
                BCNode parentNode = new BCNode(parentClusterClassId++, null);
                parentNode.Left = selectedNode1;
                parentNode.Right = selectedNode2;
                clusters.Remove(selectedNode1);
                clusters.Remove(selectedNode2);
                clusters.Add(parentNode);

            }

            Debug.Assert(clusters.Count == 1);

            mClusterRoot = clusters.First();
        }
    }
}
