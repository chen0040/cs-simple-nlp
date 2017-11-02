using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.HMM
{
    public class Viterbi
    {
        public int[] Forward(int S, int[] observations, double[][][] transitions, double[][] emissions, out double logLikelihood)
        {
            int K = observations.Length;

            double[][][] pi = new double[K][][];
            int[][][] bp = new int[K][][];

            for (int k = 0; k < K; ++k)
            {
                pi[k] = new double[S][];
                bp[k] = new int[S][];
                for (int s = 0; s < S; ++s)
                {
                    pi[k][s] = new double[S];
                    bp[k][s] = new int[S];
                }
            }

            for (int k = 0; k < K; ++k)
            {
                Compute(k, S, observations, pi, bp, transitions, emissions);
            }

            int maxState_Km1 = 0;
            int maxState_Km2 = 0;
            double maxWeight = double.MinValue;
            for (int u = 0; u < S; ++u)
            {
                for (int v = 0; v < S; ++v)
                {
                    if (pi[K - 1][u][v] > maxWeight)
                    {
                        maxWeight = pi[K - 1][u][v];
                        maxState_Km1 = v;
                        maxState_Km2 = u;
                    }
                }
            }

            logLikelihood = System.Math.Log(maxWeight);

            int[] path = new int[K];
            path[K - 1] = maxState_Km1;
            path[K - 2] = maxState_Km2;
            for (int k = K - 3; k >= 0; --k)
            {
                int u = path[k + 1];
                int v = path[k + 2];
                path[k] = bp[k + 2][u][v]; //return state at s since bp[s][v][v] stores state at s-2
            }

            return path;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s">kth iteration</param>
        /// <param name="S">Total number of distinct states</param>
        /// <param name="observations">The observation sequence</param>
        /// <param name="log_pi">
        /// Joint Probability that node(s) = observations[s] and y(s-1) = v and y(s) = v
        /// 
        /// This is what happens in the algorithm:
        /// As input taken:
        /// log_pi(s-1, u, v)
        /// where u is the hidden state at s-2;
        /// where v is the hidden state at s-1;
        /// 
        /// As output added:
        /// log_pi(s, v, v)
        /// where v is the hidden state at s-1;
        /// where v is the hidden state at s;
        /// </param>
        /// <param name="bp">
        /// Backpointers, bp[s][v][v] stores the state u at s-2
        /// </param>
        /// <param name="transitions">
        /// q(v | u, v)
        /// This is the transition probability that y(s) = v given y(s-2) = u and y(s-1) = v
        /// </param>
        /// <param name="emissions">
        /// e(x_k | v)
        /// This is the emission probability that node(s) = observations[s] given y(s) = v
        /// </param>
        protected void Compute(int k, int S, int[] observations, double[][][] pi, int[][][] bp, double[][][] transitions, double[][] emissions)
        {
            int x_k = observations[k]; //observation at time step s
            
            for (int v = 0; v < S; ++v)
            {
                double emission_x_given_v = emissions[v][x_k];
                for (int u = 0; u < S; ++u)
                {
                    double pi_k_v_given_u = double.MinValue;
                    int selected_w = -1;
                    for (int w = 0; w < S; ++w)
                    {
                        double pi_km1_u_w = 1;
                        if (k > 0) // log_pi(s=-1, *, *) = 1 
                        {
                            pi_km1_u_w = pi[k - 1][w][u];
                        }

                        double pi_k_u_v_given_w = pi_km1_u_w * transitions[w][u][v] * emission_x_given_v;

                        if (pi_k_u_v_given_w > pi_k_v_given_u)
                        {
                            pi_k_v_given_u = pi_k_u_v_given_w;
                            selected_w = w;
                        }
                    }
                    pi[k][u][v] = pi_k_v_given_u;
                    bp[k][u][v] = selected_w; //bp[s][v][v] stores state at s-2
                }
            }

            
        }
    }
}
