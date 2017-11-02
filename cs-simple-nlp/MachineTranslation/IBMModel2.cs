using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MyNLP.MachineTranslation
{
    public class IBMModel2 : IBMModel1
    {
        protected AlignmentDistortionMapper mDistortionMap = new AlignmentDistortionMapper();

        public AlignmentDistortionMapper DistortionMap
        {
            get { return mDistortionMap; }
        }

        public override double GetProbabilityOfAlignmentGivenOutputLang(string[] input_lang, int[] a, string[] output_lang)
        {
            double prob_a_given_output = 1;

            int m = input_lang.Length; //length of input;
            int l = output_lang.Length; //length of output;

            Debug.Assert(m==a.Length); 

            for (int j_input = 0; j_input < m; ++j_input)
            {
                int i_output = a[j_input];
                double q_i_given_j_and_m_and_l = GetDistortionParameter(i_output, j_input, m, l);
                prob_a_given_output *= q_i_given_j_and_m_and_l;
            }

            return prob_a_given_output;
        }

        public override int[] GetAlignment(string[] input_lang, string[] output_lang)
        {
            int m = input_lang.Length;
            int l = output_lang.Length;

            int[] a = new int[m];

            for (int j_input = 0; j_input < m; ++j_input)
            {
                string word_input = input_lang[j_input];
                
                int a_j = -1;

                double max_prob_i_given_j = GetProbabilityInputWordGivenOutputWord(word_input, "NULL") * GetDistortionParameter(-1, j_input, m, l);
                for (int i_output = 0; i_output < l; ++i_output)
                {
                    string word_output = output_lang[i_output];

                    double t_input_given_output = GetProbabilityInputWordGivenOutputWord(word_input, word_output);
                    double q_i_given_j_and_m_and_l = GetDistortionParameter(i_output, j_input, m, l);

                    double prob_i_given_j = q_i_given_j_and_m_and_l * t_input_given_output;
                    if (prob_i_given_j > max_prob_i_given_j)
                    {
                        max_prob_i_given_j = prob_i_given_j;
                        a_j = i_output;
                    }
                }
                a[j_input] = a_j;
            }

            return a;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i_output">output lang index</param>
        /// <param name="j_input">input lang index</param>
        /// <param name="input_lang"></param>
        /// <param name="output_lang"></param>
        /// <returns></returns>
        protected double GetDistortionParameter(int i_output, int j_input, int m_input_len, int l_output_len)
        {
            return mDistortionMap.GetDistortionParameter(i_output, j_input, m_input_len, l_output_len);
        }
    }
}
