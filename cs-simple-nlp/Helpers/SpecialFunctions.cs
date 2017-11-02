using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.Helpers
{
    public class SpecialFunctions
    {
        public static double LogSumExp(double[] values)
        {
            int count = values.Length;
            if (count == 0)
            {
                return double.NegativeInfinity;
            }

            double A = values.Max();

            if (A == double.NegativeInfinity)
            {
                return double.NegativeInfinity;
            }

            double sum = 0;
            
            for (int i = 0; i < count; ++i)
            {
                sum += System.Math.Exp(values[i] - A);
            }
            return A + System.Math.Log(sum);
        }
    }
}
