using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NapierBankMessage
{
    public class BlockedURL
    {
        public string url { get; set; }

        public BlockedURL(string urlIn)
        {
            url = urlIn;
        }
    }

    public class URLList
    {
        public List<BlockedURL> URLs { get; set; }
    }
}
