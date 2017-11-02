using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.Grammars
{
    public class CFGParser : CFG, IParser
    {
        public bool IsInLanguage(string sentence)
        {
            string[] words = sentence.Split(new char[] { ' ' });
            return CKY.IsInLanguage(words, mRules, mNonTerminals, mStartingSymbols);
        }

        
        public virtual GrammarTreeNode Parse(string sentence)
        {
            string[] words = sentence.Split(new char[] { ' ' });
            List<GrammarTreeNode> trees = CKY.ParseCFG(words, mRules, mNonTerminals, mStartingSymbols);

            if (trees.Count > 0)
            {
                return trees[0];
            }

            return null;
        }

        

        
    }
}
