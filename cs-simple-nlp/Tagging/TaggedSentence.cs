using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyNLP.Tokenizers;
using MyNLP.LanguageModels;
using MyNLP.Grammars;

namespace MyNLP.Tagging
{
    public class TaggedSentence : List<PairedToken>
    {
        public TaggedSentence(IEnumerable<PairedToken> data)
        {
            this.AddRange(data);
        }

        public TaggedSentence()
        {
        }

        public static List<TaggedSentence> ConvertToTaggedSentences(IList<GrammarTreeNode> tree_bank)
        {
            List<TaggedSentence> tagged_sentences = new List<TaggedSentence>();

            for (int i = 0; i < tree_bank.Count; ++i)
            {
                GrammarTreeNode tree = tree_bank[i];
                TaggedSentence tagged_sentence = ConvertToTaggedSentence(tree);
                tagged_sentences.Add(tagged_sentence);
            }

            return tagged_sentences;
        }

        public static List<TaggedSentence> ConvertToChunkedSentences(IList<GrammarTreeNode> tree_bank)
        {
            List<TaggedSentence> chunked_sentences = new List<TaggedSentence>();

            for (int i = 0; i < tree_bank.Count; ++i)
            {
                GrammarTreeNode tree = tree_bank[i];
                TaggedSentence chunked_sentence = ConvertToChunkedSentence(tree);
                chunked_sentences.Add(chunked_sentence);
            }

            return chunked_sentences;
        }

        public static TaggedSentence ConvertToTaggedSentence(GrammarTreeNode tree)
        {
            List<GrammarTreeNode> leaf_nodes = tree.CollectLeafNodes();

            TaggedSentence tagged_sentence = new TaggedSentence();

            for (int j = 0; j < leaf_nodes.Count; ++j)
            {
                GrammarTreeNode leaf_node = leaf_nodes[j];
                GrammarTreeNode parent_node = leaf_node.Parent;

                string tag = parent_node.Symbol;
                string word = leaf_node.Symbol;

                PairedToken wtpair = new PairedToken()
                {
                    Word = word,
                    Tag = tag
                };

                tagged_sentence.Add(wtpair);
            }

            return tagged_sentence;
        }

        public static TaggedSentence ConvertToChunkedSentence(GrammarTreeNode tree)
        {
            List<GrammarTreeNode> leaf_nodes = tree.CollectLeafNodes();

            TaggedSentence tagged_sentence = new TaggedSentence();

            
            for (int j = 0; j < leaf_nodes.Count; ++j)
            {
                GrammarTreeNode leaf_node = leaf_nodes[j];
                GrammarTreeNode tag_node = leaf_node.Parent;
                GrammarTreeNode chunk_node = tag_node.Parent;

                string tag = tag_node.Symbol;
                string chunk = chunk_node.Symbol;

                PairedToken wtpair = new PairedToken()
                {
                    Word = tag,
                    Tag = chunk
                };

                tagged_sentence.Add(wtpair);
            }

            return tagged_sentence;
        }

        public static List<TaggedSentence> ConvertToTaggedSentences(IList<string> input_sentences)
        {
            List<TaggedSentence> output_sentences = new List<TaggedSentence>();

            int N = input_sentences.Count;
            for (int i = 0; i < N; ++i)
            {
                output_sentences.Add(ConvertToTaggedSentence(input_sentences[i]));
            }

            return output_sentences;
        }

        public static TaggedSentence ConvertToTaggedSentence(string sentence)
        {
            PairedTokenizer tokenizer = new PairedTokenizer();
            List<PairedToken> tokens = tokenizer.Tokenize(sentence);
            TaggedSentence output_sentence = new TaggedSentence(tokens);

            return output_sentence;
        }

        public static List<PairedDataInstance<HistoryInstance, string>> ConvertToTaggedHistoryInstances(IList<TaggedSentence> tagged_sentences)
        {
            List<PairedDataInstance<HistoryInstance, string>> training_data = new List<PairedDataInstance<HistoryInstance, string>>();

            int N = tagged_sentences.Count;
            for (int i = 0; i < N; ++i)
            {
                TaggedSentence tagged_sentence = tagged_sentences[i];

                string[] sentence = new string[tagged_sentence.Count];
                for (int j = 0; j < tagged_sentence.Count; ++j)
                {
                    PairedToken pair = tagged_sentence[j];
                    sentence[j] = pair.Word;
                }

                for (int j = 0; j < tagged_sentence.Count; ++j)
                {
                    HistoryInstance input = new HistoryInstance();
                    input.w_im2 = j > 1 ? sentence[j - 2] : "";
                    input.w_im1 = j > 0 ? sentence[j - 1] : "";
                    input.w_i = sentence[j];
                    input.w_ip1 = j < tagged_sentence.Count - 1 ? sentence[j + 1] : "";
                    input.w_ip2 = j < tagged_sentence.Count - 2 ? sentence[j + 2] : "";
                    input.tag_im1 = j > 0 ? tagged_sentence[j - 1].Tag : "";
                    input.tag_im2 = j > 1 ? tagged_sentence[j - 2].Tag : "";

                    PairedDataInstance<HistoryInstance, string> rec = new PairedDataInstance<HistoryInstance, string>();
                    rec.Entry = input;
                    rec.Label = tagged_sentence[j].Tag;
                    training_data.Add(rec);
                }

            }

            return training_data;
        }

        public string[] Words
        {
            get
            {
                string[] words = new string[this.Count];
                for (int i = 0; i < Count; ++i)
                {
                    words[i] = this[i].Word;
                }
                return words;
            }
        }

        public string[] Tags
        {
            get
            {
                string[] tags = new string[Count];
                for (int i = 0; i < Count; ++i)
                {
                    tags[i] = this[i].Tag;
                }

                return tags;
            }
        }


    }
}
