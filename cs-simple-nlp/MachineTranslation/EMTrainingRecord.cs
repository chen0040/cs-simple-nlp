using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.MachineTranslation
{
    public class EMTrainingRecord
    {
        protected string[] mInputLang;
        protected string[] mOutputLang;

        public string[] InputLang
        {
            get { return mInputLang; }
            set { mInputLang = value; }
        }

        public string[] OutputLang
        {
            get { return mOutputLang; }
            set { mOutputLang = value; }
        }
    }
}
