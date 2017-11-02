using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackControl.Helpers
{
    public class SortUtil
    {
        public delegate int CompareToHandler<T>(T a, T b);
        public static void Swap<T>(T[] a, int i, int j)
        {
            T temp = a[i];
            a[i] = a[j];
            a[j] = temp;
        }
    }
}
