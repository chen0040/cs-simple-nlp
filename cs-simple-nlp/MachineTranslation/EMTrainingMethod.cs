using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MyNLP.MachineTranslation
{
    public class EMTrainingMethod
    {
        private static Random mRandom = new Random();

        public static void Train(IBMModel2 model_f_to_e, IBMModel2 model_e_to_f, IEnumerable<EMTrainingRecord> training_corpus_from_f_to_e, int maxIterations)
        {
            Train(model_f_to_e, training_corpus_from_f_to_e, maxIterations);

            List<EMTrainingRecord> training_corpus_from_e_to_f = new List<EMTrainingRecord>();
            foreach (EMTrainingRecord rec in training_corpus_from_f_to_e)
            {
                EMTrainingRecord rec2 = new EMTrainingRecord()
                {
                    InputLang=rec.OutputLang,
                    OutputLang=rec.InputLang
                };
                training_corpus_from_e_to_f.Add(rec2);
            }
            Train(model_e_to_f, training_corpus_from_f_to_e, maxIterations);
        }

        public static void Initialize(IBMModel2 model, IEnumerable<EMTrainingRecord> training_corpus)
        {
            foreach (EMTrainingRecord rec in training_corpus)
            {
                string[] input_lang = rec.InputLang;
                string[] output_lang = rec.OutputLang;

                int m_input_len = input_lang.Length;
                int l_output_len = output_lang.Length;

                for (int j = 0; j < m_input_len; ++j)
                {
                    for (int a_j = 0; a_j < l_output_len; ++a_j)
                    {
                        string word_input = input_lang[j];
                        string word_output = output_lang[a_j];

                        double count_input_output = model.TranslationMap.GetCountOfInputWordAndOutputWord(word_input, word_output);
                        double count_output = model.TranslationMap.GetCountOfOutputWord(word_output);

                        model.TranslationMap.SetProbabilityInputWordGivenOutputWord(word_input, word_output, mRandom.NextDouble());

                        double count_j_given_aj_l_m = model.DistortionMap.GetCountOfAlignmentIndexGivenAlignmentValueAndInputLenAndOutputLen(j, a_j, l_output_len, m_input_len);
                        double count_aj_l_m = model.DistortionMap.GetCountOfAlignmentValueAndInputLenAndOutputLen(a_j, l_output_len, m_input_len);

                        model.DistortionMap.SetDistortionParameter(a_j, j, m_input_len, l_output_len, mRandom.NextDouble());
                    }
                }
            }
        }

        public static void Train(IBMModel2 model, IEnumerable<EMTrainingRecord> training_corpus, int maxIterations)
        {
            for (int iteration = 0; iteration < maxIterations; ++iteration)
            {

                foreach (EMTrainingRecord rec in training_corpus)
                {
                    string[] input_lang = rec.InputLang;
                    string[] output_lang = rec.OutputLang;
                    
                    int m_input_len = input_lang.Length;
                    int l_output_len = output_lang.Length;

                    double[][] delta = new double[m_input_len][];
                    double sum = 0;
                    for (int j = 0; j < m_input_len; ++j)
                    {
                        delta[j] = new double[l_output_len];
                        string word_input = input_lang[j];
                        for (int a_j = 0; a_j < l_output_len; ++a_j)
                        {
                            string word_output = output_lang[a_j];
                            double t = model.TranslationMap.GetProbabilityInputWordGivenOutputWord(word_input, word_output);
                            double q = model.DistortionMap.GetDistortionParameter(a_j, j, m_input_len, l_output_len);
                            double deltaVal = t * q;
                            delta[j][a_j] = deltaVal;
                            sum += deltaVal;
                        }
                    }

                    for (int j = 0; j < m_input_len; ++j)
                    {
                        for (int a_j = 0; a_j < l_output_len; ++a_j)
                        {
                            delta[j][a_j] /= sum;
                        }
                    }

                    for (int j = 0; j < m_input_len; ++j)
                    {
                        for (int a_j = 0; a_j < l_output_len; ++a_j)
                        {
                            double deltaVal = delta[j][a_j];

                            string word_input = input_lang[j];
                            string word_output = output_lang[a_j];

                            double count_input_output = model.TranslationMap.GetCountOfInputWordAndOutputWord(word_input, word_output);
                            double count_output = model.TranslationMap.GetCountOfOutputWord(word_output);

                            model.TranslationMap.SetCountOfInputWordAndOutputWord(word_input, word_output, count_input_output + deltaVal);
                            model.TranslationMap.SetCountOfOutputWord(word_output, count_output + deltaVal);

                            model.TranslationMap.UpdateProbabilityInputWordGivenOutputWord(word_input, word_output);

                            double count_j_given_aj_l_m = model.DistortionMap.GetCountOfAlignmentIndexGivenAlignmentValueAndInputLenAndOutputLen(j, a_j, l_output_len, m_input_len);
                            double count_aj_l_m = model.DistortionMap.GetCountOfAlignmentValueAndInputLenAndOutputLen(a_j, l_output_len, m_input_len);

                            model.DistortionMap.SetCountOfAlignmentIndexGivenAlignmentValueAndInputLenAndOutputLen(j, a_j, l_output_len, m_input_len, count_j_given_aj_l_m + deltaVal);
                            model.DistortionMap.SetCountOfAlignmentValueAndInputLenAndOutputLen(a_j, l_output_len, m_input_len, count_aj_l_m + deltaVal);

                            model.DistortionMap.UpdateDistortionParameter(a_j, j, m_input_len, l_output_len);
                        }
                    }
                }
            }
        }

        public static void Initialize(IBMModel1 model, IEnumerable<EMTrainingRecord> training_corpus)
        {
            foreach (EMTrainingRecord rec in training_corpus)
            {
                string[] input_lang = rec.InputLang;
                string[] output_lang = rec.OutputLang;

                int m_input_len = input_lang.Length;
                int l_output_len = output_lang.Length;

                for (int j = 0; j < m_input_len; ++j)
                {
                    for (int a_j = 0; a_j < l_output_len; ++a_j)
                    {
                        string word_input = input_lang[j];
                        string word_output = output_lang[a_j];

                        double count_input_output = model.TranslationMap.GetCountOfInputWordAndOutputWord(word_input, word_output);
                        double count_output = model.TranslationMap.GetCountOfOutputWord(word_output);

                        model.TranslationMap.SetProbabilityInputWordGivenOutputWord(word_input, word_output, mRandom.NextDouble());
                    }
                }
            }
        }

        public static void Train(IBMModel1 model, IEnumerable<EMTrainingRecord> training_corpus, int maxIterations)
        {
            for (int iteration = 0; iteration < maxIterations; ++iteration)
            {

                foreach (EMTrainingRecord rec in training_corpus)
                {
                    string[] input_lang = rec.InputLang;
                    string[] output_lang = rec.OutputLang;

                    int m_input_len = input_lang.Length;
                    int l_output_len = output_lang.Length;

                    double[][] delta = new double[m_input_len][];
                    double sum = 0;
                    for (int j = 0; j < m_input_len; ++j)
                    {
                        delta[j] = new double[l_output_len];
                        string word_input = input_lang[j];
                        for (int a_j = 0; a_j < l_output_len; ++a_j)
                        {
                            string word_output = output_lang[a_j];
                            double t = model.TranslationMap.GetProbabilityInputWordGivenOutputWord(word_input, word_output);
                           
                            double deltaVal = t;
                            delta[j][a_j] = deltaVal;
                            sum += deltaVal;
                        }
                    }

                    for (int j = 0; j < m_input_len; ++j)
                    {
                        for (int a_j = 0; a_j < l_output_len; ++a_j)
                        {
                            delta[j][a_j] /= sum;
                        }
                    }

                    for (int j = 0; j < m_input_len; ++j)
                    {
                        for (int a_j = 0; a_j < l_output_len; ++a_j)
                        {
                            double deltaVal = delta[j][a_j];

                            string word_input = input_lang[j];
                            string word_output = output_lang[a_j];

                            double count_input_output = model.TranslationMap.GetCountOfInputWordAndOutputWord(word_input, word_output);
                            double count_output = model.TranslationMap.GetCountOfOutputWord(word_output);

                            model.TranslationMap.SetCountOfInputWordAndOutputWord(word_input, word_output, count_input_output + deltaVal);
                            model.TranslationMap.SetCountOfOutputWord(word_output, count_output + deltaVal);

                            model.TranslationMap.UpdateProbabilityInputWordGivenOutputWord(word_input, word_output);

                           
                        }
                    }
                }
            }
        }
    }

    
}
