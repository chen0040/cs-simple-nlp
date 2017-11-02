using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FeedbackControl.Helpers;

namespace FeedbackControl.Helpers
{
    public class BinaryHeapMaxPQ<T> : IMaxPQ<T>
    {
        protected T[] mData = null;
        protected int mCount = 0;

        public event SortUtil.CompareToHandler<T> CompareKeys;

        public void Clear()
        {
            mCount = 0;
            for (int i = 0; i < mData.Length; ++i)
            {
                mData[i] = default(T);
            }
        }

        protected int CompareTo(int k1, int k2)
        {
            return CompareTo(mData[k1 - 1], mData[k2 - 1]);
        }

        protected int CompareTo(T t1, T t2)
        {
            if (CompareKeys != null)
            {
                return CompareKeys(t1, t2);
            }
            else if (t1 is IComparable && t2 is IComparable)
            {
                IComparable c1 = t1 as IComparable;
                IComparable c2 = t2 as IComparable;
                return c1.CompareTo(c2);
            }
            else
            {
                return t1.GetHashCode().CompareTo(t2.GetHashCode());
            }
        }

        public BinaryHeapMaxPQ(int capacity = 1)
        {
            mData = new T[capacity];
        }

        public int Count
        {
            get { return mCount; }
        }

        public bool IsEmpty
        {
            get { return mCount == 0; }
        }

        public void Insert(T item)
        {
            mData[mCount++] = item;

            if (mCount == mData.Length)
            {
                Resize(mData.Length * 2);
            }

            Swim(mCount);
        }

        protected void Swim(int k)
        {
            while (k > 1 && CompareTo(k, k / 2) > 0)
            {
                Exchange(mData, k, k / 2);
                k = k / 2;
            }

        }

        protected void Sink(int k)
        {
            while (k * 2 <= mCount)
            {
                int j = k * 2;
                if (j + 1 <= mCount && CompareTo(j+1, j) > 0)
                {
                    j++;
                }
                if (CompareTo(j, k) <= 0)
                {
                    break;
                }
                Exchange(mData, k, j);
                k = j;
            }
        }

        protected void Exchange(T[] a, int k1, int k2)
        {
            SortUtil.Swap(a, k1-1, k2-1);
        }

        protected void Resize(int capacity)
        {
            T[] s = new T[capacity];
            for (int i = 0; i < mCount; ++i)
            {
                s[i] = mData[i];
            }

            mData = s;
        }

        public T DeleteMax()
        {
            T maxItem = mData[0];
            Exchange(mData, 1, mCount);
            mData[mCount - 1] = default(T);
            mCount--;
            Sink(1);

            if (mCount < mData.Length / 4)
            {
                Resize(mData.Length / 2);
            }

            return maxItem;
        }
    }
}
