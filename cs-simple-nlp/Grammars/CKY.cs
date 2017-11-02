using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MyNLP.Grammars
{
    public class CKY
    {
        public static bool IsInLanguage(string[] words, List<ProductionRule> grammarRules, List<string> grammarNonTerminals, List<string> grammarStartingSymbols)
        {

            List<ProductionRule> unitProductionRules = null;
            List<Tuple<int, int, int>> biOutputProductionRules = null;
            bool isCNFCompatible = false;
            GetProductionRules(grammarRules, grammarNonTerminals, out unitProductionRules, out biOutputProductionRules, out isCNFCompatible);

            if (!isCNFCompatible)
            {
                throw new FormatException("The rule set in the grammar is not CNF compatible");
            }

            int n = words.Length;
            int r = grammarNonTerminals.Count;

            bool[][][] P = new bool[n][][];
            for (int i = 0; i < n; ++i)
            {
                P[i] = new bool[n][];
                for (int j = 0; j < n; ++j)
                {
                    P[i][j] = new bool[r];
                    for (int k = 0; k < r; ++k)
                    {
                        P[i][j][k] = false;
                    }
                }
            }

            for (int i = 0; i < n; ++i)
            {
                string word = words[i];

                foreach (ProductionRule rule in unitProductionRules)
                {
                    string left = rule.Left;
                    string right = rule.Right[0];

                    if (right == word)
                    {
                        int X = grammarNonTerminals.IndexOf(left);

                        P[i][i][X] = true;
                    }
                }
            }

            for (int l = 1; l <= n - 1; ++l) //span length
            {
                for (int i = 0; i < n - l; ++i) //span start
                {
                    int j = i + l; //span end

                    for (int s = i; s <= j - 1; ++s) //split point
                    {
                        for (int k = 0; k < biOutputProductionRules.Count; ++k)
                        {
                            Tuple<int, int, int> rule = biOutputProductionRules[k];
                            int X = rule.Item1;
                            int Y = rule.Item2;
                            int Z = rule.Item3;

                            if (P[i][s][Y] && P[s + 1][j][Z])
                            {
                                P[i][j][X] = true;
                            }
                        }
                    }
                }
            }

            foreach (string s in grammarStartingSymbols)
            {
                int x = grammarNonTerminals.IndexOf(s);
                if (P[0][n - 1][x])
                {
                    return true;
                }
            }

            return false;
        }

        public static GrammarTreeNode ParsePCFG(string[] words, List<ProductionRule> grammarRules, List<string> grammarNonTerminals, List<string> grammarStartingSymbols)
        {
            List<ProductionRule> unitProductionRules = null;
            List<Tuple<int, int, int, ProductionRule>> biOutputProductionRules = null;
            bool isCNFCompatible = false;
            GetProductionRules2(grammarRules, grammarNonTerminals, out unitProductionRules, out biOutputProductionRules, out isCNFCompatible);


            int n = words.Length;
            int r = grammarNonTerminals.Count;

            Tuple<ProductionRule, int>[][][] bp = new Tuple<ProductionRule, int>[n][][];
            double[][][] pi = new double[n][][];
            for (int i = 0; i < n; ++i)
            {
                pi[i] = new double[n][];
                bp[i] = new Tuple<ProductionRule, int>[n][];
                for (int j = 0; j < n; ++j)
                {
                    pi[i][j] = new double[r];
                    bp[i][j] = new Tuple<ProductionRule, int>[r];

                    for (int k = 0; k < r; ++k)
                    {
                        pi[i][j][k] = 0;
                    }
                }
            }

            for (int i = 0; i < n; ++i)
            {
                string word = words[i];
                foreach (ProductionRule rule in unitProductionRules)
                {
                    string left = rule.Left;
                    string right = rule.Right[0];

                    if (right == word)
                    {
                        int X = grammarNonTerminals.IndexOf(left);
                        pi[i][i][X] = rule.Weight;
                        bp[i][i][X] = Tuple.Create(rule, i);
                    }
                }
            }

            for (int l = 1; l <= n - 1; ++l) //span length
            {
                for (int i = 0; i < n - l; ++i) //span start
                {
                    int j = i + l; //span end

                    double[] maxPi = new double[r];
                    ProductionRule[] optRule = new ProductionRule[r];
                    int[] optSplit = new int[r];

                    for (int k = 0; k < r; ++k)
                    {
                        optRule[k] = null;
                        optSplit[k] = -1;
                    }

                    for (int s = i; s <= j - 1; ++s) //split point
                    {
                        for (int k = 0; k < biOutputProductionRules.Count; ++k)
                        {
                            Tuple<int, int, int, ProductionRule> ruleInfo = biOutputProductionRules[k];
                            int X = ruleInfo.Item1;
                            int Y = ruleInfo.Item2;
                            int Z = ruleInfo.Item3;
                            ProductionRule rule = ruleInfo.Item4;
                            double weight = rule.Weight;

                            double candidatePi = weight * pi[i][s][Y] * pi[s + 1][j][Z];
                            if (candidatePi > maxPi[X])
                            {
                                maxPi[X] = candidatePi;
                                optRule[X] = rule;
                                optSplit[X] = s;
                            }
                        }
                    }

                    for (int X = 0; X < r; ++X)
                    {
                        pi[i][j][X] = maxPi[X];
                        bp[i][j][X] = Tuple.Create(optRule[X], optSplit[X]);
                    }
                }
            }

            int optS = -1;
            double maxPiS = 0;
            foreach (string s in grammarStartingSymbols)
            {
                int x = grammarNonTerminals.IndexOf(s);

                double piS = pi[0][n - 1][x];
                if (piS > maxPiS)
                {
                    maxPiS = piS;
                    optS = x;
                }
            }

            if (maxPiS > 0)
            {
                return BuildPCFGTree(grammarNonTerminals, null, pi, bp, 0, n - 1, optS);
            }
            else
            {

                return null;
            }
        }

        private static GrammarTreeNode BuildPCFGTree(List<string> grammarNonTerminals, GrammarTreeNode node, double[][][] pi, Tuple<ProductionRule, int>[][][] bp, int lo, int hi, int X)
        {
            if (lo != hi)
            {
                Tuple<ProductionRule, int> instruction = bp[lo][hi][X];
                double probability = pi[lo][hi][X];
                ProductionRule rule = instruction.Item1;
                int instruction_s = instruction.Item2;

                if (node == null)
                {
                    node = new GrammarTreeNode(grammarNonTerminals[X], false);
                }

                node.Probability = probability;
                List<GrammarTreeNode> children = rule.Fire(node);



                int Y = grammarNonTerminals.IndexOf(children[0].Symbol);
                int Z = grammarNonTerminals.IndexOf(children[1].Symbol);

                node.AddChild(BuildPCFGTree(grammarNonTerminals, children[0], pi, bp, lo, instruction_s, Y));
                node.AddChild(BuildPCFGTree(grammarNonTerminals, children[1], pi, bp, instruction_s + 1, hi, Z));
            }
            else
            {
                Tuple<ProductionRule, int> instruction = bp[lo][hi][X];
                double probability = pi[lo][hi][X];
                ProductionRule rule = instruction.Item1;

                node.Probability = probability;
                List<GrammarTreeNode> children = rule.Fire(node);

                Debug.Assert(children.Count == 1);

                node.AddChild(children[0]);
            }

            return node;
        }

        private static void GetProductionRules(List<ProductionRule> grammarRules, List<string> grammarNonTerminals, out List<ProductionRule> unitProductionRules, out List<Tuple<int, int, int>> matchedRules, out bool isCNFCompatible)
        {
            unitProductionRules = new List<ProductionRule>();
            matchedRules = new List<Tuple<int, int, int>>();
            isCNFCompatible = true;

            foreach (ProductionRule rule in grammarRules)
            {
                if (rule.IsUnitProductionRule)
                {
                    unitProductionRules.Add(rule);
                }
                else if (rule.IsBiOutputProductionRule)
                {
                    int X = grammarNonTerminals.IndexOf(rule.Left);
                    int Y = grammarNonTerminals.IndexOf(rule.Right[0]);
                    int Z = grammarNonTerminals.IndexOf(rule.Right[1]);

                    matchedRules.Add(Tuple.Create(X, Y, Z));
                }
                else
                {
                    isCNFCompatible = false;
                }
            }
        }

        private static void GetProductionRules2(List<ProductionRule> grammarRules, List<string> grammarNonTerminals, out List<ProductionRule> unitProductionRules, out List<Tuple<int, int, int, ProductionRule>> matchedRules, out bool isCNFCompatible)
        {
            unitProductionRules = new List<ProductionRule>();
            matchedRules = new List<Tuple<int, int, int, ProductionRule>>();
            isCNFCompatible = true;

            foreach (ProductionRule rule in grammarRules)
            {
                if (rule.IsUnitProductionRule)
                {
                    unitProductionRules.Add(rule);
                }
                else if (rule.IsBiOutputProductionRule)
                {
                    int X = grammarNonTerminals.IndexOf(rule.Left);
                    int Y = grammarNonTerminals.IndexOf(rule.Right[0]);
                    int Z = grammarNonTerminals.IndexOf(rule.Right[1]);

                    matchedRules.Add(Tuple.Create(X, Y, Z, rule));
                }
                else
                {
                    isCNFCompatible = false;
                }
            }
        }

        public static List<GrammarTreeNode> ParseCFG(string[] words, List<ProductionRule> grammarRules, List<string> grammarNonTerminals, List<string> grammarStartingSymbols)
        {
            List<ProductionRule> unitProductionRules = null;
            List<Tuple<int, int, int, ProductionRule>> biOutputProductionRules = null;
            bool isCNFCompatible = false;

            GetProductionRules2(grammarRules, grammarNonTerminals, out unitProductionRules, out biOutputProductionRules, out isCNFCompatible);

            if (!isCNFCompatible)
            {
                throw new FormatException("The rule set in the grammar is not CNF compatible");
            }

            int n = words.Length;
            int r = grammarNonTerminals.Count;
            bool[][][] P = new bool[n][][];
            Tuple<List<ProductionRule>, int>[][][] bp = new Tuple<List<ProductionRule>, int>[n][][];
            for (int i = 0; i < n; ++i)
            {
                P[i] = new bool[n][];
                bp[i] = new Tuple<List<ProductionRule>, int>[n][];
                for (int j = 0; j < n; ++j)
                {
                    P[i][j] = new bool[r];
                    bp[i][j] = new Tuple<List<ProductionRule>, int>[r];
                    for (int k = 0; k < r; ++k)
                    {
                        P[i][j][k] = false;
                    }
                }
            }

            for (int i = 0; i < n; ++i)
            {
                string word = words[i];

                foreach (ProductionRule rule in unitProductionRules)
                {
                    string left = rule.Left;
                    string right = rule.Right[0];

                    if (right == word)
                    {
                        int X = grammarNonTerminals.IndexOf(left);

                        P[i][i][X] = true;
                        bp[i][i][X] = Tuple.Create(new List<ProductionRule>() { rule }, i);
                    }
                }
            }

            for (int l = 1; l <= n - 1; ++l) //span length
            {
                for (int i = 0; i < n - l; ++i) //span start
                {
                    int j = i + l; //span end

                    int[] optSplit = new int[r];
                    List<ProductionRule>[] optRule = new List<ProductionRule>[r];

                    for (int X = 0; X < r; ++X)
                    {
                        optSplit[X] = -1;
                        optRule[X] = new List<ProductionRule>();
                    }

                    for (int s = i; s <= j - 1; ++s) //split point
                    {
                        foreach (Tuple<int, int, int, ProductionRule> ruleInfo in biOutputProductionRules)
                        {
                            int X = ruleInfo.Item1;
                            int Y = ruleInfo.Item2;
                            int Z = ruleInfo.Item3;

                            if (P[i][s][Y] && P[s + 1][j][Z])
                            {
                                P[i][j][X] = true;
                                optRule[X].Add(ruleInfo.Item4);
                            }
                        }
                    }

                    for (int X = 0; X < r; ++X)
                    {
                        bp[i][j][X] = Tuple.Create(optRule[X], optSplit[X]);
                    }
                }
            }

            List<GrammarTreeNode> trees = new List<GrammarTreeNode>();
            foreach (string s in grammarStartingSymbols)
            {
                int x = grammarNonTerminals.IndexOf(s);
                if (P[0][n - 1][x])
                {
                    BuildCFGTree(grammarNonTerminals, null, trees, P, bp, 0, n - 1, x);
                }
            }

            return trees;
        }

        private static void BuildCFGTree(List<string> grammarNonTerminals, GrammarTreeNode node, List<GrammarTreeNode> trees, bool[][][] P, Tuple<List<ProductionRule>, int>[][][] bp, int lo, int hi, int s)
        {
            Tuple<List<ProductionRule>, int> instruction = bp[lo][hi][s];
            List<ProductionRule> rules = instruction.Item1;
            int splitPoint = instruction.Item2;

            if (node == null)
            {
                node = new GrammarTreeNode(grammarNonTerminals[s], false);
                trees.Add(node);
            }

            ProductionRule rule = rules[0]; //only use the first rule for simple implementation

            List<GrammarTreeNode> children = rule.Fire(node);

            if (rule.IsUnitProductionRule)
            {
                node.AddChild(children[0]);
            }
            else
            {
                node.AddChild(children[0]);
                node.AddChild(children[1]);

                int Y = grammarNonTerminals.IndexOf(children[0].Symbol);
                int Z = grammarNonTerminals.IndexOf(children[1].Symbol);

                BuildCFGTree(grammarNonTerminals, children[0], trees, P, bp, lo, splitPoint, Y);
                BuildCFGTree(grammarNonTerminals, children[1], trees, P, bp, splitPoint + 1, hi, Z);
            }
        }

      
    }
}
