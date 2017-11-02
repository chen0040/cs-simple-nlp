using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.MachineTranslation
{
    public class PhraseBasedModel
    {
        public void InitializeIBMModels(IBMModel2 model_f_to_e, IBMModel2 model_e_to_f, IEnumerable<EMTrainingRecord> training_corpus_f_to_e, int maxIterations)
        {
            EMTrainingMethod.Train(model_f_to_e, model_e_to_f, training_corpus_f_to_e, maxIterations);
        }

        public Dictionary<int, int> GetIntersectionPoints(IBMModel2 model_f_to_e, IBMModel2 model_e_to_f, string[] ws_f, string[] ws_e)
        {
            Dictionary<int, int> intersection_points;
            GetAlignmentMatrix(model_f_to_e, model_e_to_f, ws_f, ws_e, out intersection_points);
            return intersection_points;
        }

        public void GetPhrases(IBMModel2 model_f_to_e, IBMModel2 model_e_to_f, string[] ws_f, string[] ws_e, Dictionary<string, string> phrases)
        {
            Dictionary<int, int> intersection_points = GetIntersectionPoints(model_f_to_e, model_e_to_f, ws_f, ws_e);

            List<int> ipList = intersection_points.Keys.ToList();
            ipList.Sort();

            for (int k = 0; k < ipList.Count-1; ++k)
            {
                int j_f1 = ipList[k];
                int j_f2 = ipList[k + 1];

                int i_e1 = intersection_points[j_f1];
                int i_e2 = intersection_points[j_f2];

            }

        }

        public void GetAlignmentMatrix(IBMModel2 model_f_to_e, IBMModel2 model_e_to_f, string[] ws_f, string[] ws_e, out Dictionary<int, int> intersection_points)
        {
            int[] alignment_f_to_e = model_f_to_e.GetAlignment(ws_f, ws_e);
            int[] alignment_e_to_f = model_e_to_f.GetAlignment(ws_e, ws_f);
            int m_f_len = alignment_f_to_e.Length;
            int l_e_len = alignment_e_to_f.Length;

            intersection_points = new Dictionary<int, int>();

            int[][] alignment_matrix = new int[m_f_len][];
            for (int j = 0; j < m_f_len; ++j)
            {
                alignment_matrix[j] = new int[l_e_len];
                for (int i = 0; i < l_e_len; ++i)
                {
                    if (alignment_f_to_e[j] == i && alignment_e_to_f[i] == j)
                    {
                        intersection_points[j] = i;
                        alignment_matrix[j][i] = 1;
                    }
                }
            }

        }

        
    }
}
