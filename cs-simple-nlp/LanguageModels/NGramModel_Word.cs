using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyNLP.Helpers;

namespace MyNLP.LanguageModels
{
    public class NGramModel_Word : NGramModel<string>
    {
        public NGramModel_Word(int N)
            : base(N)
        {
        }

        public void Parse4Gram(string paragraph)
        {
            Parse4Gram(paragraph, Constants.DefaultSplitChars);
        }

        public void Parse4Gram(string paragraph, char[] split_chars)
        {
            string[] words = paragraph.Split(split_chars, StringSplitOptions.RemoveEmptyEntries);
            Parse4Gram(words);
        }

        public void Parse4Gram(ITokenizer tokenizer)
        {
            string[] words = tokenizer.Tokens;
            Parse4Gram(words);
        }
    }
}
