using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContinuousOptimization.LocalSearch;
using ContinuousOptimization;
using System.Xml;
using MyNLP.Helpers;

namespace MyNLP.LanguageModels
{
    public class LogLinearModel<XDataType, YDataType>
    {
        protected SingleTrajectoryContinuousSolver mLocalSearcher = new ConjugateGradientSearch(); // BFGS();
        protected int mMaxSolverIteration = 20;
        protected double[] mTheta = null;

        protected double[] mThetaUpperBounds = null;
        protected double[] mThetaLowerBounds = null;

        private LogLinearModelCostFunction<XDataType, YDataType> mCostFunction;

        public double[] ParameterUpperBounds
        {
            get { return mThetaUpperBounds; }
            set { mThetaUpperBounds = value; }
        }

        public double[] ParameterLowerBounds
        {
            get { return mThetaLowerBounds; }
            set { mThetaLowerBounds = value; }
        }

        protected IFeatureFunction<XDataType, YDataType, int> mModelFeatureFunction = null;
        public IFeatureFunction<XDataType, YDataType, int> ModelFeatureFunction
        {
            get { return mModelFeatureFunction; }
            set { mModelFeatureFunction = value; }
        }

        protected IList<YDataType> mYDomain = null;
        public IList<YDataType> YDomain
        {
            get { return mYDomain; }
        }

        protected double mRegularizationLambda = 0;

        public double RegularizationLambda
        {
            get { return mRegularizationLambda; }
            set { mRegularizationLambda = value; }
        }

        public SingleTrajectoryContinuousSolver LocalSearch
        {
            get { return mLocalSearcher; }
            set { mLocalSearcher = value; }
        }

        public int MaxLocalSearchIteration
        {
            get { return mMaxSolverIteration; }
            set { mMaxSolverIteration = value; }
        }

        /// <summary>
        /// Get the conditional probability of y given x
        /// </summary>
        /// <param name="yVal"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public double GetLogConditionalProbability(YDataType y, XDataType x_input)
        {
            List<PairedDataInstance<XDataType, YDataType>> records = new List<PairedDataInstance<XDataType, YDataType>>()
                {
                    new PairedDataInstance<XDataType, YDataType>()
                    {
                        Entry = x_input,
                        Label = y
                    }
                };

            LogLinearModelCostFunction<XDataType, YDataType> f = new LogLinearModelCostFunction<XDataType, YDataType>(records, mModelFeatureFunction, mYDomain, mRegularizationLambda);
            if (mThetaLowerBounds != null) mCostFunction.LowerBounds = mThetaLowerBounds;
            if (mThetaUpperBounds != null) mCostFunction.UpperBounds = mThetaUpperBounds;

            double cost = f.Evaluate(mTheta);

            double log_probability_of_y_given_x = -cost;

            return log_probability_of_y_given_x;
        }

        public YDataType Predict(XDataType x_input)
        {
            double maxProb = double.MinValue;
            YDataType selectedY = default(YDataType);
            foreach(YDataType y in mYDomain)
            {
                double probability_of_y_given_x = GetLogConditionalProbability(y, x_input);
                if (maxProb < probability_of_y_given_x)
                {
                    maxProb = probability_of_y_given_x;
                    selectedY = y;
                }
            }
            return selectedY;
        }

        public virtual void Train(IList<PairedDataInstance<XDataType, YDataType>> training_data)
        {
            HashSet<YDataType> uniqueYValues = new HashSet<YDataType>();
            foreach (PairedDataInstance<XDataType, YDataType> rec in training_data)
            {
                uniqueYValues.Add(rec.Label);
            }
            mYDomain = uniqueYValues.ToList();

            mCostFunction = new LogLinearModelCostFunction<XDataType, YDataType>(training_data, mModelFeatureFunction, mYDomain, mRegularizationLambda);
            if (mThetaLowerBounds != null) mCostFunction.LowerBounds = mThetaLowerBounds;
            if (mThetaUpperBounds != null) mCostFunction.UpperBounds = mThetaUpperBounds;

           
            double[] theta_0 = new double[mCostFunction.DimensionCount];
            for (int d = 0; d < mCostFunction.DimensionCount; ++d)
            {
                theta_0[d] = 0;
            }
            ContinuousSolution solution = mLocalSearcher.Minimize(theta_0, mCostFunction, mMaxSolverIteration);

            mTheta = solution.Values;
        }

        protected virtual YDataType ToYValue(string yValue)
        {
            throw new NotImplementedException();
        }

       
    }
}
