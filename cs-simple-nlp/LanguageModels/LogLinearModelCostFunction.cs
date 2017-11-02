using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContinuousOptimization.ProblemModels;
using System.Threading.Tasks;
using MyNLP.Events;
using MyNLP.Helpers;
using System.Xml;
using System.Diagnostics;

namespace MyNLP.LanguageModels
{
    public class LogLinearModelCostFunction<XDataType, YDataType> : CostFunction, ILongProcess
    {
        protected int mTrainingDataCount;
        
        protected int mYDomainCount;
        protected double mRegularizationLambda;

        protected int[][] mFeatureCache1 = null;
        protected int[][][] mFeatureCache2 = null;

        public event EventHandler<LongProcessNotificationEventArgs> LongProcessNotified;
        public void NotifyLongProcess(string message)
        {
            if (LongProcessNotified != null)
            {
                LongProcessNotified(this, new LongProcessNotificationEventArgs(message));
            }
            else
            {
                Console.WriteLine(message);
            }
        }

        protected double mLongProcessNotificationIntervalInSeconds = 1;
        public double LongProcessNotificationIntervalInSeconds
        {
            get { return mLongProcessNotificationIntervalInSeconds; }
            set { mLongProcessNotificationIntervalInSeconds = value; }
        }

        public LogLinearModelCostFunction()
        {

        }

        public void Vaildate(int trainingDataCount, int yDomainCount, int dimensionCount, double lambda, double parameterLowerBound, double parameterUpperBound)
        {
            Debug.Assert(mTrainingDataCount == trainingDataCount);
            Debug.Assert(mYDomainCount == yDomainCount);
            Debug.Assert(mRegularizationLambda == lambda);
            Debug.Assert(mDimensionCount == dimensionCount);
            Debug.Assert(mLowerBounds[0] == parameterLowerBound);
            Debug.Assert(mUpperBounds[0] == parameterUpperBound);
        }

        public LogLinearModelCostFunction(IList<PairedDataInstance<XDataType, YDataType>> training_data, IFeatureFunction<XDataType, YDataType, int> func, IList<YDataType> yDomain, double lambda)
            : base(func.Dimension, 0, 1.0)
        {
            mTrainingDataCount = training_data.Count;
            mYDomainCount = yDomain.Count;
            mRegularizationLambda = lambda;

            mFeatureCache2 = new int[training_data.Count][][];
            for (int recIndex = 0; recIndex < training_data.Count; ++recIndex)
            {
                mFeatureCache2[recIndex] = new int[mYDomainCount][];
            }

            //DateTime currentTime = DateTime.UtcNow;
            //DateTime tickedTime = currentTime;

            TaskFactory<bool> tFactory = new TaskFactory<bool>();
            Task<bool>[] tasks = new Task<bool>[mDimensionCount];
            int onesCount = 0;
            for (int recIndex = 0; recIndex < training_data.Count; ++recIndex)
            {
                PairedDataInstance<XDataType, YDataType> rec = training_data[recIndex];

                for (int yi = 0; yi < mYDomainCount; ++yi)
                {
                    List<int> featureOneList = func.Get(rec.Entry, yDomain[yi]);
                    onesCount+=featureOneList.Count;

                    mFeatureCache2[recIndex][yi] = featureOneList.ToArray();
                }

                //currentTime = DateTime.UtcNow;
                //TimeSpan ts = currentTime - tickedTime;
                //if (ts.TotalSeconds > mLongProcessNotificationIntervalInSeconds)
                //{
                //    tickedTime = currentTime;
                //    NotifyLongProcess(string.Format("Prepare feature vector cache A: {0} / {1} (No. of 1s : {2})", recIndex, mTrainingDataCount, onesCount));
                //}
            }


            mFeatureCache1 = new int[mTrainingDataCount][];
            for (int recIndex = 0; recIndex < mTrainingDataCount; ++recIndex)
            {
                PairedDataInstance<XDataType, YDataType> rec = training_data[recIndex];

                List<int> featureOneList = func.Get(rec.Entry, rec.Label);
                onesCount += featureOneList.Count;

                mFeatureCache1[recIndex] = featureOneList.ToArray();

                //currentTime = DateTime.UtcNow;
                //TimeSpan ts = currentTime - tickedTime;
                //if (ts.TotalSeconds > mLongProcessNotificationIntervalInSeconds)
                //{
                //    tickedTime = currentTime;
                //    NotifyLongProcess(string.Format("Prepare feature vector cache B: {0} / {1} (No. of 1s : {2})", recIndex, mTrainingDataCount, onesCount));
                //}
            }
        }

        //protected int CreateHash(int d, PairedDataInstance<XDataType, YDataType> rec)
        //{
        //    int hash = rec.GetHashCode();
        //    hash = 3001 * hash + d.GetHashCode();
        //    return hash;
        //}

