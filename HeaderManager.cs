using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NapierBankMessage
{
    class HeaderManager
    {
        public string DetectType(string header)
        {
            string messageType = "None";

            // Check if the header (Message ID) contains these letters
            if(header.Contains("S"))
            {
                messageType = "SMS";
            }
            else if(header.Contains("E"))
            {
                messageType = "EMail";
            }
            else if(header.Contains("T"))
            {
                messageType = "Tweet";
            }
            // If it doesn't contain any of these letters than set messageType to none 
            else
            {
                messageType = "None";
            }
            return messageType;
        }
    }
}
