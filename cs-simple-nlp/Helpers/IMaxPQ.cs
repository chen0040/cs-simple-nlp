using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeedbackControl.Helpers
{
    public interface IMaxPQ<T>
    {
        int Count
        {
            get;
        }

        bool IsEmpty
        {
            get;
        }

        void Insert(T item);
        T DeleteMax();
        void Clear();
    }
}
