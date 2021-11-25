using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Windows;

namespace NapierBankMessage
{
    // This class is responsible for all of the Body (Message) processing
    class MessageManager
    {
        // Setting up Global Vars
        string header;
        string sender;
        string subject;
        string body;
        string messageType;
        bool SIR;

        // Basically acts like a "Master" functions, gets all the variables in place
        public void StartProcessing(string messageTypeIn, string headerIn, string bodyIn)
        {
            messageType = messageTypeIn;
            header = headerIn;
            body = bodyIn;

            FindDetails();
        }
        

        public void FindDetails()
        {
            #region RegEx
            // Using RegEx to find the details in the body

            // Regex for finding an international phone number (Example: +44 1234567890)
            Regex smsRegex = new Regex(@"\+((?:9[679]|8[035789]|6[789]|5[90]|42|3[578]|2[1-689])|9[0-58]|8[1246]|6[0-6]|5[1-8]|4[013-9]|3[0-469]|2[70]|7|1)(?:\W*\d){0,13}\d");
            // Regex for finding Twitter Handle (Example: @TestUser)
            Regex twitterRegex = new Regex(@"@([\w]{1,15})");
            // Regex for Hashtags (Example: #SoCool)
            Regex hashtagRegex = new Regex(@"#([\w]+)");
            // Regex for finding an Email Address (Example: TestUser@gmail.com)
            Regex emailRegex = new Regex(@"([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)");
            // Regex for finding Links (Example: www.youtube.com)
            Regex urlRegex = new Regex(@"www\.([\w]+)\.([\w]{2,3})");

            // Getting the matches for Regex
            Match smsMatch = smsRegex.Match(body);

            // Tweets may contain multiple handles so we need a collection of them
            MatchCollection twitterMatches = twitterRegex.Matches(body);
            // Same goes for Hashtags
            MatchCollection hashtagMatches = hashtagRegex.Matches(body);

            Match emailMatch = emailRegex.Match(body);

            MatchCollection urlMatches = urlRegex.Matches(body);
            #endregion

            // Preparing the Header, Sender, Subject, and Body
            switch (messageType)
            {
                case "SMS":
                    MessageBox.Show("Found a phone number: " + smsMatch.Value);
                    sender = smsMatch.Value;
                    break;

                case "EMail":
                    // Checks if the EMail is a SIR email or SEM
                    if (body.Contains("Sort Code"))
                    {
                        SIR = true;
                        MessageBox.Show("This is a SIR email");
                    }
                    else
                    {
                        SIR = false;
                        MessageBox.Show("This is a SEM email");
                    }
                    MessageBox.Show("Found an email address: " + emailMatch.Value);
                    foreach (Match match in urlMatches)
                    {
                        MessageBox.Show("Found an URL: " + match.Value);
                        //AddURLs(match.Value);
                    }
                    sender = emailMatch.Value;
                    break;

                case "Tweet":
                    for (int i = 0; i < twitterMatches.Count; i++)
                    {
                        if (i == 0)
                        {
                            MessageBox.Show("Found the Twitter Handle (Sender): " + twitterMatches[i]);
                            sender = twitterMatches[i].ToString();
                        }
                        else
                        {
                            MessageBox.Show("Found the Twitter Handle (Mention): " + twitterMatches[i]);
                        }
                    }
                    foreach (Match match in hashtagMatches)
                    {

                    }
                    break;
            }
            AddMentions(twitterMatches);
        }

        public void ConvertAbbreviations()
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };

            using (var reader = new StreamReader(@"textwords.csv"))
            {
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<TextWords>().ToList();
                    
                    for(int i = 0; i < 255; i++)
                    {
                        var abr = records[i].shortVer;
                        if (body.ToUpper().Contains(abr))
                        {
                            string longVer = "<" + records[i].longVer + ">";
                            string processedMessage = abr + " " + longVer;
                            StringBuilder builder = new StringBuilder(body);
                            builder.Replace(abr, processedMessage);
                            body = builder.ToString();
                        }
                    }
                }
            }
        }

        public void AddMentions(MatchCollection mentions)
        {
            // First we have to find the mentions in the Body

            // We can assume that the first mention we find is the Sender

            // Check if the Mentions JSON already exists
            if (File.Exists("Mentions.json"))
            {
                MentionsList mentList = JsonConvert.DeserializeObject<MentionsList>(File.ReadAllText("Mentions.json"));

                // Go through each Mention the Tweet Body
                foreach(var Mention in mentions)
                {
                    // Create an entry in a JSON file
                    Mention newMention = new Mention(Mention.ToString());
                    mentList.mentionsList.Add(newMention);

                    // Write the mention to the JSON
                    File.WriteAllText("Mentions.json", JsonConvert.SerializeObject(mentList, Formatting.Indented) + Environment.NewLine);
                }
            }
            // Otherwise if it doesn't exist then make a new one
            else
            {
                File.WriteAllText("Mentions.json", "{\"mentionsList\": []}");
                MentionsList mentList = JsonConvert.DeserializeObject<MentionsList>(File.ReadAllText("Mentions.json"));

                // Go through each Mention the Tweet Body
                foreach (var Mention in mentions)
                {
                    // Create an entry in a JSON file
                    Mention newMention = new Mention(Mention.ToString());
                    mentList.mentionsList.Add(newMention);

                    // Write the mention to the JSON
                    File.WriteAllText("Mentions.json", JsonConvert.SerializeObject(mentList, Formatting.Indented) + Environment.NewLine);
                }
            }
        }

        public void AddHashtags()
        {

        }

        public void AddURLs()
        {

        }

        public void AddMessages()
        {
            
        }
    }
}
