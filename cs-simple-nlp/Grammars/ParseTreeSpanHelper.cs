using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.Grammars
{
    public class ParseTreeSpanHelper
    {
        public static void GetTreeSpanTable(GrammarTreeNode tree, out Dictionary<GrammarTreeNode, Span> table)
        {
            int d = 0;
            table = new Dictionary<GrammarTreeNode, Span>();
            GetSpanAtLeaf(tree, ref d, table);
            GetSpanAtBranch(tree, table);
        }

        public static void GetTreeSpanTable(GrammarTreeNode tree, out List<Span> table)
        {
            table = new List<Span>();

            int d = 0;
            Dictionary<GrammarTreeNode, Span> table2 = new Dictionary<GrammarTreeNode, Span>();
            GetSpanAtLeaf(tree, ref d, table2);
            GetSpanAtBranch(tree, table2);

            foreach(GrammarTreeNode node in table2.Keys)
            {
                string nodeName = node.Symbol;
                Span span = table2[node];
                span.Tag = nodeName;
                table.Add(span);
            }
        }

        public static void RemoveLeafNode(Dictionary<GrammarTreeNode, Span> table)
        {
            List<GrammarTreeNode> nodes = table.Keys.ToList();
            foreach (GrammarTreeNode node in nodes)
            {
                if (table[node].IsSingular)
                {
                    table.Remove(node);
                }
            }
        }

        public static void RemoveLeafNode(List<Span> table)
        {   
            for(int i=table.Count-1; i >=0; --i)
            {
                Span span = table[i];
                if (span.IsSingular)
                {
                    table.Remove(span);
                }
            }
        }

        private static void GetSpanAtLeaf(GrammarTreeNode node, ref int d, Dictionary<GrammarTreeNode, Span> table)
        {
            if (node.IsLeaf)
            {
                d++;
                table[node] = new Span { Start = d, End = d };
                return;
            }

            for (int i = 0; i < node.ChildCount; ++i)
            {
                GetSpanAtLeaf(node.GetChild(i), ref d, table);
            }
        }

        private static Span GetSpanAtBranch(GrammarTreeNode node, Dictionary<GrammarTreeNode, Span> table)
        {
            if (node.IsLeaf)
            {
                return table[node];
            }
            else
            {
                Span span = new Span { Start = int.MaxValue, End = int.MinValue };
                for (int i = 0; i < node.ChildCount; ++i)
                {
                    Span childSpan = GetSpanAtBranch(node.GetChild(i), table);
                    if (childSpan.Start < span.Start)
                    {
                        span.Start = childSpan.Start;
                    }
                    if (childSpan.End > span.End)
                    {
                        span.End = childSpan.End;
                    }
                }
                table[node] = span;

                return span;
            }
        }
    }
}
