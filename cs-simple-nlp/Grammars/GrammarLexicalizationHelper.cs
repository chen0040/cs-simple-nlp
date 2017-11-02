using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.Grammars
{
    public class GrammarLexicalizationHelper
    {
        public static void Lexicalize(IEnumerable<ProductionRule> rules)
        {
            foreach (ProductionRule rule in rules)
            {
                AddHead(rule);
            }
        }

        public static void AddHead(ProductionRule rule)
        {
            int lastNNIndex = -1;
            int firstNPIndex = -1;
            int lastJJIndex = -1;
            int lastCDIndex = -1;

            int firstViIndex = -1;
            int firstVPIndex = -1;

            for (int i = 0; i < rule.Right.Count; ++i)
            {
                string word = rule.Right[i];

                if (rule.Left == "NP")
                {
                    if (word == "NN" || word == "NNS" || word == "NNP")
                    {
                        lastNNIndex = i;
                    }
                    else if (word == "NP")
                    {
                        if (firstNPIndex == -1)
                        {
                            firstNPIndex = i;
                        }
                    }
                    else if (word == "JJ")
                    {
                        lastJJIndex = i;
                    }
                    else if (word == "CD")
                    {
                        lastCDIndex = i;
                    }
                }
                else if (rule.Left == "VP")
                {
                    if (word == "Vi" || word == "Vt")
                    {
                        if (firstViIndex == -1)
                        {
                            firstViIndex = i;
                        }
                    }
                    else if (word == "VP")
                    {
                        if (firstVPIndex == -1)
                        {
                            firstVPIndex = i;
                        }
                    }
                }
                else if (rule.Left == "S")
                {
                    if (word == "VP")
                    {
                        if (firstVPIndex == -1)
                        {
                            firstVPIndex = i;
                        }
                    }
                }
            }

            if (rule.Left == "NP")
            {
                if (lastNNIndex != -1)
                {
                    rule.LexHeadIndex = lastNNIndex;
                }
                else if (firstNPIndex != -1)
                {
                    rule.LexHeadIndex = firstNPIndex;
                }
                else if (lastJJIndex != -1)
                {
                    rule.LexHeadIndex = lastJJIndex;
                }
                else if (lastCDIndex != -1)
                {
                    rule.LexHeadIndex = lastCDIndex;
                }
                else
                {
                    rule.LexHeadIndex = rule.Right.Count - 1;
                }
            }
            else if (rule.Left == "VP")
            {
                if (firstViIndex != -1)
                {
                    rule.LexHeadIndex = firstViIndex;
                }
                else if (firstVPIndex != -1)
                {
                    rule.LexHeadIndex = firstVPIndex;
                }
                else
                {
                    rule.LexHeadIndex = 0;
                }
            }
            else if (rule.Left == "S")
            {
                if (firstVPIndex != -1)
                {
                    rule.LexHeadIndex = firstVPIndex;
                }
            }
        }

        public static void ConvertToLexicalizedGrammar(string[] words, List<ProductionRule> rules, List<string> nonTerminals, List<string> startingSymbols)
        {
            List<ProductionRule> unitProductionRules = new List<ProductionRule>();
            List<ProductionRule> otherProductionRules = new List<ProductionRule>();
            foreach (ProductionRule rule in rules)
            {
                if (rule.IsUnitProductionRule)
                {
                    unitProductionRules.Add(rule);
                }
                else if (rule.Right.Count <= 3)
                {
                    otherProductionRules.Add(rule);
                }
                else
                {
                    throw new NotImplementedException("Current version does not support rule with more than 3 outputs");
                }
            }
            rules.Clear();

            int n = words.Length;
            HashSet<string> uniqueWords = new HashSet<string>();
            for (int i = 0; i < n; ++i)
            {
                uniqueWords.Add(words[i]);
            }
            string[] uniqueWordArray = uniqueWords.ToArray();

            List<string> tempStartingSymbols = new List<string>();
            foreach (string word in startingSymbols)
            {
                foreach (string word2 in uniqueWordArray)
                {
                    tempStartingSymbols.Add(string.Format("{0}({1})", word, word2));
                }
            }
            startingSymbols.Clear();
            foreach(string word in tempStartingSymbols)
            {
                startingSymbols.Add(word);
            }

            HashSet<string> tempNonTerminals = new HashSet<string>();
            nonTerminals.Clear();

            foreach (ProductionRule rule in unitProductionRules)
            {
                string left = rule.Left;
                string right = rule.Right[0];

                string left1= string.Format("{0}({1})", left, right);
                rule.SetLeft(new GrammarTreeNode(left1, false));
                rules.Add(rule);

                tempNonTerminals.Add(left1);
            }

            foreach (ProductionRule rule in otherProductionRules)
            {
                int outputCount = rule.Right.Count;
                if (outputCount == 2)
                {
                    string right1 = rule.Right[0];
                    string right2 = rule.Right[1];
                    string left = rule.Left;
                    int lexHeadIndex = rule.LexHeadIndex;

                    for (int i = 0; i < uniqueWordArray.Length; ++i)
                    {
                        string right11 = string.Format("{0}({1})", right1, uniqueWordArray[i]);
                        tempNonTerminals.Add(right11);
                        for (int j = 0; j < uniqueWordArray.Length; ++j)
                        {
                            string right22 = string.Format("{0}({1})", right2, uniqueWordArray[j]);
                            tempNonTerminals.Add(right22);

                            string left2 = string.Format("{0}({1})", left, uniqueWordArray[i]);
                            if (lexHeadIndex == 1)
                            {
                                left2 = string.Format("{0}({1})", left, uniqueWordArray[j]);   
                            }

                            tempNonTerminals.Add(left2);

                            ProductionRule newRule = new ProductionRule(left2, right11, right22);
                            rules.Add(newRule);
                        }
                    }
                    
                }
            }

            nonTerminals.Clear();
            foreach (string word in tempNonTerminals)
            {
                nonTerminals.Add(word);
            }
            
        }
    }
}
