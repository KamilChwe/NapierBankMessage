using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NapierBankMessage
{
    class Mention
    {
        public string mention { get; set; }

        public Mention(string mentionIn)
        {
            mention = mentionIn;
        }
    }

    class MentionsList
    {
        public List<Mention> mentionsList { get; set; }
    }
}
