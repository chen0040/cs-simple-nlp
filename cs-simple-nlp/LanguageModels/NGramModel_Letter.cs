using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.LanguageModels
{
    public class NGramModel_Letter : NGramModel<char>
    {
        public NGramModel_Letter(int N)
            : base(N)
        {
        }

        public void Parse4Gram(string word)
        {
            char[] chars = word.ToCharArray();
            Parse4Gram(chars);
        }
    }
}
