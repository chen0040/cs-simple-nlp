using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MyNLP.Grammars
{
    public class PCFGParser : PCFG, IParser
    {
        public bool IsInLanguage(string sentence)
        {
            string[] words = sentence.Split(new char[] { ' ' });
            return CKY.IsInLanguage(words, mRules, mNonTerminals, mStartingSymbols);
        }

        public virtual GrammarTreeNode Parse(string sentence)
        {
            string[] words = sentence.Split(new char[] { ' ' });
            return CKY.ParsePCFG(words, mRules, mNonTerminals, mStartingSymbols);
        }

    }
}
