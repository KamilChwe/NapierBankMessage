using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NapierBankMessage
{
    public class Hashtag
    {
        public string hashtag { get; set; }
        public int hashtagCount { get; set; }

        public Hashtag(string hashIn, int countIn)
        {
            hashtag = hashIn;
            hashtagCount = countIn;
        }
    }

    public class HashList
    {
        public List<Hashtag> Hashtags { get; set; }
    }
}
