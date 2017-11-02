using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.Helpers
{
    public interface ILongProcess
    {
        void NotifyLongProcess(string message);

        double LongProcessNotificationIntervalInSeconds
        {
            get;
            set;
        }
    }
}
