using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyNLP.LanguageModels;

namespace MyNLP.HMM
{
    /// <summary>
    /// Assume for TriGram HMM
    /// </summary>
    /// <typeparam name="HiddenState"></typeparam>
    /// <typeparam name="O"></typeparam>
    public class SmoothEstimation<HiddenState, O>
    {
        protected double mLambda1;
        protected double mLambda2;
        protected double mLambda3;
        protected NGramModel<HiddenState> mTriGramModel = new NGramModel<HiddenState>(3);
        protected NGramModel<HiddenState> mBiGramModel = new NGramModel<HiddenState>(2);
        protected NGramModel<HiddenState> mUniGramModel = new NGramModel<HiddenState>(1);
        protected int mWordCount = 0;
        protected Dictionary<string, int> mStateObservationJoinCounts = new Dictionary<string, int>();
        protected Dictionary<HiddenState, int> mHiddenStateCounts = new Dictionary<HiddenState, int>();

        public void Parse(HiddenState[] hidden_states, O[] observations)
        {
            ParseEmission(hidden_states, observations);
            ParseTransition(hidden_states);
        }

        protected string CreateStateObservationId(O x, HiddenState y)
        {
            return string.Format("{0}|{1}", x, y); 
        }

        protected void ParseEmission(HiddenState[] hidden_states, O[] observations)
        {
            int count = hidden_states.Length;
            for (int i = 0; i < count; ++i)
            {
                HiddenState y = hidden_states[i];
                O x = observations[i];
                string x_given_y = CreateStateObservationId(x, y);
                if (mStateObservationJoinCounts.ContainsKey(x_given_y))
                {
                    mStateObservationJoinCounts[x_given_y] += 1;
                }
                else
                {
                    mStateObservationJoinCounts[x_given_y] = 1;
                }
                if (mHiddenStateCounts.ContainsKey(y))
                {
                    mHiddenStateCounts[y] += 1;
                }
                else
                {
                    mHiddenStateCounts[y] = 1;
                }
            }
        }

        protected void ParseTransition(HiddenState[] hidden_states)
        {
            mUniGramModel.Parse4Gram(hidden_states);
            mBiGramModel.Parse4Gram(hidden_states);
            mTriGramModel.Parse4Gram(hidden_states);
            mWordCount = hidden_states.Length;
        }

        /// <summary>
        /// Calculate the conditional probability of hidden state transition
        /// </summary>
        /// <param name="ym2">state y(t-2)</param>
        /// <param name="ym1">state y(t-1)</param>
        /// <param name="y">state y(t)</param>
        /// <returns>Probability(y(t) | y(t-1), y(t-2))</returns>
        public double CalcTransitionProbability(HiddenState ym2, HiddenState ym1, HiddenState y)
        {
            string y_state = y.ToString();
            string given_ym1 = NGram<HiddenState>.CreateEvidenceSignature(new HiddenState[] { ym1});
            string given_ym1_ym2 = NGram<HiddenState>.CreateEvidenceSignature(new HiddenState[] {ym2, ym1});
            
            NGram<HiddenState> triGram = mTriGramModel.FindNGram(given_ym1_ym2, y_state);
            NGram<HiddenState> biGram = mBiGramModel.FindNGram(given_ym1, y_state);
            NGram<HiddenState> uniGram = mUniGramModel.FindNGram(null, y_state);

            double q1 = triGram.ConditionalProbability;
            double q2 = biGram.ConditionalProbability;
            double q3 = (double)uniGram.JoinSupportLevel / mWordCount; 
            double qy = mLambda1 * q1 + mLambda2 * q2 + mLambda3 *  q3;

            return qy;
        }

        /// <summary>
        /// Calculate the emission probability log_pi(node | y) while node is the observed and y is the hidden state
        /// </summary>
        /// <param name="y">y, hidden state</param>
        /// <param name="node">node, observed data</param>
        /// <returns>Probability(node | y)</returns>
        public double CalcEmissionProbability(HiddenState y, O x)
        {
            string x_given_y = CreateStateObservationId(x, y);
            int count_x_given_y = 0;
            int count_y = 0;
            if (mHiddenStateCounts.ContainsKey(y))
            {
                count_y = mHiddenStateCounts[y];
            }
            if (count_y == 0) return 0;
            if (mStateObservationJoinCounts.ContainsKey(x_given_y))
            {
                count_x_given_y = mStateObservationJoinCounts[x_given_y];
            }
            return count_x_given_y / count_y;
        }
    }
}
