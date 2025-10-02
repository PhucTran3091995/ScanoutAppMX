using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSFC.Service.Translate
{
    public class TranslationConfig
    {
        public Dictionary<string, string> Translations { get; set; }
        public Dictionary<string, TranslationGuide> Guides { get; set; }

    }
    public class TranslationGuide
    {
        public string Explain_Es { get; set; }

    }
}
