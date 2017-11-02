using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.LanguageModels
{
    public class Perceptron<XDataType, YDataType>
    {
        protected double[] mTheta;
        protected int mMaxIterations = 20;
        public delegate List<YDataType> GENHandle(XDataType x);

        public int DimensionCount
        {
            get { return mModelFeatureFunction.Dimension; }
        }

        protected GENHandle mGEN;
        public GENHandle GENMethod
        {
            get { return mGEN; }
            set { mGEN = value; }
        }

        protected IFeatureFunction<XDataType, YDataType, int> mModelFeatureFunction;
        public IFeatureFunction<XDataType, YDataType, int> FeatureFunction
        {
            get { return mModelFeatureFunction; }
            set { mModelFeatureFunction = value; }
        }

        public int MaxIterations
        {
            get { return mMaxIterations; }
            set { mMaxIterations = value; }
        }

        public double[] TrainedParameters
        {
            get { return mTheta; }
        }

        public Perceptron()
        {
            
        }

        private static Random mRnd = new Random();

        public virtual void Train(IList<PairedDataInstance<XDataType, YDataType>> training_data)
        {
            int n = training_data.Count;
            mTheta = new double[DimensionCount];
            for (int i = 0; i < DimensionCount; ++i)
            {
                mTheta[i] = mRnd.NextDouble();
            }
            for (int t = 0; t < mMaxIterations; ++t)
            {
                for (int i = 0; i < n; ++i)
                {
                    PairedDataInstance<XDataType, YDataType> rec = training_data[i];
                    XDataType x_i = rec.Entry;
                    YDataType y_i = training_data[i].Label;
                    YDataType z_i = F(x_i, mTheta);
                    if (!z_i.Equals(y_i))
                    {
                        List<double> yVals = GetFScores(x_i, y_i);
                        List<double> zVals = GetFScores(x_i, z_i);
                        for (int d = 0; d < DimensionCount; ++d)
                        {
                            mTheta[d] = mTheta[d] + yVals[d] - zVals[d];
                        }
                    }
                }
            }
        }

        protected virtual List<double> GetFScores(XDataType x, YDataType y)
        {
            throw new NotImplementedException();
        }

        protected virtual YDataType F(XDataType x, double[] theta)
        {
            List<YDataType> generated_candidates = mGEN(x);
            double val_max = double.MinValue;
            int selectedIndex = -1;
            for (int i = 0; i < generated_candidates.Count; ++i)
            {
                YDataType y_pi = generated_candidates[i];
                double val_pi = 0;
                List<double> yVals = GetFScores(x, y_pi);
                for (int d = 0; d < DimensionCount; ++d)
                {
                    val_pi += theta[d] * yVals[d]; 
                }
                if (val_pi > val_max)
                {
                    val_max = val_pi;
                    selectedIndex = i;
                }
            }

            return generated_candidates[selectedIndex];
        }
    }
}