        public override double Evaluate(double[] theta)
        {
            //DateTime currentTime = DateTime.UtcNow;
            //DateTime tickedTime = currentTime;

            double J = 0;
            for (int recIndex = 0; recIndex < mTrainingDataCount; ++recIndex )
            {
                double L = 0;

                double[] pF_pi = new double[mYDomainCount];
                for (int yi = 0; yi < mYDomainCount; ++yi)
                {
                    double pf = 0;

                    int[] featureIndexList2 = mFeatureCache2[recIndex][yi];

                    for (int fIndex = 0; fIndex < featureIndexList2.Length; ++fIndex)
                    {
                        pf += theta[featureIndexList2[fIndex]];
                    }
                    pF_pi[yi] = pf;
                }

                
                L += SpecialFunctions.LogSumExp(pF_pi); 

                int[] featureIndexList1 = mFeatureCache1[recIndex];
                for (int fIndex = 0; fIndex < featureIndexList1.Length; ++fIndex)
                {
                    L -= theta[featureIndexList1[fIndex]];
                }

                J += L;

                //currentTime = DateTime.UtcNow;
                //TimeSpan ts = currentTime - tickedTime;
                //if (ts.TotalSeconds > mLongProcessNotificationIntervalInSeconds)
                //{
                //    tickedTime = currentTime;
                //    NotifyLongProcess(string.Format("Training by samples[{0}]", recIndex));
                //}
            }

            for (int d = 1; d < mDimensionCount; ++d)
            {
                J += (mRegularizationLambda * theta[d] * theta[d]) / (2 * mTrainingDataCount);
            }

            return J;
        }

        protected override void _CalcGradient(double[] theta, double[] grad)
        {
            double[] empirical_counts = new double[mDimensionCount]; 
            for (int recIndex = 0; recIndex < mTrainingDataCount; ++recIndex)
            {
                int[] featureIndexList1 = mFeatureCache1[recIndex];
                for (int fIndex = 0; fIndex < featureIndexList1.Length; ++fIndex)
                {
                    int dd = featureIndexList1[fIndex];
                    empirical_counts[dd] += 1;
                }
            }

            TaskFactory<double[]> tFactory1 = new TaskFactory<double[]>();
            Task<double[]>[] tasks1 = new Task<double[]>[mTrainingDataCount];
            for (int taskIndex1 = 0; taskIndex1 < mTrainingDataCount; taskIndex1++)
            {
                tasks1[taskIndex1] = tFactory1.StartNew((objs) =>
                    {
                        int recIndex = (int)objs;

                        double[] py2 = new double[mYDomainCount];

                        double sum_py = 0;
                        for (int yi = 0; yi < mYDomainCount; ++yi)
                        {
                            double pf = 0;
                            int[] featureIndexList2 = mFeatureCache2[recIndex][yi];
                            for (int fIndex = 0; fIndex < featureIndexList2.Length; ++fIndex)
                            {
                                int fId = featureIndexList2[fIndex];

                                pf += theta[fId];
                            }
                            py2[yi] = System.Math.Exp(pf);
                            sum_py += py2[yi];
                        }

                        for (int yi = 0; yi < mYDomainCount; ++yi)
                        {
                            py2[yi] /= sum_py;
                        }

                        //Console.WriteLine("Done # {0}", recIndex);

                        return py2;
                    }, taskIndex1);
            }

            Task.WaitAll(tasks1);

            double[][] py = new double[mTrainingDataCount][];
            for (int taskIndex1 = 0; taskIndex1 < mTrainingDataCount; ++taskIndex1)
            {
                py[taskIndex1] = tasks1[taskIndex1].Result;
            }

            double[] expected_counts = new double[mDimensionCount];
            TaskFactory tFactory2 = new TaskFactory();
            Task[] tasks2 = new Task[mTrainingDataCount];

            for (int taskIndex2 = 0; taskIndex2 < mTrainingDataCount; taskIndex2++)
            {

                tasks2[taskIndex2] = tFactory2.StartNew((objs) =>
                {
                    int recIndex = (int)objs;



                    for (int yi = 0; yi < mYDomainCount; ++yi)
                    {
                        int[] featureIndexList2 = mFeatureCache2[recIndex][yi];

                        for (int fIndex = 0; fIndex < featureIndexList2.Length; ++fIndex)
                        {
                            expected_counts[featureIndexList2[fIndex]] += py[recIndex][yi];
                        }
                    }

                }, taskIndex2);
            }

            Task.WaitAll(tasks2);

            for (int d = 0; d < mDimensionCount; ++d)
            {
                grad[d] = - empirical_counts[d] + expected_counts[d] + (mRegularizationLambda * theta[d]) / mTrainingDataCount;
            }
        }

        
    }
}
