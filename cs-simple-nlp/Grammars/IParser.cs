using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNLP.Grammars
{
    public interface IParser
    {
        bool IsInLanguage(string sentence);

        GrammarTreeNode Parse(string sentence);
    }
}
