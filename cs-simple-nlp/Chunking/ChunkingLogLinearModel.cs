using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyNLP.Tagging;
using MyNLP.LanguageModels;
using MyNLP.Grammars;

namespace MyNLP.Chunking
{
    public class ChunkingLogLinearModel 
    {
        protected TaggingLogLinearModel mChunkingModel = new TaggingLogLinearModel();

        public TaggingLogLinearModel ChunkingModel
        {
            get { return mChunkingModel; }
        }

        public void Train(IList<GrammarTreeNode> tree_bank)
        {
            List<TaggedSentence> chunked_sentence = TaggedSentence.ConvertToChunkedSentences(tree_bank);
            mChunkingModel.Train(chunked_sentence);
        }

        public string[] FindChunks(string[] sentence)
        {
            return mChunkingModel.FindTags(sentence);
        }
    }
}
