using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace MyNLP.Grammars
{
    public class PennTreeBankHelper
    {
        /// <summary>
        /// Assume Penn Treebank Notation, that is:
        /// (S (NP (NNP John))
        /// (VP (VPZ loves)
        ///    (NP (NNP Mary)))
        /// (. .))
        /// </summary>
        /// <param name="filePath"></param>
        public static void LearnGrammarFromPennTreeBankFile(IGrammar grammar, string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line)) continue;
                    LearnGrammarFromPennTreeFormattedTextLine(grammar, line);
                }
            }
            
        }

        public static GrammarTreeNode ParseTree(string text)
        {
            GrammarTreeNode tree = new GrammarTreeNode();

            Stack<string> parseStack = new Stack<string>();
            Stack<GrammarTreeNode> outputStack = new Stack<GrammarTreeNode>();

            List<ProductionRule> candidateUnitProductionRules = new List<ProductionRule>();

            List<string> tokens = Tokenize(text);
            

            for (int i = 0; i < tokens.Count; ++i)
            {
                if (tokens[i] == ")")
                {
                    List<string> ruleContent = new List<string>();
                    while (true)
                    {
                        string word = parseStack.Pop();
                        if (word == "(")
                        {
                            parseStack.Push(ruleContent[ruleContent.Count - 1]);
                            break;
                        }
                        else
                        {
                            ruleContent.Add(word);
                        }
                    }

                    GrammarTreeNode parent_node = null;
                    string left = null;
                    List<string> right = new List<string>();
                    for (int j = ruleContent.Count - 1; j >= 0; --j)
                    {
                        if (left == null)
                        {
                            left = ruleContent[j];
                            parent_node = new GrammarTreeNode(left, false);
                        }
                        else
                        {
                            right.Add(ruleContent[j]);
                        }
                    }

                    if (right.Count >= 2 || (right.Count==1 && outputStack.Count > 0 && outputStack.Peek().Symbol == right[0]))
                    {
                        List<GrammarTreeNode> children = new List<GrammarTreeNode>();
                        for (int k = 0; k < right.Count; ++k)
                        {
                            children.Add(outputStack.Pop());
                        }
                        for (int k = right.Count-1; k >= 0; --k)
                        {
                            parent_node.AddChild(children[k]);
                        }
                    }
                    else
                    {
                        GrammarTreeNode leaf_node = new GrammarTreeNode(right[0], true);
                        parent_node.AddChild(leaf_node);
                    }

                    outputStack.Push(parent_node);
                }
                else
                {
                    parseStack.Push(tokens[i]);
                }
            }

            return outputStack.Pop();
        }

        public static void LearnGrammarFromPennTreeFormattedTextLine(IGrammar grammar, string text)
        {
            GrammarTreeNode tree = ParseTree(text);

            List<ProductionRule> unitProductionRules = new List<ProductionRule>();
            List<ProductionRule> otherProductionRules = new List<ProductionRule>();

            HashSet<string> nonTerminals = new HashSet<string>();
            HashSet<string> terminals = new HashSet<string>();
            HashSet<string> startSymbols = new HashSet<string>();

            ExtractFromParseTree(tree, nonTerminals, terminals, unitProductionRules, otherProductionRules, startSymbols);

            GrammarLexicalizationHelper.Lexicalize(unitProductionRules);
            GrammarLexicalizationHelper.Lexicalize(otherProductionRules);

            foreach (string startSymbol in startSymbols)
            {
                grammar.AddStartSymbol(startSymbol);
            }

            foreach(ProductionRule rule in unitProductionRules)
            {
                grammar.AddProductionRule(rule);
            }
            foreach(ProductionRule rule in otherProductionRules)
            {
                grammar.AddProductionRule(rule);
            }
        }

        public static void ExtractFromParseTree(GrammarTreeNode node, HashSet<string> nonTerminals, HashSet<string> terminals, List<ProductionRule> unitProductionRules, List<ProductionRule> otherProductionRules, HashSet<string> startSymbols)
        {
            if (node == null) return;

            if (node.IsLeaf)
            {
                terminals.Add(node.Symbol);
                return;
            }

            if (node.Parent == null)
            {
                startSymbols.Add(node.Symbol);
            }

            nonTerminals.Add(node.Symbol);
            string[] right = new string[node.ChildCount];
            for (int i = 0; i < node.ChildCount; ++i)
            {
                GrammarTreeNode child = node.GetChild(i);
                if (child.IsLeaf)
                {
                    ProductionRule unitProductionRule = new ProductionRule(true, node.Symbol, child.Symbol);
                    unitProductionRules.Add(unitProductionRule);
                }
                else
                {
                    right[i] = child.Symbol;
                }
            }

            if (right.Length > 1)
            {
                ProductionRule otherRule = new ProductionRule(node.Symbol, right);
                otherProductionRules.Add(otherRule);
            }

            for (int i = 0; i < node.ChildCount; ++i)
            {
                GrammarTreeNode child = node.GetChild(i);
                ExtractFromParseTree(child, nonTerminals, terminals, unitProductionRules, otherProductionRules, startSymbols);
            }
        }

        public static List<string> Tokenize(string text)
        {
            List<string> tokens = new List<string>();
            string[] words = text.Split(new char[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in words)
            {
                int start = -1;
                int end = -1;
                for (int i = 0; i < word.Length; ++i)
                {
                    if (word[i] == '(')
                    {
                        tokens.Add("(");
                        if (start != -1 && end != -1)
                        {
                            tokens.Add(word.Substring(start, end - start + 1));
                        }
                        start = -1;
                        end = -1;
                    }
                    else if (word[i] == ')')
                    {
                        
                        if (start != -1 && end != -1)
                        {
                            tokens.Add(word.Substring(start, end - start + 1));
                        }
                        tokens.Add(")");
                        start = -1;
                        end = -1;
                    }
                    else
                    {
                        if (start == -1)
                        {
                            start = i;
                        }
                        end = i;
                    }
                }

                if (start != -1 && end != -1)
                {
                    tokens.Add(word.Substring(start, end - start + 1));
                }
            }

            return tokens;
        }
    }
}
