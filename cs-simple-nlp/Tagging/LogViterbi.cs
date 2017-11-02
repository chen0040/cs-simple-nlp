using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.Tagging
{
    public class LogViterbi
    {
        /// <summary>
        /// trigram forward
        /// </summary>
        /// <param name="S"></param>
        /// <param name="observations"></param>
        /// <param name="log_conditional_probability"></param>
        /// <param name="logLikelihood"></param>
        /// <returns></returns>
        public static int[] Forward(int S, int[] observations, double[][][][] log_conditional_probability, out double logLikelihood)
        {
            int K = observations.Length;

            double[][][] log_pi = new double[K][][];
            int[][][] bp = new int[K][][];

            for (int k = 0; k < K; ++k)
            {
                log_pi[k] = new double[S][];
                bp[k] = new int[S][];
                for (int s = 0; s < S; ++s)
                {
                    log_pi[k][s] = new double[S];
                    bp[k][s] = new int[S];
                }
            }

            for (int k = 0; k < K; ++k)
            {
                Compute(k, S, observations, log_pi, bp, log_conditional_probability);
            }

            int maxState_Km1 = 0;
            int maxState_Km2 = 0;
            double maxWeight = double.MinValue;
            for (int u = 0; u < S; ++u)
            {
                for (int v = 0; v < S; ++v)
                {
                    if (log_pi[K - 1][u][v] > maxWeight)
                    {
                        maxWeight = log_pi[K - 1][u][v];
                        maxState_Km1 = v;
                        maxState_Km2 = u;
                    }
                }
            }

            logLikelihood = maxWeight;

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

        public static int[] Forward(int S, int[] observations, double[][] log_conditional_probability, out double logLikelihood)
        {
            int K = observations.Length;

            double[][] log_pi = new double[K][];
            int[][] bp = new int[K][];

            for (int k = 0; k < K; ++k)
            {
                log_pi[k] = new double[S];
                bp[k] = new int[S];
            }

            for (int k = 0; k < K; ++k)
            {
                Compute(k, S, observations, log_pi, bp, log_conditional_probability);
            }

            int maxState_Km1 = 0;
            double maxWeight = double.MinValue;
            for (int v = 0; v < S; ++v)
            {
                if (log_pi[K - 1][v] > maxWeight)
                {
                    maxWeight = log_pi[K - 1][v];
                    maxState_Km1 = v;
                }
            }

            logLikelihood = maxWeight;

            int[] path = new int[K];
            path[K - 1] = maxState_Km1;
            for (int k = K - 2; k >= 0; --k)
            {
                int v = path[k + 1];
                path[k] = bp[k + 1][v]; //return state at s since bp[s][v] stores state at s-1
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
        /// log(p(v | u, v))
        /// This is the conditional probability that y(s) = v given y(s-2) = u and y(s-1) = v
        /// </param>
        private static void Compute(int k, int S, int[] observations, double[][][] log_pi, int[][][] bp, double[][][][] log_conditional_probability)
        {
            int x_k = observations[k]; //observation at time step s

            for (int v = 0; v < S; ++v)
            {
                for (int u = 0; u < S; ++u)
                {
                    double log_pi_k_v_given_u = double.MinValue;
                    int selected_w = -1;
                    for (int w = 0; w < S; ++w)
                    {
                        double log_pi_km1_u_w = 0;
                        if (k > 0) // log_pi(s=-1, *, *) = 0, that is log_pi(s=-1, *, *) = 1
                        {
                            log_pi_km1_u_w = log_pi[k - 1][w][u];
                        }

                        double log_pi_k_u_v_given_w = log_pi_km1_u_w + log_conditional_probability[w][u][v][k];

                        if (log_pi_k_u_v_given_w > log_pi_k_v_given_u)
                        {
                            log_pi_k_v_given_u = log_pi_k_u_v_given_w;
                            selected_w = w;
                        }
                    }
                    log_pi[k][u][v] = log_pi_k_v_given_u;
                    bp[k][u][v] = selected_w; //bp[s][v][v] stores state at s-2
                }
            }


        }

        /// <summary>
        /// bigram
        /// </summary>
        /// <param name="k"></param>
        /// <param name="S"></param>
        /// <param name="observations"></param>
        /// <param name="log_pi"></param>
        /// <param name="bp"></param>
        /// <param name="log_conditional_probability"></param>
        private static void Compute(int k, int S, int[] observations, double[][] log_pi, int[][] bp, double[][] log_conditional_probability)
        {
            int x_k = observations[k]; //observation at time step k

            for (int v = 0; v < S; ++v)
            {
                double log_pi_k_v = double.MinValue;
                int selected_u = -1;
                for (int u = 0; u < S; ++u)
                {
                    double log_pi_km1_u = 0;
                    if (k > 0) // log_pi(k=-1, *) = 0, that is log_pi(k=-1, *) = 1
                    {
                        log_pi_km1_u = log_pi[k - 1][u];
                    }

                    double log_pi_k_v_given_u = log_pi_km1_u + log_conditional_probability[u][v];

                    if (log_pi_k_v_given_u > log_pi_k_v)
                    {
                        log_pi_k_v = log_pi_k_v_given_u;
                        selected_u = u;
                    }
                }
                log_pi[k][v] = log_pi_k_v;
                bp[k][v] = selected_u; //bp[k][v] stores state at k-1
            }


        }
    }
}
