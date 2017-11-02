using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.Grammars
{
    public class ParseTreeEvaluator
    {
        public static double Evaluate(GrammarTreeNode predicted_tree, GrammarTreeNode expected_tree, out double precision, out double recall)
        {
            int G, P, C;
            return Evaluate(predicted_tree, expected_tree, out G, out P, out C, out precision, out recall);
        }

        public static double Evaluate(GrammarTreeNode predicted_tree, GrammarTreeNode expected_tree, out int G, out int P, out int C, out double precision, out double recall)
        {
            Dictionary<GrammarTreeNode, Span> expected_tree_table = null;
            ParseTreeSpanHelper.GetTreeSpanTable(expected_tree, out expected_tree_table);

            return Evaluate(predicted_tree, expected_tree_table, out G, out P, out C, out precision, out recall);
        }

        public static double Evaluate(GrammarTreeNode predicted_tree, Dictionary<GrammarTreeNode, Span> expected_tree_table, out double precision, out double recall)
        {
            int G, P, C;
            return Evaluate(predicted_tree, expected_tree_table, out G, out P, out C, out precision, out recall);
        }

        public static double Evaluate(GrammarTreeNode predicted_tree,  Dictionary<GrammarTreeNode, Span> expected_tree_table, out int G, out int P, out int C, out double precision, out double recall)
        {
            Dictionary<GrammarTreeNode, Span> predicted_tree_table = null;
            ParseTreeSpanHelper.GetTreeSpanTable(predicted_tree, out predicted_tree_table);

            ParseTreeSpanHelper.RemoveLeafNode(predicted_tree_table);
            ParseTreeSpanHelper.RemoveLeafNode(expected_tree_table);

            G = expected_tree_table.Count;
            P = predicted_tree_table.Count;
            C = CalcNumberCorrectConstituents(predicted_tree_table, expected_tree_table);

            recall = 100.0 * C / G;
            precision = 100.0 * C / P;

            if (precision + recall > 0)
            {
                double F1_score = 2 * (precision * recall) / (precision + recall);
                return F1_score;
            }
            else
            {
                return -1;
            }
        }

        public static double Evaluate(GrammarTreeNode predicted_tree, List<Span> expected_tree_table, out double precision, out double recall)
        {
            int G, P, C;
            return Evaluate(predicted_tree, expected_tree_table, out G, out P, out C, out precision, out recall);
        }

        public static double Evaluate(GrammarTreeNode predicted_tree, List<Span> expected_tree_table, out int G, out int P, out int C, out double precision, out double recall)
        {
            Dictionary<GrammarTreeNode, Span> predicted_tree_table = null;
            ParseTreeSpanHelper.GetTreeSpanTable(predicted_tree, out predicted_tree_table);

            ParseTreeSpanHelper.RemoveLeafNode(predicted_tree_table);
            ParseTreeSpanHelper.RemoveLeafNode(expected_tree_table);

            G = expected_tree_table.Count;
            P = predicted_tree_table.Count;
            C = CalcNumberCorrectConstituents(predicted_tree_table, expected_tree_table);

            recall = 100.0 * C / G;
            precision = 100.0 * C / P;

            if (precision + recall > 0)
            {
                double F1_score = 2 * (precision * recall) / (precision + recall);
                return F1_score;
            }
            else
            {
                return -1;
            }
        }

        public static int CalcNumberCorrectConstituents(Dictionary<GrammarTreeNode, Span> predicted_tree_table, List<Span> expected_tree_table)
        {
            int correctCount = 0;
            foreach (GrammarTreeNode node in predicted_tree_table.Keys)
            {
                Span predictedSpan = predicted_tree_table[node];
                string nodeName = node.Symbol;
                foreach (Span expectedSpan in expected_tree_table)
                {
                    string nodeName2 = expectedSpan.Tag as string;
                    if (nodeName == nodeName2)
                    {
                        if (expectedSpan.Equals(predictedSpan))
                        {
                            correctCount++;
                            break;
                        }
                    }
                }
            }

            return correctCount;
        }

        public static int CalcNumberCorrectConstituents(Dictionary<GrammarTreeNode, Span> predicted_tree_table, Dictionary<GrammarTreeNode, Span> expected_tree_table)
        {
            int correctCount = 0;
            foreach (GrammarTreeNode node in predicted_tree_table.Keys)
            {
                Span predictedSpan = predicted_tree_table[node];
                string nodeName = node.Symbol;
                foreach (GrammarTreeNode node2 in expected_tree_table.Keys)
                {
                    string nodeName2 = node2.Symbol;
                    if (nodeName == nodeName2)
                    {
                        Span expectedSpan = expected_tree_table[node2];
                        if(expectedSpan.Equals(predictedSpan))
                        {
                            correctCount++;
                            break;
                        }
                    }
                }
            }

            return correctCount;
        }
    }
}
