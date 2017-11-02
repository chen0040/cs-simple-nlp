using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MyNLP.Grammars
{
    public class LexicalizedPCFGParser : LexicalizedPCFG, IParser
    {
        public bool IsInLanguage(string sentence)
        {
            string[] words = sentence.Split(new char[] { ' ' });
            return CKY.IsInLanguage(words, mRules, mNonTerminals, mStartingSymbols);
        }

        public virtual GrammarTreeNode Parse(string sentence)
        {
            string[] words = sentence.Split(new char[] { ' ' });

            List<ProductionRule> lexicalizedRules = new List<ProductionRule>();
            foreach (ProductionRule rule in mRules)
            {
                lexicalizedRules.Add(rule);
            }

            List<string> lexicalizedNonTerminals = new List<string>();
            foreach (string word in mNonTerminals)
            {
                lexicalizedNonTerminals.Add(word);
            }

            List<string> lexicalizedStartSymbols = new List<string>();
            foreach (string word in mStartingSymbols)
            {
                lexicalizedStartSymbols.Add(word);
            }

            GrammarLexicalizationHelper.ConvertToLexicalizedGrammar(words, lexicalizedRules, lexicalizedNonTerminals, lexicalizedStartSymbols);

            return CKY.ParsePCFG(words, lexicalizedRules, lexicalizedNonTerminals, lexicalizedStartSymbols);
        }


        public void LexicalizedRules()
        {
            GrammarLexicalizationHelper.Lexicalize(mRules);
        }
    }
}
