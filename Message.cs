using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NapierBankMessage
{
    public class Message
    {
        public string Header { get; set; }
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public Message(string headerIn, string senderIn, string subjectIn, string bodyIn)
        {
            Header = headerIn;
            Sender = senderIn;
            Subject = subjectIn;
            Body = bodyIn;
        }
    }

    public class MessageList
    {
        public List<Message> Messages { get; set; }
    }
}
