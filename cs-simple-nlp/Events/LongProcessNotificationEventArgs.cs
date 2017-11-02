using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.Events
{
    public class LongProcessNotificationEventArgs : EventArgs
    {
        protected string mMessage;
        public LongProcessNotificationEventArgs(string message)
        {
            mMessage = message;
        }

        public string Message
        {
            get { return mMessage; }
        }
    }
}
