using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MyNLP.MachineTranslation
{
    /// <summary>
    /// Expectation Maximization
    /// </summary>
    public class SimpleTrainingMethod
    {
        public static void Train(IBMModel1 model, IEnumerable<SimpleTrainingRecord> training_corpus)
        {
            foreach (SimpleTrainingRecord rec in training_corpus)
            {
                string[] input_lang = rec.InputLang;
                string[] output_lang = rec.OutputLang;
                int[] a = rec.Alignment;

                int m_input_len = input_lang.Length;
                int l_output_len = output_lang.Length;

                Debug.Assert(m_input_len == a.Length);

                for (int j = 0; j < m_input_len; ++j)
                {
                    int a_j = a[j];
                    string word_input = input_lang[j];
                    string word_output = output_lang[a_j];

                    double count_input_output = model.TranslationMap.GetCountOfInputWordAndOutputWord(word_input, word_output);
                    double count_output = model.TranslationMap.GetCountOfOutputWord(word_output);

                    model.TranslationMap.SetCountOfInputWordAndOutputWord(word_input, word_output, count_input_output + 1);
                    model.TranslationMap.SetCountOfOutputWord(word_output, count_output + 1);

                    model.TranslationMap.UpdateProbabilityInputWordGivenOutputWord(word_input, word_output);
                }
            }
        }

        public static void Train(IBMModel2 model, IEnumerable<SimpleTrainingRecord> training_corpus)
        {
            foreach (SimpleTrainingRecord rec in training_corpus)
            {
                string[] input_lang = rec.InputLang;
                string[] output_lang = rec.OutputLang;
                int[] a = rec.Alignment;

                int m_input_len = input_lang.Length;
                int l_output_len = output_lang.Length;

                Debug.Assert(m_input_len == a.Length);

                for (int j = 0; j < m_input_len; ++j)
                {
                    int a_j = a[j];
                    string word_input = input_lang[j];
                    string word_output = output_lang[a_j];

                    double count_input_output = model.TranslationMap.GetCountOfInputWordAndOutputWord(word_input, word_output);
                    double count_output = model.TranslationMap.GetCountOfOutputWord(word_output);

                    model.TranslationMap.SetCountOfInputWordAndOutputWord(word_input, word_output, count_input_output + 1);
                    model.TranslationMap.SetCountOfOutputWord(word_output, count_output + 1);

                    model.TranslationMap.UpdateProbabilityInputWordGivenOutputWord(word_input, word_output);

                    double count_j_given_aj_l_m = model.DistortionMap.GetCountOfAlignmentIndexGivenAlignmentValueAndInputLenAndOutputLen(j, a_j, l_output_len, m_input_len);
                    double count_aj_l_m = model.DistortionMap.GetCountOfAlignmentValueAndInputLenAndOutputLen(a_j, l_output_len, m_input_len);

                    model.DistortionMap.SetCountOfAlignmentIndexGivenAlignmentValueAndInputLenAndOutputLen(j, a_j, l_output_len, m_input_len, count_j_given_aj_l_m + 1);
                    model.DistortionMap.SetCountOfAlignmentValueAndInputLenAndOutputLen(a_j, l_output_len, m_input_len, count_aj_l_m + 1);

                    model.DistortionMap.UpdateDistortionParameter(a_j, j, m_input_len, l_output_len);
                }
            }
        }
    }
}
