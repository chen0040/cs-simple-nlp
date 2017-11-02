using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.Helpers
{
    public class Constants
    {
        public static char[] DefaultSplitChars
        {
            get
            {
                return new char[15] { ' ', '\r', '\t', '\n', ',', ';', '.', ':', '[', ']', '(', ')', '{', '}', '"' };
            }
        }
    }
}
